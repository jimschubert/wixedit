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
using System.Text;
using System.Xml;
using System.Windows.Forms;

using WixEdit.Settings;

namespace WixEdit {
    public class WixFiles : IDisposable {
        FileInfo wxsFile;

        UndoManager undoManager;

        XmlDocument wxsDocument;
        XmlNamespaceManager wxsNsmgr;

        ProjectSettings projectSettings;

        FileSystemWatcher wxsWatcher;
        FileSystemEventHandler wxsWatcher_ChangedHandler;
        public event EventHandler wxsChanged;

        static XmlDocument xsdDocument;
        static XmlNamespaceManager xsdNsmgr;

        static WixFiles() {
            ReloadXsd();
        }

        public WixFiles(FileInfo wxsFileInfo) {
            wxsFile = wxsFileInfo;
            
            LoadWxsFile();

            wxsNsmgr = new XmlNamespaceManager(wxsDocument.NameTable);
            wxsNsmgr.AddNamespace("wix", wxsDocument.DocumentElement.NamespaceURI);

            undoManager = new UndoManager(wxsDocument);

            wxsWatcher = new FileSystemWatcher(wxsFile.Directory.FullName, wxsFile.Name);
            wxsWatcher_ChangedHandler = new FileSystemEventHandler(wxsWatcher_Changed);
            wxsWatcher.Changed += wxsWatcher_ChangedHandler;
            wxsWatcher.EnableRaisingEvents = true;
        }

        public ProjectSettings ProjectSettings {
            get {
                return projectSettings;
            }
        }

