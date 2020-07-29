using System;
using System.Threading.Tasks;

namespace CMH.CS.ERP.IntegrationHub.Interpol.Interfaces
{
    public interface IMultistepOperationTimer
    {
        Task<T> RunTimedFunction<T1, T2, T>(Guid id, Func<T1, T2, Task<T>> func, T1 arg1, T2 arg2, string dataType, string businessUnit, TimeSpan criticalAlertLimit);

        Task<T> RunTimedFunction<T1, T2, T3, T>(Guid id, Func<T1, T2, T3, Task<T>> func, T1 arg1, T2 arg2, T3 arg3, string dataType, string businessUnit, TimeSpan criticalAlertLimit);
    }
}