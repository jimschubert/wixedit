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
using System.Collections.Specialized;
using System.Xml;

namespace WixEdit {
    /// <summary>
    /// Panel to edit global data.
    /// </summary>
    public class EditGlobalDataPanel : DetailsBasePanel {
        public EditGlobalDataPanel(WixFiles wixFiles) : base(wixFiles) {
        }

        private StringCollection skipElements;
        protected override StringCollection SkipElements {
            get {
                if (skipElements == null) {
                    skipElements = new StringCollection();
                    skipElements.Add("Property");
                    skipElements.Add("UI");
                    skipElements.Add("Icon");
                    skipElements.Add("Binary");
                    skipElements.Add("Feature");
                    skipElements.Add("Directory");
                    skipElements.Add("DirectoryRef");
                    skipElements.Add("InstallExecuteSequence");
                    skipElements.Add("InstallUISequence");
                    skipElements.Add("AdminExecuteSequence");
                    skipElements.Add("AdminUISequence");
                    skipElements.Add("AdvertiseExecuteSequence");
                    skipElements.Add("CustomAction");
                    skipElements.Add("Include");
                }

                return skipElements;
            }
        }

        protected override ArrayList GetXmlNodes() {
            ArrayList nodes = new ArrayList();
            XmlNodeList xmlNodes = wixFiles.WxsDocument.SelectNodes("/wix:Wix/*", wixFiles.WxsNsmgr);
            foreach (XmlNode xmlNode in xmlNodes) {
                nodes.Add(xmlNode);
            }

            return nodes;
        }

        protected override void InsertNewXmlNode(XmlNode parentElement, XmlNode newElement) {
            if (newElement.Name == "Package") {
                if (parentElement.FirstChild != null) {
                    parentElement.InsertBefore(newElement, parentElement.FirstChild);
                } else {
                    parentElement.AppendChild(newElement);
                }
            } else {
                base.InsertNewXmlNode (parentElement, newElement);
            }
        }
    }
}