using CMH.CS.ERP.IntegrationHub.Interpol.Interfaces;
using CMH.CS.ERP.IntegrationHub.Interpol.Models;
using CMH.CSS.ERP.IntegrationHub.CanonicalModels.Enumerations;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CMH.CS.ERP.IntegrationHub.Interpol.Biz
{
    /// <summary>
    /// Implementation of the ISchedulerTaskFactory
    /// </summary>
    public class SchedulerTaskFactory : ISchedulerTaskFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public SchedulerTaskFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        /// <inheritdoc/>
        public ISchedulerTask GetSchedulerTask(params object[] properties)
        {
            var dataType = (DataTypes)properties[0];
            var businessUnit = (IBusinessUnit)properties[1];
            var taskType = typeof(IEnumerable<ISchedulerTask>);
            var codeType = dataType.FromDataTypeToCodeType();

            var registeredServices = _serviceProvider.GetService(taskType) as IEnumerable<ISchedulerTask>;
            ISchedulerTask _task = registeredServices.FirstOrDefault(_service => _service.GetType().GetGenericArguments()[0] == codeType);

            if (_task is null)
            {
                throw new NotImplementedException($"No {nameof(ISchedulerTask)} with generic implementation type of {codeType.Name} was registered");
            }

            _task.DataType = dataType;
            _task.BusinessUnit = businessUnit;

            return _task;
        }
    }
}