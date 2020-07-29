using CMH.CS.ERP.IntegrationHub.Interpol.Interfaces.Configuration;
using FluentValidation;
using System;
using System.IO;

namespace CMH.CS.ERP.IntegrationHub.Interpol.Configuration
{
    /// <summary>
    /// Implementation of the AbstractValidator class for Interpol configuration
    /// </summary>
    public class InterpolConfigurationValidator : AbstractValidator<IInterpolConfiguration>
    {
        /// <summary>
        /// Default ctor for InterpolValidator
        /// </summary>
        public InterpolConfigurationValidator()
        {
            RuleFor(_interpol => _interpol.Schedules)
                .NotEmpty()
                .WithMessage("Schedules must have a value");

            RuleFor(_interpol => _interpol.EnableDataExport)
                .NotNull();

            RuleFor(_interpol => _interpol.ExportDirectory)
                        .NotEmpty()
                        .WithMessage("Export directory must be defined")
                        .When(_interpol => _interpol.EnableDataExport);

            RuleFor(_interpol => _interpol.ExportDirectory)
                .Must(_dir => Directory.Exists(_dir))
                .WithMessage(_interpol => $"Export directory \"{_interpol.ExportDirectory}\" does not exist")
                .When(_interpol => _interpol.EnableDataExport);

            RuleFor(_interpol => _interpol.PollRetryCount)
                .NotNull()
                .GreaterThan(-1);

            RuleFor(_interpol => _interpol.PollRetryDelay)
                .NotNull()
                .GreaterThan(-1);

            RuleFor(_interpol => _interpol.PublishRetryCount)
                .NotNull()
                .GreaterThan(-1);

            RuleFor(_interpol => _interpol.PublishRetryDelay)
                .NotNull()
                .GreaterThan(-1);

            RuleFor(_interpol => _interpol.CacheLifetime)
                .NotNull()
                .GreaterThanOrEqualTo(new TimeSpan(0));
        }
    }
}