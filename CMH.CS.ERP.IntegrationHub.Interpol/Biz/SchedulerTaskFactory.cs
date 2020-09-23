using CMH.CS.ERP.IntegrationHub.Interpol.Interfaces;
using CMH.CSS.ERP.IntegrationHub.CanonicalModels;
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
        private readonly IEnumerable<ISchedulerTask> _schedulerTasks;

        public SchedulerTaskFactory(IEnumerable<ISchedulerTask> schedulerTasks)
        {
            _schedulerTasks = schedulerTasks;
        }

        /// <inheritdoc/>
        public ISchedulerTask GetSchedulerTask(DataTypes dataType, IBusinessUnit businessUnit)
        {
            var codeType = FromDataTypeToCodeType(dataType);
            ISchedulerTask _task = _schedulerTasks.FirstOrDefault(task => task.GetType().GenericTypeArguments?.FirstOrDefault() == codeType);

            if (_task is null)
            {
                throw new NotImplementedException($"No {nameof(ISchedulerTask)} with generic implementation type of {dataType} was registered");
            }

            _task.DataType = dataType;
            _task.BusinessUnit = businessUnit;

            return _task;
        }

        private Type FromDataTypeToCodeType(DataTypes dataType) => dataType switch
        {
            DataTypes.apinvoice => typeof(APInvoice),
            DataTypes.appaymentrequest => typeof(APPaymentRequest),
            DataTypes.supplier => typeof(Supplier),
            DataTypes.employeesync => typeof(EmployeeSync),
            DataTypes.apinvoicestatusmessage => typeof(APInvoiceStatusMessage),
            DataTypes.appaymentrequeststatusmessage => typeof(APPaymentRequestStatusMessage),
            DataTypes.appayment => typeof(APPayment),
            DataTypes.accountinghubstatusmessage => typeof(AccountingHubStatusMessage),
            DataTypes.gljournal => typeof(GLJournal),
            DataTypes.cashmanagementstatusmessage => typeof(CashManagementStatusMessage),
            DataTypes.gljournalstatusmessage => typeof(GLJournalStatusMessage),
            _ => throw new NotImplementedException($"No code type defined for { dataType }"),
        };
    }
}