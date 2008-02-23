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

            ArrayList xmlElements = WixFiles.GetXsdAllElementNames();
            foreach (XmlNode xmlElement in xmlElements) {
                XmlAttribute nameAtt = xmlElement.Attributes["name"];
                if (nameAtt != null) {
                    if (nameAtt.Value != null && nameAtt.Value.Length > 0) {
                        XmlNode deprecated = xmlElement.SelectSingleNode("xs:annotation/xs:appinfo/xse:deprecated", WixFiles.GetXsdNsmgr());
                        if (deprecated != null) {
                            if (types.Contains("deprecated_"+nameAtt.Value) == false) {
                                types.Add("deprecated_"+nameAtt.Value);
                            }
                        } else if (types.Contains(nameAtt.Value) == false) {
                            types.Add(nameAtt.Value);
                        }
                    }
                }
            }

            return types;
        }

        private static ImageList CreateImageList(StringCollection types) {
            ImageList images = new ImageList();

            Bitmap unknownBmp = new Bitmap(WixFiles.GetResourceStream("elements.unknown.bmp"));
            unknownBmp.MakeTransparent();

            Bitmap typeBmp;
            foreach (string type in types) {
                try {
                    typeBmp = null;

                    if (type.StartsWith("deprecated")) {
                        if (WixFiles.HasResource(String.Format("elements.{0}.bmp", type.Remove(0, 11).ToLower()))) {
                            typeBmp = new Bitmap(WixFiles.GetResourceStream(String.Format("elements.{0}.bmp", type.Remove(0, 11).ToLower())));
                            Bitmap tmpBmp = OverlayWarning(typeBmp);
                            typeBmp.Dispose();
                            typeBmp = tmpBmp;
                        }
                    } else {
                        if (WixFiles.HasResource(String.Format("elements.{0}.bmp", type.ToLower()))) {
                            typeBmp = new Bitmap(WixFiles.GetResourceStream(String.Format("elements.{0}.bmp", type.ToLower())));
                        }
                    }
                    if (typeBmp != null) {
                        typeBmp.MakeTransparent();
                    } else {
                        typeBmp = unknownBmp;
                    }
                } catch {
                    typeBmp = unknownBmp;
                }

                images.Images.Add(typeBmp);
            }

            Bitmap unsupportedBmp = new Bitmap(WixFiles.GetResourceStream("elements.unsupported.bmp"));
            unsupportedBmp.MakeTransparent();

            images.Images.Add(unsupportedBmp);

            return images;
        }

        private static Bitmap OverlayWarning(Bitmap typeBmp) {
            Bitmap bmp = new Bitmap(typeBmp.Width, typeBmp.Height);

            Graphics g = Graphics.FromImage(bmp);

            g.DrawImageUnscaled(typeBmp, 0, 0);

            g.FillPolygon(Brushes.Yellow, new Point[] { new Point(5, 15), new Point(15, 15), new Point(10, 5) });

            g.DrawLine(Pens.Black, 5, 15, 15, 15);
            g.DrawLine(Pens.Black, 5, 14, 10, 5);
            g.DrawLine(Pens.Black, 10, 5, 15, 14);

            g.DrawLine(Pens.Black, 10, 13, 10, 14);
            g.DrawLine(Pens.Yellow, 9, 14, 10, 14);
            g.DrawLine(Pens.Black, 10, 8, 10, 11);

            return bmp;

            //typeBmp.SetPixel(5, 14, Color.Black);
            //typeBmp.SetPixel(15, 14, Color.Black);
            //for (int i = 6; i <= 14; i++) {
            //    typeBmp.SetPixel(i, 14, Color.Yellow);
            //}

            //typeBmp.SetPixel(6, 13, Color.Black);
            //typeBmp.SetPixel(14, 13, Color.Black);
            //for (int i = 7; i <= 13; i++) {
            //    typeBmp.SetPixel(i, 13, Color.Yellow);
            //}
            //typeBmp.SetPixel(10, 13, Color.Black);
            
            //typeBmp.SetPixel(6, 12, Color.Black);
            //typeBmp.SetPixel(14, 12, Color.Black);
            //for (int i = 7; i <= 13; i++) {
            //    typeBmp.SetPixel(i, 12, Color.Yellow);
            //}

            
            //typeBmp.SetPixel(7, 11, Color.Black);
            //typeBmp.SetPixel(13, 11, Color.Black);
            //for (int i = 8; i <= 12; i++) {
            //    typeBmp.SetPixel(i, 11, Color.Yellow);
            //}
            //typeBmp.SetPixel(10, 11, Color.Black);

            //typeBmp.SetPixel(7, 10, Color.Black);
            //typeBmp.SetPixel(13, 10, Color.Black);
            //for (int i = 8; i <= 12; i++) {
            //    typeBmp.SetPixel(i, 10, Color.Yellow);
            //}
            //typeBmp.SetPixel(10, 10, Color.Black);

            //typeBmp.SetPixel(8, 9, Color.Black);
            //typeBmp.SetPixel(12, 9, Color.Black);
            //for (int i = 9; i <= 11; i++) {
            //    typeBmp.SetPixel(i, 9, Color.Yellow);
            //}
            //typeBmp.SetPixel(10, 9, Color.Black);
            
            //typeBmp.SetPixel(8, 8, Color.Black);
            //typeBmp.SetPixel(12, 8, Color.Black);
            //for (int i = 9; i <= 11; i++) {
            //    typeBmp.SetPixel(i, 8, Color.Yellow);
            //}

            //typeBmp.SetPixel(9, 7, Color.Black);
            //typeBmp.SetPixel(10, 7, Color.Yellow);
            //typeBmp.SetPixel(11, 7, Color.Black);

            //typeBmp.SetPixel(9, 6, Color.Black);
            //typeBmp.SetPixel(10, 6, Color.Yellow);
            //typeBmp.SetPixel(11, 6, Color.Black);

            //typeBmp.SetPixel(10, 5, Color.Black);

        }

        public static ImageList GetImageList()
        {
            return imageList;
        }

        public static int GetImageIndex(string imageName) {
            int ret = imageTypes.IndexOf(imageName);
            if (ret < 0) {
                ret = imageTypes.IndexOf("deprecated_"+imageName);
                if (ret < 0) {
                    int colonPos = imageName.IndexOf(":");
                    if (colonPos > 0) {
                        ret = imageTypes.IndexOf(imageName.Substring(colonPos+1));
                    }
                    if (ret < 0) {
                        ret = imageTypes.IndexOf("deprecated_"+imageName.Substring(colonPos + 1));
                    }
                }
                if (ret < 0) {
                    ret = GetUnsupportedImageIndex();
                }
            }

            return ret;
        }

        public static int GetUnsupportedImageIndex() {
            return imageTypes.Count;
        }
    }
}