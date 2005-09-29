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
using System.Collections;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using System.Windows.Forms;

using WixEdit.PropertyGridExtensions;

namespace WixEdit.Settings {
    public enum PathHandling {
        UseRelativePathsWhenPossible = 0,
        ForceRelativePaths = 1,
        ForceAbolutePaths = 2
    }

    [DefaultPropertyAttribute("BinDirectory")]
    public class WixEditSettings : PropertyAdapterBase {
        [XmlRoot("WixEdit")]
        public class WixEditData {
            public WixEditData() {
                UseRelativeOrAbsolutePaths = PathHandling.UseRelativePathsWhenPossible;
                ExternalXmlEditor = Path.Combine(Environment.SystemDirectory, "notepad.exe");

                EditDialog = new EditDialogData();

                Version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            }

            public WixEditData(WixEditData oldVersion) {
                BinDirectory = oldVersion.BinDirectory;
                DarkLocation = oldVersion.DarkLocation;
                CandleLocation = oldVersion.CandleLocation;
                LightLocation = oldVersion.LightLocation;
                XsdLocation = oldVersion.XsdLocation;
                TemplateDirectory = oldVersion.TemplateDirectory;
                DefaultProjectDirectory = oldVersion.DefaultProjectDirectory;
                UseRelativeOrAbsolutePaths = oldVersion.UseRelativeOrAbsolutePaths;
                ExternalXmlEditor = oldVersion.ExternalXmlEditor;
                if (ExternalXmlEditor == null || ExternalXmlEditor.Length == 0) {
                    ExternalXmlEditor = Path.Combine(Environment.SystemDirectory, "notepad.exe");
                }
                
                if (oldVersion.EditDialog == null) {
                    EditDialog = new EditDialogData();
                } else {
                    EditDialog = oldVersion.EditDialog;
                }

                Version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            }

            public string BinDirectory;
            public string DarkLocation;
            public string CandleLocation;
            public string LightLocation;
            public string XsdLocation;
            public string TemplateDirectory;
            public string ExternalXmlEditor;
            public string DefaultProjectDirectory;
            public string Version;
            public PathHandling UseRelativeOrAbsolutePaths;

            public EditDialogData EditDialog;
        }
        public class EditDialogData {
            public int SnapToGrid = 5;
            public double Scale = 1.00;
            public double Opacity = 1.00;
            public bool AlwaysOnTop = false;
        }

        private static string filename = "WixEditSettings.xml";
        // private static string defaultXml = "<WixEdit><EditDialog /></WixEdit>";

        protected WixEditData data;

        public readonly static WixEditSettings Instance = new WixEditSettings();
        
        private WixEditSettings() : base(null) {
            LoadFromDisk();
        }

        public WixEditData GetInternalDataStructure() {
            return data;
        }

        private string SettingsFile {
            get {
                return Path.Combine(new FileInfo(Assembly.GetExecutingAssembly().Location).DirectoryName, filename);
            }
        }

        void LoadFromDisk() {
            Stream xmlStream = null;
            if (File.Exists(SettingsFilename)) {
                // A FileStream is needed to read the XML document.
                xmlStream = new FileStream(SettingsFilename, FileMode.Open);

                using (xmlStream) {
                    // Create an instance of the XmlSerializer class;
                    // specify the type of object to be deserialized.
                    XmlSerializer serializer = new XmlSerializer(typeof(WixEditData));
    
                    // If the XML document has been altered with unknown 
                    // nodes or attributes, handle them with the 
                    // UnknownNode and UnknownAttribute events.
                    serializer.UnknownNode += new XmlNodeEventHandler(DeserializeUnknownNode);
                    serializer.UnknownAttribute += new XmlAttributeEventHandler(DeserializeUnknownAttribute);
                
    
                    // Use the Deserialize method to restore the object's state with
                    // data from the XML document
                    data = (WixEditData) serializer.Deserialize(xmlStream);
                }
            } else {
                data = new WixEditData();
            }


            try {
                if (data.Version == null) {
                    data = new WixEditData(data);
                } else {
                    Version current = GetCurrentVersion();
                    Version old = new Version(data.Version);

                    if (current.CompareTo(old) != 0) {
                        // Ok, watch out.
                        if (current.CompareTo(old) < 0) {
                            // This is a config file of a future version.
                            MessageBox.Show("The version of the configuration file is newer than the version of this application, if any problems occur remove the WixEditSettings.xml from the directory where WixEdit.exe is located.", "Configuration file", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            data = new WixEditData(data);
                        } else {
                            // This is a config file of an old version.
                            data = new WixEditData(data);

                            if (File.Exists(SettingsFilename)) {
                                string oldFileName = SettingsFilename + "_v" + old.ToString();
                                while (File.Exists(oldFileName)) {
                                    oldFileName = oldFileName + "_";
                                }

                                File.Copy(SettingsFilename, oldFileName);
                            }
                        }

                        SaveChanges();
                    }
                }
            } catch {
                MessageBox.Show("Failed to convert the existing configuration file to the current version, using a default configuration.", "Configuration file", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                data = new WixEditData();
            }
        }


        public void DiscardChanges() {
            LoadFromDisk();
        }

        private string SettingsFilename {
            get {
                return Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), filename);
            }
        }

