using CMH.CSS.ERP.IntegrationHub.CanonicalModels;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace CMH.CS.ERP.IntegrationHub.Interpol.Models.Mappers
{
    /// <summary>
    /// Supplier Model
    /// </summary>   
    [XmlRoot("Suppliers_report")] 
    public class Suppliers : IEnumerable<Supplier>
    {
        private readonly List<Supplier> _suppliers = new List<Supplier>();

        /// <summary>
        /// Add Supplier
        /// <param name="supplier">The canonical supplier objects</param>
        /// </summary>
        public void Add(Supplier supplier)
        {
            _suppliers.Add(supplier);
        }

        /// <summary>
        /// GetEnumerator
        /// </summary>
        public IEnumerator<Supplier> GetEnumerator()
        {
            return _suppliers.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}