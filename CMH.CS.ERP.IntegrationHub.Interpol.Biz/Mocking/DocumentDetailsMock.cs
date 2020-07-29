using System.Text;

namespace CMH.CS.ERP.IntegrationHub.Interpol.Biz
{
    /// <summary>
    /// Mocks the existing DocumentDetails class to allow a plain string to be deserialized from a file.
    /// </summary>
    public class DocumentDetailsMock
    {
        /// <summary>
        /// Default constructor, ususually used during JSON deserilization.
        /// </summary>
        public DocumentDetailsMock() { }

        /// <summary>
        /// Creates a new mock object using the values from the provided real object.
        /// Assumes using UTF-8 encoding for the Content field.
        /// </summary>
        /// <param name="document">A document object</param>
        public DocumentDetailsMock(DocumentDetails document)
        {
            Content = Encoding.UTF8.GetString(document?.Content);
            ContentType = document?.ContentType;
            DocumentAccount = document?.DocumentAccount;
            DocumentAuthor = document?.DocumentAuthor;
            DocumentId = document?.DocumentId;
            DocumentName = document?.DocumentName;
            DocumentSecurityGroup = document?.DocumentSecurityGroup;
            DocumentTitle = document?.DocumentTitle;
            FileName = document?.FileName;
        }

        /// <summary>
        /// Transforms the mock object to a real object, assuming UTF-8 encoding.
        /// </summary>
        /// <returns>The real DocumentDetails object</returns>
        public DocumentDetails AsDocumentDetails() => new DocumentDetails()
        {
            Content = Encoding.UTF8.GetBytes(Content),
            ContentType = ContentType,
            DocumentAccount = DocumentAccount,
            DocumentAuthor = DocumentAuthor,
            DocumentId = DocumentId,
            DocumentName = DocumentName,
            DocumentSecurityGroup = DocumentSecurityGroup,
            DocumentTitle = DocumentTitle,
            FileName = FileName
        };

        public string Content { get; set; }

        public string ContentType { get; set; }

        public string DocumentAccount { get; set; }

        public string DocumentAuthor { get; set; }

        public string DocumentId { get; set; }

        public string DocumentName { get; set; }

        public string DocumentSecurityGroup { get; set; }

        public string DocumentTitle { get; set; }

        public string FileName { get; set; }
    }
}