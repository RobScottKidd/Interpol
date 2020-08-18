using CMH.CS.ERP.IntegrationHub.Interpol.Interfaces.Biz;
using System.Linq;
using System.Reflection;

namespace CMH.CS.ERP.IntegrationHub.Interpol.Biz
{
    public static class SetAlternateRoutingVerticalProperties
    {
        public static IAlternateRoutingBU SetProperties(object baseClass, IAlternateRoutingBU destinationClass)
        {
            var bindingFlags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic;
            var baseClassType = baseClass.GetType();
            MemberInfo[] members = baseClassType.GetFields(bindingFlags).Cast<MemberInfo>()
                .Concat(baseClassType.GetProperties(bindingFlags)).ToArray();
            foreach (var memberInfo in members)
            {
                if (memberInfo.MemberType == MemberTypes.Property)
                {
                    var propertyInfo = memberInfo as PropertyInfo;
                    object value = propertyInfo.GetValue(baseClass, null);
                    if (value != null && propertyInfo.CanWrite)
                    {
                        propertyInfo.SetValue(destinationClass, value, null);
                    }
                }
                else
                {
                    var fieldInfo = memberInfo as FieldInfo;
                    object value = fieldInfo.GetValue(baseClass);
                    if (value != null)
                    {
                        fieldInfo.SetValue(destinationClass, value);
                    }
                }
            }
            destinationClass.BaseVerticalType = baseClassType;
            return destinationClass;
        }
    }
}