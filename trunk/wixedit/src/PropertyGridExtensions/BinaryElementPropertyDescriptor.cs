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
using System.Xml;

using WixEdit.Settings;

namespace WixEdit.PropertyGridExtensions {
    /// <summary>
    /// PropertyDescriptor for BinaryElements.
    /// </summary>
    public class BinaryElementPropertyDescriptor : CustomPropertyDescriptorBase {
        XmlNode binaryElement;
        WixFiles wixFiles;

        public BinaryElementPropertyDescriptor(XmlNode binaryElement, WixFiles wixFiles, string name, Attribute[] attrs) :
            base(name, attrs) {
            this.binaryElement = binaryElement;
            this.wixFiles = wixFiles;
        }

        public XmlNode BinaryElement {
            get {
                return binaryElement;
            }
        }

        public override object GetValue(object component) {
            return binaryElement.Attributes["src"].Value;
        }

        public override void SetValue(object component, object value) {
            if (value == null) {
                binaryElement.Attributes["src"].Value = String.Empty;
            } else {
                string sepCharString = Path.DirectorySeparatorChar.ToString();
                string path = value.ToString();
                if (WixEditSettings.Instance.UseRelativeOrAbsolutePaths == PathHandling.ForceAbolutePaths) {
                    if (File.Exists(Path.GetFullPath(path)) == false) {
                        throw new FileNotFoundException(String.Format("{0} could not be located", path), path);
                    }

                    binaryElement.Attributes["src"].Value = Path.GetFullPath(path);
                } else {
                    if (File.Exists(Path.GetFullPath(path)) == false) {
                        throw new FileNotFoundException(String.Format("{0} could not be located", path), path);
                    }

                    Uri newBinaryPath = new Uri(Path.GetFullPath(path));

                    string binaries = wixFiles.WxsDirectory.FullName;
                    if (binaries.EndsWith(sepCharString) == false) {
                        binaries = binaries + sepCharString;
                    }

                    Uri binariesPath = new Uri(binaries);
                
                    string relativeValue = binariesPath.MakeRelative(newBinaryPath);
                    relativeValue = relativeValue.Replace("/", sepCharString);
                

                    FileInfo testRelativeValue = null;

                    if (Path.IsPathRooted(relativeValue)) {
                        testRelativeValue = new FileInfo(Path.Combine(binaries, relativeValue));
                    } else {
                        if (relativeValue.StartsWith("file:")) {
                            relativeValue.Remove(0, 5);
                        }

                        testRelativeValue = new FileInfo(relativeValue);
                    }
                
                    if (WixEditSettings.Instance.UseRelativeOrAbsolutePaths == PathHandling.ForceRelativePaths && Path.IsPathRooted(relativeValue) == true) {
                        throw new Exception(String.Format("{0} is invalid. {1} should be relative to {2}", relativeValue, path, binaries));
                    }

                    binaryElement.Attributes["src"].Value = relativeValue;
                }
            }
        }

        public override bool CanResetValue(object component) {
            return (GetValue(component).Equals("") == false);
        }
    }
}
