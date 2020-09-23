using CMH.CSS.ERP.IntegrationHub.CanonicalModels;
using CMH.CSS.ERP.IntegrationHub.CanonicalModels.Enumerations;
using System;

namespace CMH.CS.ERP.IntegrationHub.Interpol.Models
{
    /// <summary>
    /// Contains extensions for data type conversions/manipulations etc...
    /// </summary>
    public static class DataTypeExtensions
    {
        /// <summary>
        /// Converts code type to data type enum
        /// </summary>
        /// <param name="classType"></param>
        /// <returns></returns>
        public static DataTypes FromCodeTypeToDataType(this Type classType) => classType.Name switch
        {
            nameof(APInvoice) => DataTypes.apinvoice,
            nameof(APPaymentRequest) => DataTypes.appaymentrequest,
            nameof(Supplier) => DataTypes.supplier,
            nameof(EmployeeSync) => DataTypes.employeesync,
            nameof(APInvoiceStatusMessage) => DataTypes.apinvoicestatusmessage,
            nameof(APPaymentRequestStatusMessage) => DataTypes.appaymentrequeststatusmessage,
            nameof(APPayment) => DataTypes.appayment,
            nameof(APPaymentWithDocument) => DataTypes.appaymentwithdocument,
            nameof(AccountingHubStatusMessage) => DataTypes.accountinghubstatusmessage,
            nameof(GLJournal) => DataTypes.gljournal,
            nameof(CashManagementStatusMessage) => DataTypes.cashmanagementstatusmessage,
            nameof(GLJournalStatusMessage) => DataTypes.gljournalstatusmessage,
            _ => throw new NotImplementedException($"No data type defined for {classType.Name}"),
        };

        public static Type FromDataTypeToCodeType(this DataTypes dataType) => dataType switch
        {
            DataTypes.apinvoice => typeof(APInvoice),
            DataTypes.appaymentrequest => typeof(APPaymentRequest),
            DataTypes.supplier => typeof(Supplier),
            DataTypes.employeesync => typeof(EmployeeSync),
            DataTypes.apinvoicestatusmessage => typeof(CSS.ERP.IntegrationHub.CanonicalModels.APInvoiceStatusMessage),
            DataTypes.appaymentrequeststatusmessage => typeof(CSS.ERP.IntegrationHub.CanonicalModels.APPaymentRequestStatusMessage),
            DataTypes.appayment => typeof(APPayment),
            DataTypes.appaymentwithdocument => typeof(APPaymentWithDocument),
            DataTypes.accountinghubstatusmessage => typeof(CSS.ERP.IntegrationHub.CanonicalModels.AccountingHubStatusMessage),
            DataTypes.gljournal => typeof(GLJournal),
            DataTypes.cashmanagementstatusmessage => typeof(CSS.ERP.IntegrationHub.CanonicalModels.CashManagementStatusMessage),
            DataTypes.gljournalstatusmessage => typeof(CSS.ERP.IntegrationHub.CanonicalModels.GLJournalStatusMessage),
            _ => throw new NotImplementedException($"No code type defined for { dataType }"),
        };
    }
}