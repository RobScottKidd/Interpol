using System;
using System.Collections.Generic;
using System.Text;

namespace CMH.CS.ERP.IntegrationHub.Interpol.Interfaces.Biz
{
    public interface IDirectedMessageProcessor
    {
        int Process(object[] items, string businessUnit, string exchangeName, string routingKey);
    }
}
