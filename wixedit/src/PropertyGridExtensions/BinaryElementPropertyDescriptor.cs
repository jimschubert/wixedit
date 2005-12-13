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
using System.Windows.Forms;
using System.Xml;

using WixEdit.Settings;

namespace WixEdit.PropertyGridExtensions {
    /// <summary>
    /// PropertyDescriptor for BinaryElements.
    /// </summary>
    public class BinaryElementPropertyDescriptor : CustomXmlPropertyDescriptorBase {
        public BinaryElementPropertyDescriptor(XmlNode binaryElement, WixFiles wixFiles, string name, Attribute[] attrs) :
            base(wixFiles, binaryElement, name, attrs) {
        }

        public override object GetValue(object component) {
            return XmlElement.Attributes["src"].Value;
        }

        public override void SetValue(object component, object value) {
            wixFiles.UndoManager.BeginNewCommandRange();

            if (value == null) {
                XmlElement.Attributes["src"].Value = String.Empty;
            } else {

                string sepCharString = Path.DirectorySeparatorChar.ToString();
                string path = value.ToString();

                if (File.Exists(Path.GetFullPath(path)) == false) {
                    MessageBox.Show(String.Format("{0} could not be located", path), "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    XmlElement.Attributes["src"].Value = path;
                } else {
                  if (WixEditSettings.Instance.UseRelativeOrAbsolutePaths == PathHandling.ForceAbolutePaths) {
                      XmlElement.Attributes["src"].Value = Path.GetFullPath(path);
                  } else {
                      XmlElement.Attributes["src"].Value = RelativePathHelper.GetRelativePath(value as string, wixFiles);
                  }
                }
            }
        }

        public override bool CanResetValue(object component) {
            return (GetValue(component).Equals("") == false);
        }
    }
}