        public bool ReadOnly() {
            return wxsFile.Exists && ((wxsFile.Attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly);
        }

        public void LoadWxsFile() {
            if (wxsDocument == null) {
                wxsDocument = new XmlDocument();
            }
            
            if (ReadOnly()) {
                MessageBox.Show(String.Format("\"{0}\" is readony.", wxsFile.Name), "Read Only!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            FileMode mode = FileMode.Open;
            using(FileStream fs = new FileStream(wxsFile.FullName, mode, FileAccess.Read, FileShare.Read)) {
                wxsDocument.Load(fs);
                fs.Close();
            }


            XmlNode possibleComment = wxsDocument.FirstChild.NextSibling;
            if (possibleComment != null && possibleComment.Name == "#comment") {
                string comment = possibleComment.Value;

                string candleArgs = String.Empty;
                string lightArgs = String.Empty;
                bool foundArg = false;
                foreach (string fullLine in comment.Split('\r', '\n')) {
                    string line = fullLine.Trim();
                    if (line.Length == 0) {
                        continue;
                    }

                    string candleStart = "candleargs:";
                    if (line.ToLower().StartsWith(candleStart)) {
                        candleArgs = line.Remove(0, candleStart.Length);
                        foundArg = true;
                    }

                    string lightStart = "lightargs:";
                    if (line.ToLower().StartsWith(lightStart)) {
                        lightArgs = line.Remove(0, lightStart.Length);
                        foundArg = true;
                    }
                }

                if (foundArg == true) {
                    wxsDocument.RemoveChild(possibleComment);
                }

                projectSettings = new ProjectSettings(candleArgs.Trim(), lightArgs.Trim());
            } else {
                projectSettings = new ProjectSettings(String.Empty, String.Empty);
            }
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
            xsdNsmgr.AddNamespace("xse", "http://schemas.microsoft.com/wix/2005/XmlSchemaExtension");
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
            wxsWatcher.EnableRaisingEvents = false;
            wxsWatcher.Changed -= wxsWatcher_ChangedHandler;
            wxsWatcher.Dispose();
            wxsWatcher = null;

            wxsDocument = null;
            wxsNsmgr = null;
        }

        #endregion

        public bool HasChanges() {
            return (!ReadOnly() && (UndoManager.HasChanges() || projectSettings.HasChanges()));
        }

        public void SaveAs(string newFile) {
            wxsFile = new FileInfo(newFile);

            wxsWatcher.EnableRaisingEvents = false;
            wxsWatcher.Changed -= wxsWatcher_ChangedHandler;

            wxsWatcher = new FileSystemWatcher(wxsFile.Directory.FullName, wxsFile.Name);
            wxsWatcher.Changed += wxsWatcher_ChangedHandler;
            
            Save();
        }

        public void Save() {
            if (ReadOnly()) {
                MessageBox.Show(String.Format("\"{0}\" is readony, cannot save this file.", wxsFile.Name), "Read Only!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            wxsWatcher.EnableRaisingEvents = false;

            XmlComment commentElement = null;
            if (projectSettings.IsEmpty() == false) {
                StringBuilder commentBuilder = new StringBuilder();
                commentBuilder.Append("\r\n");
                commentBuilder.Append("    # This comment is generated by WixEdit, the specific commandline\r\n");
                commentBuilder.Append("    # arguments for the WiX Toolset are stored here.\r\n\r\n");
                commentBuilder.AppendFormat("    candleArgs: {0}\r\n", projectSettings.CandleArgs);
                commentBuilder.AppendFormat("    lightArgs: {0}\r\n", projectSettings.LightArgs);

                commentElement = wxsDocument.CreateComment(commentBuilder.ToString());
                wxsDocument.InsertBefore(commentElement, wxsDocument.DocumentElement);
            }

            FileMode mode = FileMode.OpenOrCreate;
            if (File.Exists(wxsFile.FullName)) {
                mode = mode|FileMode.Truncate;
            }

            using(FileStream fs = new FileStream(wxsFile.FullName, mode)) {
                XmlTextWriter writer = new XmlTextWriter(fs, new System.Text.UTF8Encoding());
                writer.Formatting = Formatting.Indented;
                writer.Indentation = WixEditSettings.Instance.XmlIndentation;
    
                wxsDocument.Save(writer);
    
                writer.Close();
                fs.Close();
            }

            projectSettings.ChangesHasBeenSaved();

            if (commentElement != null) {
                wxsDocument.RemoveChild(commentElement);
            }

            wxsWatcher.EnableRaisingEvents = true;

            undoManager.Clear();
        }

        private void wxsWatcher_Changed(object sender, FileSystemEventArgs e) {
            wxsWatcher.EnableRaisingEvents = false;

            DialogResult result = DialogResult.None;
            if (undoManager.HasChanges()) {
                result = MessageBox.Show(String.Format("An external program changed \"{0}\", do you want to load the changes from disk and ignore the changes in memory?", wxsFile.Name), "Reload?", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            } else {
                result = MessageBox.Show(String.Format("An external program changed \"{0}\", do you want to load the changes from disk?", wxsFile.Name), "Reload?", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            }

            if (result == DialogResult.Yes) {
                LoadWxsFile();
                UndoManager.Clear();

                if (wxsChanged != null) {
                    wxsChanged(this, new EventArgs());
                }
            }

            wxsWatcher.EnableRaisingEvents = true;
        }
    }

    public class ProjectSettings {
        private bool hasChanges;
        private string candleArgs;
        private string lightArgs;

        public readonly string DefaultCandleArgs = "\"<projectfile>\" -out \"<projectname>.wixobj\"";
        public readonly string DefaultLightArgs = "\"<projectname>.wixobj\" -out \"<projectname>.msi\"";

        public ProjectSettings(string candleArguments, string lightArguments) {
            candleArgs = candleArguments;
            lightArgs = lightArguments;

            hasChanges = false;
        }

        public string CandleArgs {
            get {
                return candleArgs;
            }
            set {
                candleArgs = value;
                hasChanges = true;
            }
        }
        public string LightArgs {
            get {
                return lightArgs;
            }
            set {
                lightArgs = value;
                hasChanges = true;
            }
        }

        public bool IsEmpty() {
            return ((lightArgs == String.Empty) && (candleArgs == String.Empty));
        }

        public bool HasChanges() {
            return hasChanges;
        }

        public void ChangesHasBeenSaved() {
            hasChanges = false;
        }
    }
}