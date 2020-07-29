using CMH.CS.ERP.IntegrationHub.Interpol.Interfaces;
using CMH.CS.ERP.IntegrationHub.Interpol.Models;
using CMH.CSS.ERP.IntegrationHub.CanonicalModels.Interfaces;
using System.Linq;

namespace CMH.CS.ERP.IntegrationHub.Interpol
{
    /// <summary>
    /// Implementation of Routing Key Generator
    /// </summary>
    public class EMBRoutingKeyGenerator : IEMBRoutingKeyGenerator
    {
        /// <inheritdoc/>
        public IEMBRoutingKeyInfo[] GenerateRoutingKeys(IEMBRoutingKeyProvider model)
        {
            string modelType;
            if (model is IAlternateDataTypeProvider)
            {
                modelType = (model as IAlternateDataTypeProvider).TreatAsDataType.Name;
            }
            else
            {
                modelType = model.GetType().Name;
            }
            
            return model.BusinessUnits                
                .Distinct()
                .Select(_bu => new EMBRoutingKeyInfo() 
                {
                    BusinessUnit = new BusinessUnit() { BUAbbreviation = _bu.CleanBUName(), BUName = _bu.CleanBUName() },
                    RoutingKey = $"{ _bu.CleanBUName()}.erp.{modelType.QueueNameFromDataTypeName()}"
                })
                .ToArray();
        }
    }
}