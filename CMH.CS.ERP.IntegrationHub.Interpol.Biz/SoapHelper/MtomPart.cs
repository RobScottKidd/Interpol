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
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.ServiceModel.Channels;
using System.Text;
using System.Text.RegularExpressions;

namespace WcfCoreMtomEncoder
{
    internal class MtomPart
    {
        private readonly HttpContent _part;

        public MtomPart(HttpContent part)
        {
            _part = part;
        }

        public MediaTypeHeaderValue ContentType => _part.Headers.ContentType;
        public string ContentTransferEncoding => _part.Headers.TryGetValues("Content-Transfer-Encoding", out var values) ? values.Single() : null;
        public string ContentId => _part.Headers.TryGetValues("Content-ID", out var values) ? values.Single() : null;

        public byte[] GetRawContent()
        {
            if (!Regex.IsMatch(ContentTransferEncoding, "((7|8)bit)|binary", RegexOptions.IgnoreCase))
                throw new NotSupportedException();

            return ReadFromStream();
        }

        public string GetStringContentForEncoder(MessageEncoder encoder)
        {
            if (!ContentType.Parameters.Any(p => p.Name == "type" && encoder.IsContentTypeSupported(p.Value.Replace("\"", ""))))
                throw new NotSupportedException();

            var encoding = ContentType.CharSet != null ? Encoding.GetEncoding(ContentType.CharSet) : Encoding.Default;

            return encoding.GetString(GetRawContent());
        }

        private byte[] ReadFromStream()
        {
            using (var buffer = new MemoryStream())
            {
                _part.ReadAsStreamAsync().Result.CopyTo(buffer);
                return buffer.ToArray();
            }
        }
    }
}