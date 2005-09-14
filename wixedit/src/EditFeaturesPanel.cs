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
using System.Drawing;
using System.Xml;
using System.Windows.Forms;

namespace WixEdit {
    /// <summary>
    /// Panel to edit features.
    /// </summary>
    public class EditFeaturesPanel : DetailsBasePanel {
        protected ContextMenu globalTreeViewContextMenu;

        public EditFeaturesPanel(WixFiles wixFiles) : base(wixFiles) {
            globalTreeViewContextMenu = new ContextMenu();
            globalTreeViewContextMenu.Popup += new EventHandler(PopupGlobalTreeViewContextMenu);
        }

        protected override ArrayList GetXmlNodes() {
             ArrayList nodes = new ArrayList();
            XmlNodeList xmlNodes = wixFiles.WxsDocument.SelectNodes("/wix:Wix/*/wix:Feature", wixFiles.WxsNsmgr);
            foreach (XmlNode xmlNode in xmlNodes) {
                nodes.Add(xmlNode);
            }

            return nodes;
        }
                
        protected override void OnGlobalTreeViewContextMenu(object sender, System.Windows.Forms.MouseEventArgs e) {
            Point spot = PointToClient(treeView.PointToScreen(new Point(e.X,e.Y)));

            globalTreeViewContextMenu.Show(this, spot);
        }


        protected void PopupGlobalTreeViewContextMenu(System.Object sender, System.EventArgs e) {
            globalTreeViewContextMenu.MenuItems.Clear();

            IconMenuItem subMenuItem = new IconMenuItem("New Feature", new Bitmap(WixFiles.GetResourceStream("bmp.new.bmp")));

            subMenuItem.Click += new EventHandler(NewCustomElement_Click);

            globalTreeViewContextMenu.MenuItems.Add(subMenuItem);
        }

        private void NewCustomElement_Click(object sender, System.EventArgs e) {
            string elementName = "Feature";

            XmlNode xmlNode = wixFiles.WxsDocument.SelectSingleNode("/wix:Wix/*", wixFiles.WxsNsmgr);

            XmlElement newElement = wixFiles.WxsDocument.CreateElement(elementName, "http://schemas.microsoft.com/wix/2003/01/wi");
            TreeNode action = new TreeNode(elementName);
            action.Tag = newElement;

            int imageIndex = ImageListFactory.GetImageIndex(elementName);
            if (imageIndex >= 0) {
                action.ImageIndex = imageIndex;
                action.SelectedImageIndex = imageIndex;
            }

            XmlNodeList sameNodes = xmlNode.SelectNodes("wix:" + elementName, wixFiles.WxsNsmgr);
            if (sameNodes.Count > 0) {
                xmlNode.InsertAfter(newElement, sameNodes[sameNodes.Count - 1]);
            } else {
                xmlNode.AppendChild(newElement);
            }

            treeView.Nodes.Add(action);
            treeView.SelectedNode = action;

            ShowProperties(newElement); 
        }
    }
}