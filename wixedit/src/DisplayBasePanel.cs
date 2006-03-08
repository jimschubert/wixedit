
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
using System.Xml;

namespace WixEdit {
    /// <summary>
    /// Summary description for DisplayBasePanel.
    /// </summary>
    public abstract class DisplayBasePanel : BasePanel{
        public DisplayBasePanel(WixFiles wixFiles) : base(wixFiles) {
            Reload += new ReloadHandler(ReloadData);

            CreateControl();
        }

        public abstract bool IsOwnerOfNode(XmlNode node);
        public abstract void ShowNode(XmlNode node);
        public abstract void ReloadData();

        protected virtual XmlNode GetShowableNode(XmlNode node) {
            XmlNode showableNode = node;
            while (showableNode.NodeType != XmlNodeType.Element) {
                if (showableNode.NodeType == XmlNodeType.Attribute) {
                    showableNode = ((XmlAttribute) showableNode).OwnerElement;
                } else {
                    showableNode = showableNode.ParentNode;
                }
            }

            return showableNode;
        }

        private delegate void ReloadHandler();
        private event ReloadHandler Reload;

        public void DoReload() {
            Reload();
        }
    }
}
