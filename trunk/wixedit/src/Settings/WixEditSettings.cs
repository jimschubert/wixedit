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
using System.Text;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using System.Windows.Forms;

namespace WixEdit.Settings {
    [XmlRoot("WixEdit")]
    public class WixEditSettings {
        private static string filename = "WixEditSettings.xml";
        private static string defaultXml = "<WixEdit />";
        private static WixEditSettings instance;

        public static WixEditSettings Instance {
            get {
                if (instance == null) {
                    instance = null;
                }
                return instance;
            }
        }
        
        static WixEditSettings() {
            Stream xmlStream = null;
            if (File.Exists(filename)) {
                // A FileStream is needed to read the XML document.
                xmlStream = new FileStream(filename, FileMode.Open);
            } else {
                byte[] data = Encoding.ASCII.GetBytes(defaultXml);
                xmlStream = new MemoryStream(data);
            }

            using (xmlStream) {
                // Create an instance of the XmlSerializer class;
                // specify the type of object to be deserialized.
                XmlSerializer serializer = new XmlSerializer(typeof(WixEditSettings));
    
                // If the XML document has been altered with unknown 
                // nodes or attributes, handle them with the 
                // UnknownNode and UnknownAttribute events.
                serializer.UnknownNode += new XmlNodeEventHandler(DeserializeUnknownNode);
                serializer.UnknownAttribute += new XmlAttributeEventHandler(DeserializeUnknownAttribute);
                
    
                // Declare an object variable of the type to be deserialized.
                WixEditSettings settings;
                // Use the Deserialize method to restore the object's state with
                // data from the XML document
                settings = (WixEditSettings) serializer.Deserialize(xmlStream);
    
                instance = settings;
            }
        }

        public static void SaveChanges() {
            XmlSerializer ser = new XmlSerializer(typeof(WixEditSettings));
            // A FileStream is used to write the file.
            FileStream fs = new FileStream("WixEditSettings.xml",FileMode.OpenOrCreate|FileMode.Truncate);

            ser.Serialize(fs,instance);
            fs.Close();
        }
          
        string binDirectory;
        public string BinDirectory {
            get {
                return binDirectory;
            }
            set {
                binDirectory = value;
            }
        }

        static protected void DeserializeUnknownNode(object sender, XmlNodeEventArgs e) {
            MessageBox.Show("Ignoring Unknown Node: " +   e.Name + "='" + e.Text + "'");
        }
        
        static protected void DeserializeUnknownAttribute(object sender, XmlAttributeEventArgs e) {
            System.Xml.XmlAttribute attr = e.Attr;
            MessageBox.Show("Ignoring Unknown attribute: " + attr.Name + "='" + attr.Value + "'");
        }

    }
}