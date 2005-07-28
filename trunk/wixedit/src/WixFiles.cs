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
        FileInfo wxsFile;

        UndoManager undoManager;

        XmlDocument wxsDocument;
        XmlNamespaceManager wxsNsmgr;

        static XmlDocument xsdDocument;
        static XmlNamespaceManager xsdNsmgr;

        static WixFiles() {
            ReloadXsd();
        }

        public WixFiles(FileInfo wxsFileInfo) {
            wxsFile = wxsFileInfo;

            wxsDocument = new XmlDocument();
            wxsDocument.Load(wxsFile.FullName);
            
            wxsNsmgr = new XmlNamespaceManager(wxsDocument.NameTable);
            wxsNsmgr.AddNamespace("wix", wxsDocument.DocumentElement.NamespaceURI);

            undoManager = new UndoManager(wxsDocument);
        }

        public UndoManager UndoManager {
            get {
                return undoManager;
            }
        }

        public static void ReloadXsd() {
            xsdDocument = new XmlDocument();

            if (File.Exists(WixEditSettings.Instance.WixBinariesDirectory.Xsd)) {
                xsdDocument.Load(WixEditSettings.Instance.WixBinariesDirectory.Xsd);
            } else {
                xsdDocument.Load(WixFiles.GetResourceStream("wix.xsd"));
            }
        
            xsdNsmgr = new XmlNamespaceManager(xsdDocument.NameTable);
            xsdNsmgr.AddNamespace("xs", "http://www.w3.org/2001/XMLSchema");
        }

        public static XmlDocument GetXsdDocument() {
            return xsdDocument;
        }

        public static XmlNamespaceManager GetXsdNsmgr() {
            return xsdNsmgr;
        }

        public XmlDocument WxsDocument {
            get { return wxsDocument; }
        }

        public XmlNamespaceManager WxsNsmgr {
            get { return wxsNsmgr; }
        }

        public XmlDocument XsdDocument {
            get { return xsdDocument; }
        }

        public XmlNamespaceManager XsdNsmgr {
            get { return xsdNsmgr; }
        }

        public FileInfo WxsFile {
            get { return wxsFile; }
        }

        public DirectoryInfo WxsDirectory {
            get { return wxsFile.Directory; }
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
            wxsDocument = null;
            wxsNsmgr = null;
        }

        #endregion

        public void Save() {
            wxsDocument.Save(wxsFile.FullName);

            undoManager.Clear();
        }
    }
}