        public void SaveChanges() {
            XmlSerializer ser = new XmlSerializer(typeof(WixEditData));
            // A FileStream is used to write the file.

            FileMode mode = FileMode.OpenOrCreate;
            if (File.Exists(SettingsFile)) {
                mode = mode|FileMode.Truncate;
            }

            FileStream fs = new FileStream(SettingsFile, mode);

            ser.Serialize(fs, data);
            fs.Close();
        }

        [
        Category("Locations"), 
        Description("The directory where the WiX binaries are located. The wix.xsd is also being located by this path."), 
        Editor(typeof(BinDirectoryStructureEditor), typeof(System.Drawing.Design.UITypeEditor)),
        TypeConverter(typeof(BinDirectoryStructure.BinDirectoryExpandableObjectConverter))
        ]
        public BinDirectoryStructure WixBinariesDirectory {
            get {
                if (data.BinDirectory == null && data.CandleLocation == null && data.DarkLocation == null && data.LightLocation == null && data.XsdLocation == null) {
                    // With the installation of WixEdit the WiX toolset binaries are installed in "..\wix*", 
                    // relative to the WixEdit binary.
                    DirectoryInfo parent = new FileInfo(Assembly.GetExecutingAssembly().Location).Directory.Parent;
                    if (parent != null) {
                        foreach (DirectoryInfo dir in parent.GetDirectories("wix*")) {
                            foreach (FileInfo file in dir.GetFiles("*.exe")) {
                                if (file.Name.ToLower().Equals("candle.exe")) {
                                    data.BinDirectory = dir.FullName;
                                    break;
                                }
                            }
                        }
                    }
                }

                return new BinDirectoryStructure(data);
            }
            set {
                if (value.HasSameBinDirectory()) {
                    data.BinDirectory = new FileInfo(value.Candle).Directory.FullName;
                } else {
                    data.CandleLocation = value.Candle;
                    data.LightLocation = value.Light;
                    data.DarkLocation = value.Dark;
                    data.XsdLocation = value.Xsd;
                    data.BinDirectory = value.BinDirectory;
                }
            }
        }

        [
        Category("Locations"), 
        Description("The directory where the WixEdit templates are located."), 
        Editor(typeof(BinDirectoryStructureEditor), typeof(System.Drawing.Design.UITypeEditor))
        ]
        public string TemplateDirectory {
            get {
                if (data.TemplateDirectory != null && data.TemplateDirectory.Length > 0) {
                    return data.TemplateDirectory;
                }

                // With the installation of WixEdit the WixEdit Templates are installed in "..\templates", 
                // relative to the WixEdit binary.
                DirectoryInfo parent = new FileInfo(Assembly.GetExecutingAssembly().Location).Directory.Parent;
                if (parent != null) {
                    string templateDir = Path.Combine(parent.FullName, "templates");
                    if (Directory.Exists(templateDir)) {
                        return templateDir;
                    }
                }

                return String.Empty;
            }
            set {
                data.TemplateDirectory = value;
            }
        }

