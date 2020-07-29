using CMH.CS.ERP.IntegrationHub.Interpol.Interfaces.Configuration;
using FluentValidation;
using System;
using System.Linq;

namespace CMH.CS.ERP.IntegrationHub.Interpol.Configuration
{
    /// <summary>
    /// Implementation of the AbstractValidator class for Schedule configuration
    /// </summary>
    public class ScheduleValidator : AbstractValidator<IScheduleConfiguration>
    {
        private static readonly DayOfWeek[] _daysOfWeek = (Enum.GetValues(typeof(DayOfWeek)) as DayOfWeek[]);

        /// <summary>
        /// Default ctor for ScheduleValidator
        /// </summary>
        public ScheduleValidator()
        {
            RuleFor(_schedule => _schedule.Name)
                .NotNull()
                .MinimumLength(1)
                .WithMessage("Schedule Name must be supplied and >= 1 character");

            RuleFor(_schedule => _schedule.DaysOfWeek)
                .Must(_days => (_days?.Length ?? 0) <= _daysOfWeek.Length)
                .WithMessage($"DaysOfWeek can contain a maximum of {_daysOfWeek.Length} values");

            RuleForEach(_schedule => _schedule.DaysOfWeek)
                .Must(_day => _daysOfWeek.Contains(_day))
                .WithMessage($"DaysOfWeek must be comprised of the following values: {string.Join(", ", _daysOfWeek)}");

            RuleFor(_schedule => _schedule.PollingIntervalMilliseconds)
                .NotNull()
                .WithMessage("Schedule Interval must contain a value");

            RuleFor(_schedule => _schedule)
                .Must(_schedule =>
                {
                    if (_schedule.StartTime.HasValue && _schedule.EndTime.HasValue)
                    {
                        return _schedule.StartTime.Value < _schedule.EndTime.Value;
                    }

                    return true;
                })
                .WithMessage("Schedule start time must be < Schedule end time");
        }
    }
}