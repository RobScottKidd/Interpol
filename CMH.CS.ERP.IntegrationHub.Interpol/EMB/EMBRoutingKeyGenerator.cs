using CMH.CS.ERP.IntegrationHub.Interpol.Interfaces;
using CMH.CS.ERP.IntegrationHub.Interpol.Interfaces.Biz;
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
            if (model is IAlternateDataTypeProvider altDataType)
            {
                modelType = altDataType.TreatAsDataType.Name;
            }
            else
            {
                modelType = model.GetType().Name;
            }
            // use model.AlternateBusinessUnit field for building the routing key

            if (model is IAlternateRoutingBU altModel)
            {
                return model.BusinessUnits
                    .Distinct()
                    .Select(_bu => new EMBRoutingKeyInfo()
                    {
                        BusinessUnit = altModel.AlternateBU,
                        RoutingKey = $"{altModel.AlternateBU.BUName.CleanBUName()}.erp.{altModel.BaseVerticalType.Name}"
                    })
                    .ToArray();
            }
            else
            {
                // compare bu to model.BusinessUnits
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
}