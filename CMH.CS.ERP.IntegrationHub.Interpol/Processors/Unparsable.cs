using CMH.CS.ERP.IntegrationHub.Interpol.Interfaces.Biz;
using System;

namespace CMH.CS.ERP.IntegrationHub.Interpol.Biz
{
    public class Unparsable : IUnparsable
    {
        public string Xml { get; set; }

        public string Error { get; set; }

        public string[] BusinessUnits { get; set; }

        public string Status { get; set; }
        
        public Type TreatAsDataType { get; set; }
        
        public string MessageType { get; set; }

        private string _version;

        /// <summary>
        /// Specifies the version for this instance of canonical model 
        /// </summary>
        public string Version
        {
            get
            {
                if (string.IsNullOrEmpty(_version))
                {
                    _version = InternalModelVersion;
                }
                return _version;
            }
            set
            {
                _version = !string.IsNullOrEmpty(value) ? value : InternalModelVersion;
            }
        }

        /// <summary>
        /// Specifies the version of this class
        /// </summary>
        public string InternalModelVersion => "V1.0";
    }
}