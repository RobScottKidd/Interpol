using CMH.CS.ERP.IntegrationHub.Interpol.Interfaces.Biz;
using CMH.CSS.ERP.IntegrationHub.CanonicalModels;
using Microsoft.Extensions.Logging;

namespace CMH.CS.ERP.IntegrationHub.Interpol.Biz
{
    /// <summary>
    /// GL Journal specfic implementation of the backflow processor
    /// </summary>
    public class OracleBackflowGLJournalProcessor : OracleBackflowProcessor<GLJournal>
    {
        /// <summary>
        /// Base constructor
        /// </summary>
        /// <param name="logger"></param>
        public OracleBackflowGLJournalProcessor(ILogger<OracleBackflowGLJournalProcessor> logger) : base(logger)
        {
            ROOT_ELEMENT = "G_1";
        }

        /// <summary>
        /// Processes all Accounting Hub items from the xml string
        /// </summary>
        /// <param name="xmlString">XML string from Oracle</param>
        /// <returns></returns>
        public override IProcessingResultSet<GLJournal> ProcessItems(string xmlString, string businessUnit)
        {
            return base.ProcessItems(xmlString, businessUnit);
        }
    }
}