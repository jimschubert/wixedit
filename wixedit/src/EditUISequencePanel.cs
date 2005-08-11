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
    /// Panel to edit UISequence data.
    /// </summary>
    public class EditUISequencePanel : DetailsBasePanel {
        protected ContextMenu globalTreeViewContextMenu;

        public EditUISequencePanel(WixFiles wixFiles) : base(wixFiles) {
            globalTreeViewContextMenu = new ContextMenu();
            globalTreeViewContextMenu.Popup += new EventHandler(PopupGlobalTreeViewContextMenu);
        }

        protected override ArrayList GetXmlNodes() {
            ArrayList nodes = new ArrayList();
            XmlNodeList xmlNodes = wixFiles.WxsDocument.SelectNodes("/wix:Wix//wix:InstallUISequence", wixFiles.WxsNsmgr);
            foreach (XmlNode xmlNode in xmlNodes) {
                nodes.Add(xmlNode);
            }

            xmlNodes = wixFiles.WxsDocument.SelectNodes("/wix:Wix//wix:AdminUISequence", wixFiles.WxsNsmgr);
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

            IconMenuItem subMenuItem = new IconMenuItem("New", new Bitmap(WixFiles.GetResourceStream("bmp.new.bmp")));

            IconMenuItem subSubMenuItem1 = new IconMenuItem("InstallUISequence");
            IconMenuItem subSubMenuItem2 = new IconMenuItem("AdminUISequence");

            subSubMenuItem1.Click += new EventHandler(NewCustomElement_Click);
            subSubMenuItem2.Click += new EventHandler(NewCustomElement_Click);

            subMenuItem.MenuItems.Add(subSubMenuItem1);
            subMenuItem.MenuItems.Add(subSubMenuItem2);

            globalTreeViewContextMenu.MenuItems.Add(subMenuItem);
        }

        private void NewCustomElement_Click(object sender, System.EventArgs e) {
            MenuItem item = (MenuItem) sender;

            XmlNode xmlNode = wixFiles.WxsDocument.SelectSingleNode("/wix:Wix/*", wixFiles.WxsNsmgr);

            XmlElement newElement = wixFiles.WxsDocument.CreateElement(item.Text, "http://schemas.microsoft.com/wix/2003/01/wi");
            TreeNode action = new TreeNode(item.Text);
            action.Tag = newElement;

            int imageIndex = ImageListFactory.GetImageIndex(item.Text);
            if (imageIndex >= 0) {
                action.ImageIndex = imageIndex;
                action.SelectedImageIndex = imageIndex;
            }

            xmlNode.AppendChild(newElement);

            treeView.Nodes.Add(action);
            treeView.SelectedNode = action;

            ShowProperties(newElement); 
        }
    }
}