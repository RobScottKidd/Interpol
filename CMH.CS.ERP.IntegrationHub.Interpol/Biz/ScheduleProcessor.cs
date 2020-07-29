using CMH.CS.ERP.IntegrationHub.Interpol.Interfaces;
using CMH.CS.ERP.IntegrationHub.Interpol.Interfaces.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CMH.CS.ERP.IntegrationHub.Interpol.Biz
{
    /// <summary>
    /// Implementation of the IScheduler interface
    /// </summary>
    public sealed class ScheduleProcessor : IScheduleProcessor
    {
        private readonly ILogger<ScheduleProcessor> _logger;
        private readonly IDictionary<Timer, SchedulerTimerState> _runningTimers;
        private readonly IInterpolConfiguration _config;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly IInstanceKeyProvider _instanceKeyProvider;
        private CancellationToken token;
        private readonly ISchedulerTaskFactory _taskFactory;
        private readonly IDataCache _dataCache;
        private readonly SemaphoreSlim _taskRunning;

        /// <summary>
        /// Ctor sets up the timers for each schedule
        /// </summary>
        /// <param name="config"></param>
        public ScheduleProcessor(
            ILogger<ScheduleProcessor> logger,
            IInterpolConfiguration config,
            IDateTimeProvider dateTimeProvider,
            ISchedulerTaskFactory taskFactory,
            IInstanceKeyProvider instanceKeyProvider,
            IDataCache dataCache
        ) {
            _config = config;
            _dateTimeProvider = dateTimeProvider;
            _runningTimers = new Dictionary<Timer, SchedulerTimerState>();
            _logger = logger;
            _instanceKeyProvider = instanceKeyProvider;
            _taskFactory = taskFactory;
            _dataCache = dataCache;
            _taskRunning = new SemaphoreSlim(1, 1);
        }

        /// <summary>
        /// Starts the timers for the schedule processor
        /// </summary>
        /// <param name="token"></param>
        public void Start(CancellationToken token)
        {
            // if this setting is set, don't even start the application
            if (_config.CompletelyDisableInterpol)
            {
                _logger.LogInformation($"Interpol is currently configured NOT to start: CompletelyDisableInterpol = { _config.CompletelyDisableInterpol }");
                return;
            }

            this.token = token;
            var scheduleBlocks = _config.Schedules[_instanceKeyProvider.InstanceKey.ToString()];
            foreach (var schedule in scheduleBlocks)
            {
                foreach (var businessUnit in schedule.BusinessUnits)
                {
                    foreach (var dataType in schedule.DataTypes)
                    {
                        var matchingBusinessUnit = _dataCache.BusinessUnits().FirstOrDefault(bu =>
                            bu.BUAbbreviation.ToLower() == businessUnit?.ToLower())
                            ?? throw new Exception($"Business Unit {businessUnit} specified in Interpol Configuration is invalid");
                        var timerState = new SchedulerTimerState()
                        {
                            SchedulerTask = _taskFactory.GetSchedulerTask(
                                dataType,
                                matchingBusinessUnit),

                            Configuration = schedule
                        };

                        var timer = new Timer(new TimerCallback(TimerTick), timerState, Timeout.Infinite, schedule.PollingIntervalMilliseconds.Value);
                        timerState.SchedulerTimer = timer;

                        _runningTimers.Add(timer, timerState);
                    }
                }
            }

            foreach (var keyValue in _runningTimers)
            {
                // this sets all the timers start running at the provided interval
                keyValue.Key.Change(0, keyValue.Value.Configuration.PollingIntervalMilliseconds.Value);
            }
        }

        /// <summary>
        /// Gracefully stops all the timers and disposes of them
        /// </summary>
        public void Stop()
        {
            // cleanup timers
            foreach (var timerState in _runningTimers.Values)
            {
                timerState.SchedulerTimer.Change(Timeout.Infinite, 0);
                timerState.SchedulerTimer.Dispose();
            }

            _runningTimers.Clear();
        }

        /// <summary>
        /// Callback executed on the interval tick for each schedule configuration
        /// Runs the schedule task if applicable
        /// </summary>
        /// <param name="state"></param>
        private async void TimerTick(object state)
        {
            if (state is SchedulerTimerState timerState && ShouldRunTimerNow(timerState.Configuration))
            {
                try
                {
                    // todo: I'm leaving this in here commented out because it is much easier to debug in single threaded mode
                    if (!_config.UseMultithreaded)
                    {       
                        //<-- Eventually we may want to take this out and allow the application to run in multi-threaded mode, but not until some other issues are resolved.
                        await _taskRunning.WaitAsync();
                    }
                    await timerState.SchedulerTask.Run(token, _config.PollRetryCount.Value, _config.PollRetryDelay.Value);
                }
                catch (TaskCanceledException e)
                {
                    _logger.LogInformation($"Giving up trying to run scheduled task for { timerState.SchedulerTask.BusinessUnit.BUAbbreviation }, { timerState.SchedulerTask.DataType }", e);
                }
                catch (Exception e)
                {
                    _logger.LogError(e, $"Unexpected exception occured while trying to run scheduled task for { timerState.SchedulerTask.BusinessUnit }, { timerState.SchedulerTask.DataType }");
                }
                finally
                {
                    if (_taskRunning.CurrentCount == 0)
                    {
                        _taskRunning.Release(1);
                    }
                }
            }
        }

        /// <summary>
        /// Indicates if the scheduled task should be run now
        /// </summary>
        /// <param name="config"></param>
        /// <returns>true if current time in range for schedule config, false otherwise</returns>
        public bool ShouldRunTimerNow(IScheduleConfiguration config)
        {
            var now = _dateTimeProvider.CurrentTime;
            bool shouldRunStart = now.TimeOfDay >= (config.StartTime ?? new TimeSpan());
            bool shouldRunEnd = now.TimeOfDay <= (config.EndTime ?? new TimeSpan(23, 59, 59));
            bool shouldRunDay = (config.DaysOfWeek?.Contains(now.DayOfWeek) ?? true);

            return shouldRunStart && shouldRunEnd && shouldRunDay;
        }
    }
}