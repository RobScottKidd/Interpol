using CMH.CSS.ERP.GlobalUtilities;
using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace CMH.CS.ERP.IntegrationHub.Interpol.Models.Mappers
{
    /// <summary>
    /// Interface defines methods for mapping objects to Oracle or Canonical formats
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ReportMapper<T>
    {
        /// <summary>
        /// T Mapper to Oracle or Canonical formats
        /// </summary>
        /// <param name="ofXml"></param>
        public T Mapper(string ofXml)
        {
            var serializer = new XmlSerializer(typeof(T));
            using var reader = new CachedXmlReader(new StringReader(ofXml));
            try
            {
                var result = (T)serializer.Deserialize(reader);
                return result;
            }
            catch (Exception ex)
            {
                var exceptionToUse = (ex.InnerException != null) ? ex.InnerException : ex;
                throw new XmlException($"Unable to Parse XML for field {reader.PreviousNode.Name} because {exceptionToUse.Message}", exceptionToUse);
            }
        }

        //private void RecursivelyPopulateModel(IEnumerable<XElement> nodes, string elementName, ref T newObj2, ref List<string> errors)
        //{
        //    var simpleList = nodes.ToList();
        //    foreach (var node in nodes)
        //    {

        //        if (node.HasElements)
        //        {
        //            RecursivelyPopulateModel(node.Elements(), node.Name.ToString(), ref newObj2, ref errors);
        //        }

        //        var test = node.Name.ToString();
        //        PropertyInfo prop = newObj2.GetType().GetProperty(node.Name.ToString());

        //        if (prop != null)
        //        {
        //            try
        //            {
        //                switch (prop.PropertyType.Name)
        //                {
        //                    case "String":
        //                        prop.SetValue(newObj2, node.Value);
        //                        break;
        //                    case "Int32":
        //                        prop.SetValue(newObj2, Convert.ToInt32(node.Value));
        //                        break;
        //                    case "Int64":
        //                        prop.SetValue(newObj2, Convert.ToInt64(node.Value));
        //                        break;
        //                    case "Double":
        //                        prop.SetValue(newObj2, Convert.ToDouble(node.Value));
        //                        break;
        //                    case "Date":

        //                        string dateToParse;
        //                        if (node.Value.Contains("T")) //need this in here to handle dates with times
        //                        {
        //                            dateToParse = node.Value.Split('T')[0];
        //                        }

        //                        else
        //                        {
        //                            dateToParse = node.Value;
        //                        }

        //                        prop.SetValue(newObj2, Date.Parse(dateToParse));
        //                        break;
        //                    case "Guid":
        //                        prop.SetValue(newObj2, new Guid(node.Value));
        //                        break;
        //                }
        //            }
        //            catch (Exception ex)
        //            {
        //                errors.Add($"{prop.Name} : {ex.Message}");
        //            }
        //        }
        //    }
        //}
    }
}