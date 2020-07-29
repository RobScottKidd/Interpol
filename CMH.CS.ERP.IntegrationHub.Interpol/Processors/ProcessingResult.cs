using CMH.CS.ERP.IntegrationHub.Interpol.Interfaces.Biz;

namespace CMH.CS.ERP.IntegrationHub.Interpol.Biz
{
    public class ProcessingResult<T> : IProcessingResult<T>
    {
        public T ProcessedItem { get; set; }
    }
}