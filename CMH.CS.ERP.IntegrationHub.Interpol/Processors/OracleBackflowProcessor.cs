﻿using CMH.CS.ERP.IntegrationHub.Interpol.Interfaces.Biz;
using CMH.CS.ERP.IntegrationHub.Interpol.Models.Mappers;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace CMH.CS.ERP.IntegrationHub.Interpol.Biz
{
    public class OracleBackflowProcessor<T> : IOracleBackflowProcessor<T>
    {
        protected readonly ILogger _logger;
        protected string ROOT_ELEMENT;

        private const string BUSINESSUNIT_TAG_BU = "businessunit";
        private const string SUBLEDGER_TAG = "subledger";
        private const string PROCUREMENT_TAG = "procurementbu";

        /// <summary>
        /// Base constructor
        /// </summary>
        /// <param name="logger"></param>
        public OracleBackflowProcessor(ILogger<OracleBackflowProcessor<T>> logger, string rootElement = null)
        {
            _logger = logger;
            ROOT_ELEMENT = rootElement;
        }

        /// <summary>
        /// Processes the xml string to canonical format
        /// </summary>
        /// <param name="xmlString">xml string for collection of canonical models</param>
        /// <returns></returns>
        public virtual IProcessingResultSet<T> ProcessItems(string xmlString, string businessUnit)
        {
            if (string.IsNullOrWhiteSpace(xmlString))
            {
                throw new ArgumentException("XML string must not be empty");
            }

            if (string.IsNullOrWhiteSpace(ROOT_ELEMENT))
            {
                throw new InvalidOperationException($"No root element specified for xml parsing of {typeof(T).Name}");
            }

            //TODO: The following code skips every other element in the array if there is no white space between them.
            // This is NOT the right way to fix this issue, but it will work for now. Note the space before the root element

            // nothing to process
            if (!xmlString.Contains(ROOT_ELEMENT))
            {
                return new ProcessingResultSet<T>();
            }

            //can't have these in if I want to parse with Regex.
            xmlString = xmlString.Replace("\r", "").Replace("\n", "").Replace("\t", "");
            //set Regex pattern

            Regex rgx = new Regex($"<{ROOT_ELEMENT}>.*?</{ROOT_ELEMENT}>");
            //grab all matches
            var itemBlocks = rgx.Matches(xmlString);

            var reportMapper = new ReportMapper<T>();
            var successfulItems = new List<IProcessingResult<T>>();
            var unsuccessfulItems = new List<IProcessingResult<IUnparsable>>();
            
            foreach (Match match in itemBlocks)
            {
                try
                {
                    T processedItem = default;

                    var validXml = $"<?xml version=\"1.0\" encoding=\"UTF8\"?>";
                    validXml += match.Value.Replace($"<{ROOT_ELEMENT}>", $"<{ROOT_ELEMENT} xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\">");
                    
                    processedItem = reportMapper.Mapper(validXml);
                    successfulItems.Add(new ProcessingResult<T>()
                    {
                        ProcessedItem = processedItem
                    });
                }
                catch (Exception e)
                {
                    _logger.LogError(e, $"Could not deserialize report Xml for { typeof(T).Name }");

                    if (string.IsNullOrEmpty(businessUnit) || businessUnit.ToLower() == "all")
                    {
                        businessUnit = ExtractBUFromString(match.Value);
                    }

                    unsuccessfulItems.Add(new ProcessingResult<IUnparsable>()
                    {
                        ProcessedItem = new Unparsable<T>()
                        {
                            //TreatAsDataType = typeof(T),
                            BusinessUnits = new[] { businessUnit },
                            Error = e.Message,
                            Status = "Unparsable Xml",
                            Xml = match.Value,
                            MessageType = "Detail"
                        },
                    });
                }
            }

            return new ProcessingResultSet<T>() { ProcessedItems = successfulItems, UnparsableItems = unsuccessfulItems };
        }

        private string ExtractBUFromString(string item)
        {
            item = item.ToLower();
            string bu = "all";

            if (item.Contains($"<{BUSINESSUNIT_TAG_BU}>"))
            {
                bu = ExtractString(BUSINESSUNIT_TAG_BU, item);
            }
            else if (item.Contains($"<{SUBLEDGER_TAG}>"))
            {
                bu = ExtractString(SUBLEDGER_TAG, item);
            }
            else if (item.Contains($"<{PROCUREMENT_TAG}>"))
            {
                bu = ExtractString(PROCUREMENT_TAG, item);
            }
            else
            {
                // DO NOTHING
            }

            return bu;
        }

        private string ExtractString(string tagName, string item)
        {
            Regex rgx = new Regex($"<{tagName}>.*?</{tagName}>");
            var itemElement = rgx.Match(item);
            return itemElement.Value.Replace($"<{tagName}>", "")
                                  .Replace($"</{tagName}>", "");
        }
    }
}