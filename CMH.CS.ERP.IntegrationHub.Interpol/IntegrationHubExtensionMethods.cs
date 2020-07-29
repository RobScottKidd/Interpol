using System;

namespace CMH.CS.ERP.IntegrationHub.Interpol
{
    public static class IntegrationHubExtensionMethods
    {
        /// <summary>
        /// Returns the data type name we need in various operations
        /// </summary>
        /// <param name="value">The object to type</param>
        /// <returns>The lowercase name</returns>
        public static string DataTypeName(this object value)
        {
            return value.GetType().Name.ToLower().Trim();
        }

        public static string CleanBUName(this string value)
        {
            var buName = string.Empty;
            if (string.IsNullOrEmpty(value)) 
            {
                return buName;
            }
            else
            {
                buName = string.IsNullOrEmpty(value) ? "unknown" : value;
                return buName.Replace(" BU", "")
                     .Replace(" ", "")
                     .ToLower()
                     .Trim(); ;
            }
        }

        public static string CleanBUALtName(this string value)
        { 
            var buName = string.Empty;
            if (string.IsNullOrEmpty(value))
            {
                return buName;
            }
            else
            {
                buName = string.IsNullOrEmpty(value) ? "unknown" : value;
                return buName.Replace(" BU", "")
                     .Replace(" ", "")
                     .ToLower()
                     .Trim(); ;
            }
        }

        /// <summary>
        /// gets the appropriate queue name for the data type supplied (some message types may share queues
        /// </summary>
        /// <param name="value">The object to get the type from</param>
        /// <returns></returns>
        public static string QueueNameFromDataTypeName(this string value)
        {
            value = value.ToLower();

            string queuename = "queuename";//To fake it out so queuename is not blank
            string[] strArray = { "StatusMessage", "Feedback", "EntryCrp", "EntryDomo", "EntryHfa", "EntryVmf" };

            foreach (string str in strArray)
            {
                if (value.Contains(str.ToLower()))
                {
                    queuename = str.ToLower();
                    break;
                }
            }

            return value.Replace(queuename, "").ToLower();
        }

        /// <summary>
        /// gets the appropriate queue name for the data type supplied (some message types may share queues
        /// </summary>
        /// <param name="value">The object to get the type from</param>
        /// <returns></returns>
        public static string QueueNameFromDataTypeName(this object value)
        {
            string queuename = "queuename";//To fake it out so queuename is not blank
            string[] strArray = {"StatusMessage", "Feedback", "EntryCrp", "EntryDomo", "EntryHfa", "EntryVmf" };
            
            foreach (string str in strArray)
            {
                if (Convert.ToString(value).Contains(str)) 
                { 
                    queuename = str; 
                    break; 
                }
            }

            return DataTypeName(value).Replace(queuename.ToLower(), "").ToLower();
        }
    }
}