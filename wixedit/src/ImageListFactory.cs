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
using System.Drawing;
using System.Collections;
using System.Collections.Specialized;
using System.Xml;
using System.Windows.Forms;

 
namespace WixEdit {
    /// <summary>
    /// Editing of dialogs.
    /// </summary>
    public class ImageListFactory {
        protected static ImageList imageList;
        protected static StringCollection imageTypes;

        static ImageListFactory() {
            imageTypes = GetTypes();
            imageList = CreateImageList(imageTypes);
        }

        private static StringCollection GetTypes() {
            StringCollection types = new StringCollection();

            XmlNodeList xmlElements = WixFiles.GetXsdDocument().SelectNodes("/xs:schema/xs:element", WixFiles.GetXsdNsmgr());

            foreach (XmlNode xmlElement in xmlElements) {
                XmlAttribute nameAtt = xmlElement.Attributes["name"];
                if (nameAtt != null) {
                    if (nameAtt.Value != null && nameAtt.Value.Length > 0) {
                        if (types.Contains(nameAtt.Value) == false) {
                            types.Add(nameAtt.Value);
                        }
                    }
                }
            }

            return types;
        }

        private static ImageList CreateImageList(StringCollection types) {
            ImageList images = new ImageList(); 

            Bitmap unknownBmp = new Bitmap(WixFiles.GetResourceStream("WixEdit.empty.bmp"));
            unknownBmp.MakeTransparent();

            Bitmap typeBmp;
            foreach (string type in types) {
                try {
                    typeBmp = new Bitmap(WixFiles.GetResourceStream(String.Format("WixEdit.{0}.bmp", type.ToLower())));
                    typeBmp.MakeTransparent();
                } catch {
                    typeBmp = unknownBmp;
                }

                images.Images.Add(typeBmp);
            }

            return images;
        }

        public static ImageList GetImageList() {
            return imageList;
        }

        public static int GetImageIndex(string imageName) {
            return imageTypes.IndexOf(imageName);
        }
    }
}