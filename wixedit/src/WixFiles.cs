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

using WixEdit.Settings;

namespace WixEdit {
    public class WixFiles : IDisposable {
        FileInfo _wxsFile;

        XmlDocument _wxsDocument;
        XmlNamespaceManager _wxsNsmgr;

        static XmlDocument _xsdDocument;
        static XmlNamespaceManager _xsdNsmgr;

        static WixFiles() {
            ReloadXsd();
        }

        public WixFiles(FileInfo wxsFile) {
            _wxsFile = wxsFile;

            this._wxsDocument = new XmlDocument();
            this._wxsDocument.Load(wxsFile.FullName);
            
            this._wxsNsmgr = new XmlNamespaceManager(this._wxsDocument.NameTable);
            this._wxsNsmgr.AddNamespace("wix", this._wxsDocument.DocumentElement.NamespaceURI);
        }

        public static void ReloadXsd() {
            _xsdDocument = new XmlDocument();

            if (WixEditSettings.Instance.BinDirectory != null &&
                Directory.Exists(WixEditSettings.Instance.BinDirectory) &&
                ( File.Exists(Path.Combine(WixEditSettings.Instance.BinDirectory, "wix.xsd")) ||
                File.Exists(Path.Combine(WixEditSettings.Instance.BinDirectory, "doc\\wix.xsd")))) {
                if (File.Exists(Path.Combine(WixEditSettings.Instance.BinDirectory, "doc\\wix.xsd"))) {
                    _xsdDocument.Load(Path.Combine(WixEditSettings.Instance.BinDirectory, "doc\\wix.xsd"));
                } else {
                    _xsdDocument.Load(Path.Combine(WixEditSettings.Instance.BinDirectory, "wix.xsd"));
                }
            } else {
                _xsdDocument.Load(WixFiles.GetResourceStream("wix.xsd"));
            }
        
            _xsdNsmgr = new XmlNamespaceManager(_xsdDocument.NameTable);
            _xsdNsmgr.AddNamespace("xs", "http://www.w3.org/2001/XMLSchema");
        }

        public static XmlDocument GetXsdDocument() {
            return _xsdDocument;
        }

        public static XmlNamespaceManager GetXsdNsmgr() {
            return _xsdNsmgr;
        }

        public XmlDocument WxsDocument {
            get { return this._wxsDocument; }
        }

        public XmlNamespaceManager WxsNsmgr {
            get { return this._wxsNsmgr; }
        }

        public XmlDocument XsdDocument {
            get { return _xsdDocument; }
        }

        public XmlNamespaceManager XsdNsmgr {
            get { return _xsdNsmgr; }
        }

        public FileInfo WxsFile {
            get { return this._wxsFile; }
        }

        public DirectoryInfo WxsDirectory {
            get { return this._wxsFile.Directory; }
        }

        public static Stream GetResourceStream(string resourceName) {
            string resourceNamespace = "WixEdit.res.";
            Assembly assembly = Assembly.GetExecutingAssembly();
            if (assembly.GetManifestResourceInfo(resourceNamespace + resourceName) == null) {
                throw new Exception("Could not find resource: " + resourceNamespace + resourceName);
            }

            Stream resourceStream = assembly.GetManifestResourceStream(resourceNamespace + resourceName);
            if (resourceStream == null) {
                throw new Exception("Could not load resource: " + resourceNamespace + resourceName);
            }

            return resourceStream;
        }

        #region IDisposable Members

        public void Dispose() {
            _wxsDocument = null;
            _wxsNsmgr = null;
        }

        #endregion

        public void Save() {
            this._wxsDocument.Save(_wxsFile.FullName);
        }
    }
}