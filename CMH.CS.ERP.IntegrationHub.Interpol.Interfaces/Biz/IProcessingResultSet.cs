using System.Collections.Generic;

namespace CMH.CS.ERP.IntegrationHub.Interpol.Interfaces.Biz
{
    public interface IProcessingResultSet<T> 
    {
        IEnumerable<IProcessingResult<T>> ProcessedItems { get; set; }

        IEnumerable<IProcessingResult<IUnparsable>> UnparsableItems { get; set; }
    }
}