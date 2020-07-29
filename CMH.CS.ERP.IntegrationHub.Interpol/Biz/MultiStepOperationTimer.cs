using CMH.CS.ERP.IntegrationHub.Interpol.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CMH.CS.ERP.IntegrationHub.Interpol.Biz
{
    /// <summary>
    /// class that collects timed events for eventual logging
    /// </summary>
    public class MultiStepOperationTimer : IMultistepOperationTimer
    {
        private readonly ILogger<IMultistepOperationTimer> _logger;
        private readonly IDictionary<Guid, ThreadableStopWatch> _sws;
        private readonly SemaphoreSlim _calling;
        
        /// <summary>
        /// constructor
        /// </summary>
        public MultiStepOperationTimer(ILogger<IMultistepOperationTimer> logger)
        {
            _logger = logger;
            _sws = new ConcurrentDictionary<Guid, ThreadableStopWatch>();
            _calling = new SemaphoreSlim(1, 1);
        }

        private Guid Start(Guid id, string name, string datatype, string businessunit, TimeSpan criticalAlertThreshold)
        {
            var thread = new ThreadableStopWatch() 
            {
                Id = Guid.NewGuid(),
                ThreadID = id,
                TaskName = name,
                DataType = datatype,
                BusinessUnit = businessunit,
                CriticalAlertThreshold = criticalAlertThreshold
            };

            thread.Timer = new Timer(new TimerCallback(TimerTick), thread, 0, 500);
            
            _sws.Add(thread.Id, thread);

            return thread.Id;
        }

        public ThreadableStopWatch Stop(Guid processID)
        {
            var timerState = _sws[processID];

            timerState.Timer.Change(Timeout.Infinite, 0);

            return timerState;
        }

        private void TimerTick(object state)
        {
            if (state is ThreadableStopWatch timerState)
            {
                timerState.ElapsedTime += TimeSpan.FromMilliseconds(500); 
                    
                if (TimeSpan.Compare(timerState.ElapsedTime, timerState.CriticalAlertThreshold) == 0)
                {
                    var timeElapsed = timerState.ElapsedTime;
                    var taskName = timerState.TaskName;

                    _logger.LogError(
                        new CallTimeoutException($"A call to Oracle for { taskName } exceed the alert time. Execution time { timeElapsed }"),
                        "An Oracle process may be hanging up"
                    );
                }

                if (_calling.CurrentCount == 0)
                {
                    _calling.Release(1);
                }
            }
        }

        /// <summary>
        /// Runs the provided method in wrapped in a timer
        /// </summary>
        /// <returns></returns>
        public async Task<T> RunTimedFunction<T1, T2, T>(Guid id, Func<T1, T2, Task<T>> func, T1 arg1, T2 arg2, string dataType, string businessUnit, TimeSpan criticalAlertLimit)
        {       
            var processId = Start(id, func.Method.Name.ToString(), dataType, businessUnit, criticalAlertLimit);
            
            var result = await func.Invoke(arg1, arg2);

            var timer = Stop(processId);
                                             
            _logger.LogInformation("Call to Oracle {0} ThreadId: {1}, {2}.{3} execution time:  {4} mins, {5} seconds",
                timer.TaskName, timer.Id, timer.BusinessUnit, timer.DataType, timer.ElapsedTime.Minutes, timer.ElapsedTime.Seconds);

            CleanupTimers(ref timer);

            return result;
        }

        /// <summary>
        /// Runs the provided method in wrapped in a timer
        /// </summary>
        /// <returns></returns>
        public async Task<T> RunTimedFunction<T1, T2, T3, T>(Guid id, Func<T1, T2, T3, Task<T>> func, T1 arg1, T2 arg2, T3 arg3, string dataType, string businessUnit, TimeSpan criticalAlertLimit)
        {
            var processId = Start(id, func.Method.Name.ToString(), dataType, businessUnit, criticalAlertLimit);

            var result = await func.Invoke(arg1, arg2, arg3);

            var timer = Stop(processId);

            _logger.LogInformation("Call to Oracle {0} ThreadId: {1}, DataType {2}, BusinessUnit {3} execution time:  {4} mins, {5} seconds",
                timer.TaskName, timer.Id, timer.DataType, timer.BusinessUnit, timer.ElapsedTime.Minutes, timer.ElapsedTime.Seconds);

            CleanupTimers(ref timer);

            return result;
        }

        private void CleanupTimers(ref ThreadableStopWatch timer)
        {
            timer.Timer.Change(Timeout.Infinite, 0);
            timer.Timer.Dispose();
            _sws.Remove(timer.Id);
        }
    }
}
