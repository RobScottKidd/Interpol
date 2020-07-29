using CMH.CSS.ERP.IntegrationHub.CanonicalModels.Enumerations;
using System.Threading.Tasks;

namespace CMH.CS.ERP.IntegrationHub.Interpol.Interfaces.Biz
{
    /// <summary>
    /// Defines the capabilities of File exporting
    /// </summary>
    public interface IFileExporter
    {
        /// <summary>
        /// Creates and returns the file path used when saving a report data dump
        /// </summary>
        /// <param name="datatype"></param>
        /// <returns></returns>
        string GenerateFilePath(DataTypes datatype);

        /// <summary>
        /// Exports string data to file
        /// </summary>
        /// <param name="datatype">Datatype of file being exported</param>
        /// <param name="content">Text content of the file</param>
        /// <returns></returns>
        Task<string> Export(DataTypes datatype, string content);

        /// <summary>
        /// Exports string data to given file name
        /// </summary>
        /// <param name="fileName">Location to save file</param>
        /// <param name="content">Text content of the file</param>
        /// <returns></returns>
        Task Export(string fileName, string content);
    }
}