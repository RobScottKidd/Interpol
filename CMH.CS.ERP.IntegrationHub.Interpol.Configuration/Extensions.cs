using CMH.CS.ERP.IntegrationHub.Interpol.Configuration;
using CMH.CS.ERP.IntegrationHub.Interpol.Interfaces.Configuration;
using FluentValidation;
using System;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Extension methods class for the configuration component
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Extension providing service collection singleton registration for specified configuration class
        /// </summary>
        /// <param name="collection"></param>
        /// <returns></returns>
        public static IServiceCollection AddConfiguration<T>(this IServiceCollection collection, IValidator<T> validator = null, Action<T> initFunction = null)
        {
            collection.AddSingleton(typeof(IBaseConfiguration<T>), new BaseConfiguration<T>(typeof(T).Name, validator, initFunction));
            return collection;
        }

        public static IServiceCollection AddConfiguration<T>(this IServiceCollection collection, Action<T> initFunction) => collection.AddConfiguration(null, initFunction);
    }
}