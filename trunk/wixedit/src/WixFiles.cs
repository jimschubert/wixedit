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
using System.Reflection;
using System.Xml;

namespace WixEdit {
    public class WixFiles : IDisposable {
        FileInfo _wxsFile;

        XmlDocument _wxsDocument;
        XmlNamespaceManager _wxsNsmgr;

        XmlDocument _xsdDocument;
        XmlNamespaceManager _xsdNsmgr;

        public WixFiles(FileInfo wxsFile) {
            _wxsFile = wxsFile;

            this._wxsDocument = new XmlDocument();
            this._wxsDocument.Load(wxsFile.FullName);
            
            this._wxsNsmgr = new XmlNamespaceManager(this._wxsDocument.NameTable);
            this._wxsNsmgr.AddNamespace("wix", this._wxsDocument.DocumentElement.NamespaceURI);

            this._xsdDocument = new XmlDocument();
            this._xsdDocument.Load(WixFiles.GetResourceStream("WixEdit.wix.xsd"));
            
            this._xsdNsmgr = new XmlNamespaceManager(this._xsdDocument.NameTable);
            this._xsdNsmgr.AddNamespace("xs", "http://www.w3.org/2001/XMLSchema");
        }

        public XmlDocument WxsDocument {
            get { return this._wxsDocument; }
        }

        public XmlNamespaceManager WxsNsmgr {
            get { return this._wxsNsmgr; }
        }

        public XmlDocument XsdDocument {
            get { return this._xsdDocument; }
        }

        public XmlNamespaceManager XsdNsmgr {
            get { return this._xsdNsmgr; }
        }

        public FileInfo WxsFile {
            get { return this._wxsFile; }
        }

        public DirectoryInfo WxsDirectory {
            get { return this._wxsFile.Directory; }
        }

        public static Stream GetResourceStream(string resourceName) {
            Assembly assembly = Assembly.GetExecutingAssembly();
            if (assembly.GetManifestResourceInfo(resourceName) == null) {
                throw new Exception("Could not find resource: " + resourceName);
            }

            Stream resourceStream = assembly.GetManifestResourceStream(resourceName);
            if (resourceStream == null) {
                throw new Exception("Could not load resource: " +  resourceName);
            }

            return resourceStream;
        }

        #region IDisposable Members

        public void Dispose() {
            this._wxsDocument = null;
            this._wxsNsmgr = null;

            this._xsdDocument = null;
            this._xsdNsmgr = null;
        }

        #endregion
    }
}