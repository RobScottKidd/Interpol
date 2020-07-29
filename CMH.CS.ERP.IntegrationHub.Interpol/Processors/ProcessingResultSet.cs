using CMH.CS.ERP.IntegrationHub.Interpol.Interfaces.Biz;
using System.Collections.Generic;

namespace CMH.CS.ERP.IntegrationHub.Interpol.Biz
{
    public class ProcessingResultSet<T> : IProcessingResultSet<T>
    {
        public ProcessingResultSet()
        {
            ProcessedItems = new List<IProcessingResult<T>>();
            UnparsableItems = new List<IProcessingResult<IUnparsable>>();
        }

        public IEnumerable<IProcessingResult<T>> ProcessedItems { get; set; }

        public IEnumerable<IProcessingResult<IUnparsable>> UnparsableItems { get; set; }
    }
}