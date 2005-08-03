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
using System.Xml;

namespace WixEdit {
    /// <summary>
    /// Panel for adding and removing files and other installable items.
    /// </summary>
    public class EditFilesPanel : DetailsBasePanel {
        public EditFilesPanel(WixFiles wixFiles) : base(wixFiles) {
        }

        protected override ArrayList GetXmlNodes() {
            ArrayList nodes = new ArrayList();
            XmlNodeList xmlNodes = wixFiles.WxsDocument.SelectNodes("/wix:Wix/*/wix:Directory", wixFiles.WxsNsmgr);
            foreach (XmlNode xmlNode in xmlNodes) {
                nodes.Add(xmlNode);
            }

            return nodes;
        }
    }
}