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
using System.ComponentModel;
using System.Globalization;
using System.Drawing.Design;
using System.IO;
using System.Windows.Forms;

namespace WixEdit.Settings {
    [DescriptionAttribute("The directory with Wix binaries.")]
    public class BinDirectoryStructure {
        private WixEditSettings.WixEditData wixEditData;
        public BinDirectoryStructure(WixEditSettings.WixEditData data) {
            wixEditData = data;
        }

        [
        DefaultValueAttribute(true),
        Editor(typeof(FilteredFileNameEditor), typeof(System.Drawing.Design.UITypeEditor)),
        FilteredFileNameEditor.Filter("dark.exe |dark.exe")
        ]
        public string Dark {
            get {
                if (wixEditData.DarkLocation == null) {
                    if (wixEditData.BinDirectory == null) {
                        return null;
                    }
                    return Path.Combine(wixEditData.BinDirectory, "dark.exe");
                } else {
                    return wixEditData.DarkLocation;
                }
            }
            set { wixEditData.DarkLocation = value; }
        }

        [
        DefaultValueAttribute(true),
        Editor(typeof(FilteredFileNameEditor), typeof(System.Drawing.Design.UITypeEditor)),
        FilteredFileNameEditor.Filter("candle.exe |candle.exe")
        ]
        public string Candle {
            get {
                if (wixEditData.CandleLocation == null) {
                    if (wixEditData.BinDirectory == null) {
                        return null;
                    }
                    return Path.Combine(wixEditData.BinDirectory, "candle.exe");
                } else {
                    return wixEditData.CandleLocation;
                }
            }
            set { wixEditData.CandleLocation = value; }
        }

        [
        DefaultValueAttribute(true),
        Editor(typeof(FilteredFileNameEditor), typeof(System.Drawing.Design.UITypeEditor)),
        FilteredFileNameEditor.Filter("wix.xsd |wix.xsd")
        ]
        public string Xsd {
            get {
                if (wixEditData.XsdLocation == null) {
                    if (wixEditData.BinDirectory == null) {
                        return null;
                    }
                    return Path.Combine(wixEditData.BinDirectory, "doc\\wix.xsd");
                } else {
                    return wixEditData.XsdLocation;
                }
            }
            set { wixEditData.XsdLocation = value; }
        }

        public bool HasSameBinDirectory() {
            if (wixEditData.CandleLocation == null && wixEditData.DarkLocation == null && wixEditData.XsdLocation == null) {
                return true;
            }

            if (Candle == null || Dark == null || Xsd == null) {
                return false;
            }

            return (new FileInfo(Candle).Directory.FullName == new FileInfo(Dark).Directory.FullName && 
                new FileInfo(Xsd).Directory.FullName.StartsWith(new FileInfo(Candle).Directory.FullName));
        }

        /// <summary>
        /// Not showing in property grid.
        /// </summary>
        [Browsable(false)]
        public string BinDirectory {
            get {
                return wixEditData.BinDirectory;
            }
            set {
                wixEditData.BinDirectory = value;
            }
        }

        public class BinDirectoryExpandableObjectConverter : ExpandableObjectConverter {
            public override bool CanConvertTo(ITypeDescriptorContext context,
                System.Type destinationType) {
                if (destinationType == typeof(BinDirectoryStructure))
                    return true;
                
                return base.CanConvertTo(context, destinationType);
            }
            
            public override object ConvertTo(ITypeDescriptorContext context,
                CultureInfo culture, 
                object value, 
                System.Type destinationType) {
                if (destinationType == typeof(System.String) && 
                    value is BinDirectoryStructure){             
                    BinDirectoryStructure bd = (BinDirectoryStructure)value;
                
                    FileInfo candleInfo = new FileInfo(bd.Candle);
                    FileInfo darkInfo = new FileInfo(bd.Dark);
                    FileInfo xsdInfo = new FileInfo(bd.Xsd);
            
                    if (bd.HasSameBinDirectory()) {
                        return bd.BinDirectory;
                    } else {
                        return "...";
                    }
                }
                return base.ConvertTo(context, culture, value, destinationType);
            }
            
            public override bool CanConvertFrom(ITypeDescriptorContext context,
                System.Type sourceType) {
                if (sourceType == typeof(string))
                    return true;
                
                return false;
            }

            public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value) {
                WixEditSettings.WixEditData data = WixEditSettings.Instance.GetInternalDataStructure();
                data.BinDirectory = value as string;
                data.CandleLocation = null;
                data.DarkLocation = null;
                data.XsdLocation = null;

                return new BinDirectoryStructure(data);
            }
        }
    }
}