        [
        Category("Locations"), 
        Description("The location of your favourite xml editor."), 
        Editor(typeof(FilteredFileNameEditor), typeof(System.Drawing.Design.UITypeEditor)),
        FilteredFileNameEditor.Filter("*.exe |*.exe")
        ]
        public string ExternalXmlEditor {
            get {
                if (data.ExternalXmlEditor != null && data.ExternalXmlEditor.Length > 0) {
                    return data.ExternalXmlEditor;
                }

                return String.Empty;
            }
            set {
                data.ExternalXmlEditor = value;
            }
        }

        [
        Category("Locations"), 
        Description("The default directory where WixEdit creates projects."), 
        Editor(typeof(BinDirectoryStructureEditor), typeof(System.Drawing.Design.UITypeEditor))
        ]
        public string DefaultProjectDirectory {
            get {
                return data.DefaultProjectDirectory;
            }
            set {
                data.DefaultProjectDirectory = value;
            }
        }

        [
        Category("Miscellaneous"), 
        Description("Use relative or absolute paths.")
        ]
        public PathHandling UseRelativeOrAbsolutePaths {
            get {
                return data.UseRelativeOrAbsolutePaths;
            }
            set {
                data.UseRelativeOrAbsolutePaths = value;
            }
        }

        [
        Category("Version"), 
        Description("The version number of the WixEdit application."), 
        ReadOnly(true)
        ]
        public string ApplicationVersion {
            get { return GetCurrentVersion().ToString(); }
            set {}
        }

        private Version GetCurrentVersion() {
            return Assembly.GetExecutingAssembly().GetName().Version;
        }

        #region EditDialog properties

        [
        Category("Dialog Editor Settings"),
        Description("Number of pixels to snap to in the dialog edior. (Mimimal 1 pixel)"),
        Browsable(false)
        ]
        public int SnapToGrid {
            get {
                // Minimum of 1 pixel
                return Math.Max(1, data.EditDialog.SnapToGrid);
            }
            set {
                data.EditDialog.SnapToGrid = value;
            }
        }

        [
        Category("Dialog Editor Settings"),
        Description("Scale of the dialog in the dialog designer. (For example: 0.50 or 0,50 depending on your regional settings.)"),
        Browsable(false)
        ]
        public double Scale {
            get {
                return data.EditDialog.Scale;
            }
            set {
                data.EditDialog.Scale = value;
            }
        }

        [
        Category("Dialog Editor Settings"),
        Description("Opacity of the dialog in the dialog designer. (For example: 0.50 or 0,50 depending on your regional settings.)"),
        Browsable(false)
        ]
        public double Opacity {
            get {
                // Default to 5 pixels
                return Math.Min(1.00, data.EditDialog.Opacity);
            }
            set {
                data.EditDialog.Opacity = value;
            }
        }

        [
        Category("Dialog Editor Settings"),
        Description("Keeps the dialog in the dialog designer on top of everything."),
        Browsable(false)
        ]
        public bool AlwaysOnTop {
            get {
                return data.EditDialog.AlwaysOnTop;
            }
            set {
                data.EditDialog.AlwaysOnTop = value;
            }
        }


        #endregion

        #region Serialization helpers

        static protected void DeserializeUnknownNode(object sender, XmlNodeEventArgs e) {
            MessageBox.Show("Ignoring Unknown Node from settings file: " +   e.Name);
        }
        
        static protected void DeserializeUnknownAttribute(object sender, XmlAttributeEventArgs e) {
            System.Xml.XmlAttribute attr = e.Attr;
            MessageBox.Show("Ignoring Unknown Attribute from settings file: " + attr.Name + " = '" + attr.Value + "'");
        }

        #endregion

        #region PropertyAdapterBase overrides

        public override PropertyDescriptorCollection GetProperties(Attribute[] attributes) {
            ArrayList propertyDescriptors = new ArrayList();
            foreach (PropertyInfo propInfo in GetType().GetProperties(BindingFlags.Public | BindingFlags.DeclaredOnly | BindingFlags.Instance)) {
                ArrayList atts = new ArrayList(propInfo.GetCustomAttributes(false));
                propertyDescriptors.Add(new CustomDisplayNamePropertyDescriptor(wixFiles, propInfo, (Attribute[]) atts.ToArray(typeof(Attribute))));
            }

            return new PropertyDescriptorCollection((PropertyDescriptor[]) propertyDescriptors.ToArray(typeof(PropertyDescriptor)));
        }

        #endregion
   }
}