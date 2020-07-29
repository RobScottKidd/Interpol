using CMH.CS.ERP.IntegrationHub.Interpol.Interfaces.Biz;
using CMH.CS.ERP.IntegrationHub.Interpol.Interfaces.Configuration;
using CMH.CSS.ERP.IntegrationHub.CanonicalModels.Enumerations;
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading.Tasks;

namespace CMH.CS.ERP.IntegrationHub.Interpol.Biz
{
    /// <summary>
    /// Implementation of IFileExporter used to export report dumps from Oracle
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class FileExporter : IFileExporter
    {
        private readonly string destinationFolder;

        /// <summary>
        /// Constructor with DI'ed config
        /// </summary>
        /// <param name="config"></param>
        public FileExporter(IInterpolConfiguration config)
        {
            destinationFolder = config?.ExportDirectory;
        }

        /// <inheritdoc/>
        public string GenerateFilePath(DataTypes datatype) => Path.Join(destinationFolder, $"{datatype}_DUMP_{DateTime.UtcNow.ToFileTimeUtc()}.xml");

        /// <inheritdoc/>
        public async Task<string> Export(DataTypes datatype, string content)
        {
            string filePath = GenerateFilePath(datatype);
            await File.WriteAllTextAsync(filePath, content);
            return filePath;
        }

        /// <inheritdoc/>
        public async Task Export(string fileName, string content) => await File.WriteAllTextAsync(fileName, content);
    }
}