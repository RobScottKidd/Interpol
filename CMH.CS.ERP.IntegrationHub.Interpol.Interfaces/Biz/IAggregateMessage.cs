namespace CMH.CS.ERP.IntegrationHub.Interpol.Interfaces.Biz
{
    public interface IAggregateMessage<T>
    {
        string EventType { get; }

        string Status { get; set; }

        string BusinessUnit { get; }

        T[] Messages { get; set; }

        string Version { get; }
    }
}