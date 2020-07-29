/*
MIT License
Copyright (c) 2019 Lenny Kean
Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:
The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.
THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.ServiceModel.Channels;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace WcfCoreMtomEncoder
{
    public class MtomMessageEncoder : MessageEncoder
    {
        private readonly MessageEncoder _innerEncoder;

        public MtomMessageEncoder(MessageEncoder innerEncoder)
        {
            _innerEncoder = innerEncoder;
        }

        public override string ContentType => _innerEncoder.ContentType;
        public override string MediaType => _innerEncoder.MediaType;
        public override MessageVersion MessageVersion => _innerEncoder.MessageVersion;

        public override Message ReadMessage(ArraySegment<byte> buffer, BufferManager bufferManager, string contentType)
        {
            using (var stream = new MemoryStream(buffer.ToArray()))
            {
                var message = ReadMessage(stream, 1024, contentType);
                bufferManager.ReturnBuffer(buffer.Array);
                return message;
            }
        }

        public override Message ReadMessage(Stream stream, int maxSizeOfHeaders, string contentType)
        {
            var content = new StreamContent(stream);

            if (content.Headers.ContentType == null && MediaTypeHeaderValue.TryParse(contentType, out var mediaType))
            {
                content.Headers.ContentType = mediaType;
            }

            if (content.IsMimeMultipartContent())
            {
                var parts = (
                    from p in GetMultipartContent(content, contentType)
                    select new MtomPart(p)).ToList();

                var mainPart = (
                    from part in parts
                    where part.ContentId == new ContentType(contentType).Parameters?["start"]
                    select part).SingleOrDefault() ?? parts.First();

                var mainContent = ResolveRefs(mainPart.GetStringContentForEncoder(_innerEncoder), parts);
                var mainContentStream = CreateStream(mainContent, mainPart.ContentType);
                return _innerEncoder.ReadMessage(mainContentStream, maxSizeOfHeaders, mainPart.ContentType.ToString());
            }

            //var mainContent = 
            var contentStream = CreateStream(Encoding.UTF8.GetString(content.ReadAsByteArrayAsync().Result), new MediaTypeHeaderValue("text/xml") { CharSet = "utf-8" });
            return _innerEncoder.ReadMessage(contentStream, maxSizeOfHeaders);
        }

        public override ArraySegment<byte> WriteMessage(Message message, int maxMessageSize, BufferManager bufferManager, int messageOffset)
        {
            return _innerEncoder.WriteMessage(message, maxMessageSize, bufferManager, messageOffset);
        }

        public override void WriteMessage(Message message, Stream stream)
        {
            _innerEncoder.WriteMessage(message, stream);
        }

        public override bool IsContentTypeSupported(string contentType)
        {
            if (_innerEncoder.IsContentTypeSupported(contentType))
                return true;

            var contentTypes = contentType.Split(';').Select(c => c.Trim()).ToList();

            if (contentTypes.Contains("multipart/related", StringComparer.OrdinalIgnoreCase) &&
                contentTypes.Contains("type=\"application/xop+xml\"", StringComparer.OrdinalIgnoreCase))
            {
                return true;
            }
            return false;
        }

        public override T GetProperty<T>()
        {
            return _innerEncoder.GetProperty<T>();
        }

        private static IEnumerable<HttpContent> GetMultipartContent(StreamContent content, string contentType)
        {
            if (content.Headers.ContentType == null)
            {
                content.Headers.Add("Content-Type", contentType);
            }
            return content.ReadAsMultipartAsync().Result.Contents;
        }

        private static string ResolveRefs(string mainContent, IList<MtomPart> parts)
        {
            bool ReferenceMatch(XAttribute hrefAttr, MtomPart part)
            {
                var partId = Regex.Match(part.ContentId, "<(?<uri>.*)>");
                var href = Regex.Match(hrefAttr.Value, "cid:(?<uri>.*)");

                return href.Groups["uri"].Value == partId.Groups["uri"].Value;
            }

            var doc = XDocument.Parse(mainContent);
            var references = doc.Descendants(XName.Get("Include", "http://www.w3.org/2004/08/xop/include")).ToList();

            foreach (var reference in references)
            {
                var referencedPart = (
                    from part in parts
                    where ReferenceMatch(reference.Attribute("href"), part)
                    select part).Single();

                reference.ReplaceWith(Convert.ToBase64String(referencedPart.GetRawContent()));
            }
            return doc.ToString(SaveOptions.DisableFormatting);
        }

        private static Stream CreateStream(string content, MediaTypeHeaderValue contentType)
        {
            var encoding = !string.IsNullOrEmpty(contentType.CharSet)
                ? Encoding.GetEncoding(contentType.CharSet)
                : Encoding.Default;

            return new MemoryStream(encoding.GetBytes(content));
        }
    }
}