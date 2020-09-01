using CMH.CS.ERP.IntegrationHub.Interpol.Interfaces.Biz;
using System.Linq;
using System.Reflection;

namespace CMH.CS.ERP.IntegrationHub.Interpol.Biz
{
    public static class SetAlternateRoutingVerticalProperties
    {
        public static void SetProperties(this IAlternateRoutingBU destinationClass, object baseClass)
        {
            var bindingFlags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic;
            var baseClassType = baseClass.GetType();

            baseClassType.GetFields(bindingFlags)
                .ToList()
                .ForEach(field => field.SetValue(destinationClass, field.GetValue(baseClass)));

            baseClassType.GetProperties(bindingFlags)
                .Where(property => property.CanWrite)
                .ToList()
                .ForEach(property => property.SetValue(destinationClass, property.GetValue(baseClass)));
        }
    }
}