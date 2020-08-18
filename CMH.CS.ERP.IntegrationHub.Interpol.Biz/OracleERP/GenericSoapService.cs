﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System;
using System.ServiceModel.Channels;
using WcfCoreMtomEncoder;

namespace CMH.CS.ERP.IntegrationHub.Interpol.Biz.GenericSoapService
{
    [System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.Tools.ServiceModel.Svcutil", "2.0.1-preview-30514-0828")]
    [System.ServiceModel.ServiceContractAttribute(Namespace = "urn:GenericSoap", ConfigurationName = "GenericSoapService.GenericSoapPortType")]
    public interface IGenericSoapPortType : IDisposable
    {
        // CODEGEN: Generating message contract since the operation GenericSoapOperation is neither RPC nor document wrapped.
        [System.ServiceModel.OperationContractAttribute(Action = "urn:GenericSoap/GenericSoapOperation", ReplyAction = "urn:GenericSoap/GenericSoapPortType/GenericSoapOperationResponse")]
        [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults = true)]
        GenericSoapService.GenericSoapOperationResponse GenericSoapOperation(GenericSoapService.GenericSoapOperationRequest request);

        [System.ServiceModel.OperationContractAttribute(Action = "urn:GenericSoap/GenericSoapOperation", ReplyAction = "urn:GenericSoap/GenericSoapPortType/GenericSoapOperationResponse")]
        System.Threading.Tasks.Task<GenericSoapService.GenericSoapOperationResponse> GenericSoapOperationAsync(GenericSoapService.GenericSoapOperationRequest request);
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.Tools.ServiceModel.Svcutil", "2.0.1-preview-30514-0828")]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.oracle.com/UCM")]
    public partial class Generic
    {

        private Service serviceField;

        private string webKeyField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Order = 0)]
        public Service Service
        {
            get
            {
                return this.serviceField;
            }
            set
            {
                this.serviceField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string webKey
        {
            get
            {
                return this.webKeyField;
            }
            set
            {
                this.webKeyField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.Tools.ServiceModel.Svcutil", "2.0.1-preview-30514-0828")]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.oracle.com/UCM")]
    public partial class Service
    {

        private Container userField;

        private ServiceDocument documentField;

        private string idcServiceField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Order = 0)]
        public Container User
        {
            get
            {
                return this.userField;
            }
            set
            {
                this.userField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Order = 1)]
        public ServiceDocument Document
        {
            get
            {
                return this.documentField;
            }
            set
            {
                this.documentField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string IdcService
        {
            get
            {
                return this.idcServiceField;
            }
            set
            {
                this.idcServiceField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.Tools.ServiceModel.Svcutil", "2.0.1-preview-30514-0828")]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.oracle.com/UCM")]
    public partial class Container
    {

        private Field[] fieldField;

        private ResultSet[] resultSetField;

        private OptionList[] optionListField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("Field", Order = 0)]
        public Field[] Field
        {
            get
            {
                return this.fieldField;
            }
            set
            {
                this.fieldField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("ResultSet", Order = 1)]
        public ResultSet[] ResultSet
        {
            get
            {
                return this.resultSetField;
            }
            set
            {
                this.resultSetField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("OptionList", Order = 2)]
        public OptionList[] OptionList
        {
            get
            {
                return this.optionListField;
            }
            set
            {
                this.optionListField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.Tools.ServiceModel.Svcutil", "2.0.1-preview-30514-0828")]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.oracle.com/UCM")]
    public partial class Field
    {

        private string nameField;

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string name
        {
            get
            {
                return this.nameField;
            }
            set
            {
                this.nameField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlTextAttribute()]
        public string Value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.Tools.ServiceModel.Svcutil", "2.0.1-preview-30514-0828")]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.oracle.com/UCM")]
    public partial class File
    {

        private byte[] contentsField;

        private string nameField;

        private string hrefField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "base64Binary", Order = 0)]
        public byte[] Contents
        {
            get
            {
                return this.contentsField;
            }
            set
            {
                this.contentsField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string name
        {
            get
            {
                return this.nameField;
            }
            set
            {
                this.nameField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string href
        {
            get
            {
                return this.hrefField;
            }
            set
            {
                this.hrefField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.Tools.ServiceModel.Svcutil", "2.0.1-preview-30514-0828")]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.oracle.com/UCM")]
    public partial class OptionList
    {

        private string[] optionField;

        private string nameField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("Option", Order = 0)]
        public string[] Option
        {
            get
            {
                return this.optionField;
            }
            set
            {
                this.optionField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string name
        {
            get
            {
                return this.nameField;
            }
            set
            {
                this.nameField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.Tools.ServiceModel.Svcutil", "2.0.1-preview-30514-0828")]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.oracle.com/UCM")]
    public partial class Row
    {

        private Field[] fieldField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("Field", Order = 0)]
        public Field[] Field
        {
            get
            {
                return this.fieldField;
            }
            set
            {
                this.fieldField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.Tools.ServiceModel.Svcutil", "2.0.1-preview-30514-0828")]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.oracle.com/UCM")]
    public partial class ResultSet
    {

        private Row[] rowField;

        private string nameField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("Row", Order = 0)]
        public Row[] Row
        {
            get
            {
                return this.rowField;
            }
            set
            {
                this.rowField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string name
        {
            get
            {
                return this.nameField;
            }
            set
            {
                this.nameField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.Tools.ServiceModel.Svcutil", "2.0.1-preview-30514-0828")]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.oracle.com/UCM")]
    public partial class ServiceDocument : Container
    {

        private File[] fileField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("File", Order = 0)]
        public File[] File
        {
            get
            {
                return this.fileField;
            }
            set
            {
                this.fileField = value;
            }
        }
    }

    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.Tools.ServiceModel.Svcutil", "2.0.1-preview-30514-0828")]
    [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
    [System.ServiceModel.MessageContractAttribute(IsWrapped = false)]
    public partial class GenericSoapOperationRequest
    {

        [System.ServiceModel.MessageBodyMemberAttribute(Namespace = "http://www.oracle.com/UCM", Order = 0)]
        public GenericSoapService.Generic GenericRequest;

        public GenericSoapOperationRequest()
        {
        }

        public GenericSoapOperationRequest(GenericSoapService.Generic GenericRequest)
        {
            this.GenericRequest = GenericRequest;
        }
    }

    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.Tools.ServiceModel.Svcutil", "2.0.1-preview-30514-0828")]
    [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
    [System.ServiceModel.MessageContractAttribute(IsWrapped = false)]
    public partial class GenericSoapOperationResponse
    {

        [System.ServiceModel.MessageBodyMemberAttribute(Namespace = "http://www.oracle.com/UCM", Order = 0)]
        public GenericSoapService.Generic GenericResponse;

        public GenericSoapOperationResponse()
        {
        }

        public GenericSoapOperationResponse(GenericSoapService.Generic GenericResponse)
        {
            this.GenericResponse = GenericResponse;
        }
    }

    [System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.Tools.ServiceModel.Svcutil", "2.0.1-preview-30514-0828")]
    public interface GenericSoapPortTypeChannel : GenericSoapService.IGenericSoapPortType, System.ServiceModel.IClientChannel
    {
    }

    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.Tools.ServiceModel.Svcutil", "2.0.1-preview-30514-0828")]
    public partial class GenericSoapPortTypeClient : System.ServiceModel.ClientBase<GenericSoapService.IGenericSoapPortType>, GenericSoapService.IGenericSoapPortType
    {

        public GenericSoapPortTypeClient(string endpointUrl, TimeSpan sendTimeout, TimeSpan receiveTimeout, string username, string password) :
        base(GenericSoapPortTypeClient.GetBindingForEndpoint(sendTimeout, receiveTimeout), GenericSoapPortTypeClient.GetEndpointAddress(endpointUrl))
        {
            this.ChannelFactory.Credentials.UserName.UserName = username;
            this.ChannelFactory.Credentials.UserName.Password = password;
            ConfigureEndpoint(this.Endpoint, this.ClientCredentials);
        }

        public GenericSoapPortTypeClient(System.ServiceModel.Channels.Binding binding, System.ServiceModel.EndpointAddress remoteAddress) : base(binding, remoteAddress)
        {
        }

        static partial void ConfigureEndpoint(System.ServiceModel.Description.ServiceEndpoint serviceEndpoint, System.ServiceModel.Description.ClientCredentials clientCredentials);

        private static System.ServiceModel.Channels.Binding GetBindingForEndpoint(TimeSpan sendTimeout, TimeSpan receiveTimeout)
        {
            var encoding = new MtomMessageEncoderBindingElement(new TextMessageEncodingBindingElement()
            {
                MessageVersion = MessageVersion.Soap11,
                ReaderQuotas = new System.Xml.XmlDictionaryReaderQuotas()
                {
                    MaxStringContentLength = int.MaxValue,
                    MaxArrayLength = int.MaxValue
                }
            });
            var transport = new HttpsTransportBindingElement()
            {
                AllowCookies = true,
                MaxBufferSize = int.MaxValue,
                MaxReceivedMessageSize = int.MaxValue,
                AuthenticationScheme = System.Net.AuthenticationSchemes.Basic,
                KeepAliveEnabled = false
            };

            var customBinding = new CustomBinding(encoding, transport)
            {
                SendTimeout = sendTimeout,
                ReceiveTimeout = receiveTimeout
            };

            return customBinding;
        }

        private static System.ServiceModel.EndpointAddress GetEndpointAddress(string endpointUrl)
        {
            if (!endpointUrl.StartsWith("https://"))
            {
                throw new UriFormatException("The endpoint URL must start with https://.");
            }
            return new System.ServiceModel.EndpointAddress(endpointUrl);
        }

        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
        GenericSoapService.GenericSoapOperationResponse GenericSoapService.IGenericSoapPortType.GenericSoapOperation(GenericSoapService.GenericSoapOperationRequest request)
        {
            return base.Channel.GenericSoapOperation(request);
        }

        public GenericSoapService.Generic GenericSoapOperation(GenericSoapService.Generic GenericRequest)
        {
            GenericSoapService.GenericSoapOperationRequest inValue = new GenericSoapService.GenericSoapOperationRequest();
            inValue.GenericRequest = GenericRequest;
            GenericSoapService.GenericSoapOperationResponse retVal = ((GenericSoapService.IGenericSoapPortType)(this)).GenericSoapOperation(inValue);
            return retVal.GenericResponse;
        }

        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
        System.Threading.Tasks.Task<GenericSoapService.GenericSoapOperationResponse> GenericSoapService.IGenericSoapPortType.GenericSoapOperationAsync(GenericSoapService.GenericSoapOperationRequest request)
        {
            return base.Channel.GenericSoapOperationAsync(request);
        }

        public System.Threading.Tasks.Task<GenericSoapService.GenericSoapOperationResponse> GenericSoapOperationAsync(GenericSoapService.Generic GenericRequest)
        {
            GenericSoapService.GenericSoapOperationRequest inValue = new GenericSoapService.GenericSoapOperationRequest();
            inValue.GenericRequest = GenericRequest;
            return ((GenericSoapService.IGenericSoapPortType)(this)).GenericSoapOperationAsync(inValue);
        }

        public virtual System.Threading.Tasks.Task OpenAsync()
        {
            return System.Threading.Tasks.Task.Factory.FromAsync(((System.ServiceModel.ICommunicationObject)(this)).BeginOpen(null, null), new System.Action<System.IAsyncResult>(((System.ServiceModel.ICommunicationObject)(this)).EndOpen));
        }

        public virtual System.Threading.Tasks.Task CloseAsync()
        {
            return System.Threading.Tasks.Task.Factory.FromAsync(((System.ServiceModel.ICommunicationObject)(this)).BeginClose(null, null), new System.Action<System.IAsyncResult>(((System.ServiceModel.ICommunicationObject)(this)).EndClose));
        }
    }
}
