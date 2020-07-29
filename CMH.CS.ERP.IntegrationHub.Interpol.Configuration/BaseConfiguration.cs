using CMH.CS.ERP.IntegrationHub.Interpol.Interfaces.Configuration;
using FluentValidation;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq;

namespace CMH.CS.ERP.IntegrationHub.Interpol.Configuration
{
    /// <summary>
    /// Basic implementation of the base configuration interface
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class BaseConfiguration<T> : IBaseConfiguration<T>
    {
        /// <summary>
        /// Stores the root structure of the deserialized configuration file
        /// </summary>
        protected IConfigurationRoot config;
        private readonly string _fileName;
        private readonly IValidator<T> _validator;
        private T _value;
        private Action<T> _initFunction;

        /// <summary>
        /// Standard constructor
        /// </summary>
        /// <param name="fileName">Name of the configuration file excluding the file extension</param>
        public BaseConfiguration(string fileName, IValidator<T> validator, Action<T> initFunction = null)
        {
            _fileName = fileName;
            _validator = validator;
            _initFunction = initFunction;
            Build();
        }

        /// <summary>
        /// Builds the IConfigurationRoot from the given json file
        /// </summary>
        private void Build()
        {
            config = new ConfigurationBuilder()
                .AddJsonFile($"{_fileName}.json")
                .Build();
        }

        /// <summary>
        /// Validates the current configuration using a dynamically aquired implementation 
        /// of the AbstractValidator class
        /// </summary>
        private void ValidateConfiguration()
        {
            if (_validator != null)
            {
                var result = _validator.Validate(_value);

                if (result.Errors.Count > 0)
                {
                    throw new ValidationException($"Configuration for {_fileName} is invalid: {string.Join(", ", result.Errors.Select(_error => _error.ErrorMessage))}");
                }
            }
        }

        /// <inheritdoc/>
        public T Value
        {
            get
            {
                if (_value == null)
                {
                    _value = config.Get<T>();
                    if (_initFunction != default)
                    {
                        _initFunction(_value);
                    }
                    ValidateConfiguration();
                }

                return _value;
            }
        }
    }
}