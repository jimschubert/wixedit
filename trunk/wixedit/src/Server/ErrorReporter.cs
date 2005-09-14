// Copyright (c) 2005 J.Keuper (j.keuper@gmail.com)
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to 
// deal in the Software without restriction, including without limitation the 
// rights to use, copy, modify, merge, publish, distribute, sublicense, and/or 
// sell copies of the Software, and to permit persons to whom the Software is 
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in 
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE 
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING 
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS
// IN THE SOFTWARE.


using System;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Windows.Forms;

using WixEdit.Settings;

namespace WixEdit {
    public class ErrorReporter {
        protected readonly string formFieldName = "message";
        protected readonly string reportingUrl = "http://wixedit.sourceforge.net/server/report.php";

        public void Report(Exception exception) {
            try {
                // Start building http POST.
                StringBuilder buffer = new StringBuilder();

                string boundary = "----------" + Guid.NewGuid().ToString("N");
            
                buffer.Append("--").Append(boundary).Append("\r\n");
                buffer.Append("Content-Disposition: form-data; name=\"");
                buffer.Append(formFieldName).Append("\"\r\n\r\n");
                buffer.Append(exception.ToString()).Append("\r\n");
            
                buffer.Append("\r\n--").Append(boundary).Append("--\r\n");

                // Create a request.
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(reportingUrl);

                request.Credentials =  CredentialCache.DefaultCredentials;

                request.ContentType = "multipart/form-data; boundary=" + boundary;
                request.Method = "POST";
                request.ContentLength = buffer.Length;

                string stringBuffer = buffer.ToString();

                request.ContentLength = Encoding.ASCII.GetByteCount(stringBuffer);

                Stream requestStream = request.GetRequestStream();

                requestStream.Write(Encoding.ASCII.GetBytes(stringBuffer), 0, Encoding.ASCII.GetByteCount(stringBuffer));
                requestStream.Close();

                // Get response back.
                using (HttpWebResponse httpResponse = (HttpWebResponse)request.GetResponse()) {
                }
            } catch (Exception) {
                MessageBox.Show("Error occured while reporting an error.");
            }
        }
    }
}