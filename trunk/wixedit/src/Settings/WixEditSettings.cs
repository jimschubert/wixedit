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
using System.IO;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using System.Windows.Forms;

using WixEdit.PropertyGridExtensions;

namespace WixEdit.Settings {
    [DefaultPropertyAttribute("BinDirectory")]
    public class WixEditSettings : PropertyAdapterBase {
        [XmlRoot("WixEdit")]
        public class WixEditData {
            public WixEditData() {}
            public string BinDirectory;
        }

        private static string filename = "WixEditSettings.xml";
        private static string defaultXml = "<WixEdit />";

        private WixEditData data;
        public readonly static WixEditSettings Instance = new WixEditSettings();
        
        private WixEditSettings() : base(null) {
            LoadFromDisk();
        }

        void LoadFromDisk() {
            Stream xmlStream = null;
            if (File.Exists(SettingsFilename)) {
                // A FileStream is needed to read the XML document.
                xmlStream = new FileStream(SettingsFilename, FileMode.Open);
            } else {
                byte[] data = Encoding.ASCII.GetBytes(defaultXml);
                xmlStream = new MemoryStream(data);
            }

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
            FileStream fs;
            FileMode mode = FileMode.OpenOrCreate;

            if (File.Exists(SettingsFilename)) {
                mode = mode | FileMode.Truncate;
            }

            fs = new FileStream(SettingsFilename, mode);

            ser.Serialize(fs, data);
            fs.Close();
        }

        [
        Category("WiX Settings"), 
        Description("The directory where the WiX binaries are located."), 
        Editor(typeof(System.Windows.Forms.Design.FolderNameEditor), typeof(System.Drawing.Design.UITypeEditor))
        ]
        public string BinDirectory {
            get {
                return data.BinDirectory;
            }
            set {
                data.BinDirectory = value;
            }
        }

        [
        Category("Version"), 
        Description("The version number of the WixEdit application."), 
        ReadOnly(true)
        ]
        public string AppVersion {
            get { return Assembly.GetExecutingAssembly().GetName().Version.ToString(); }
            set {}
        }

        #region Serialization helpers
        static protected void DeserializeUnknownNode(object sender, XmlNodeEventArgs e) {
            MessageBox.Show("Ignoring Unknown Node: " +   e.Name + "='" + e.Text + "'");
        }
        
        static protected void DeserializeUnknownAttribute(object sender, XmlAttributeEventArgs e) {
            System.Xml.XmlAttribute attr = e.Attr;
            MessageBox.Show("Ignoring Unknown attribute: " + attr.Name + "='" + attr.Value + "'");
        }
        #endregion

        #region PropertyAdapterBase overrides
        public override PropertyDescriptorCollection GetProperties(Attribute[] attributes) {
            ArrayList propertyDescriptors = new ArrayList();
            foreach (PropertyInfo propInfo in this.GetType().GetProperties(BindingFlags.Public | BindingFlags.DeclaredOnly | BindingFlags.Instance)) {
                ArrayList atts = new ArrayList(propInfo.GetCustomAttributes(false));
                propertyDescriptors.Add(new CustomDisplayNamePropertyDescriptor(propInfo, (Attribute[]) atts.ToArray(typeof(Attribute))));
            }

            return new PropertyDescriptorCollection((PropertyDescriptor[]) propertyDescriptors.ToArray(typeof(PropertyDescriptor)));
        }
        #endregion

    }
}