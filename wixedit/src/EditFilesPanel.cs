
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
using System.Drawing.Drawing2D;
using System.Collections;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Data;
using System.Xml;
using System.IO;
using System.Resources;
using System.Reflection;

using WixEdit.PropertyGridExtensions;

namespace WixEdit {
    /// <summary>
    /// Summary description for EditResourcesPanel.
    /// </summary>
    public class EditFilesPanel : Panel {
        private WixFiles wixFiles;
        private TreeView directoryTreeView;
        private ContextMenu directoryTreeViewContextMenu;

        public EditFilesPanel(WixFiles wixFiles) {
            this.wixFiles = wixFiles;

            InitializeComponent();
        }

        #region Initialize Controls
        private void InitializeComponent() {
            this.directoryTreeView = new TreeView();


            // 
            // directoryTreeView
            // 
            this.directoryTreeView.Dock = DockStyle.Left;
            this.directoryTreeView.ImageIndex = -1;
            this.directoryTreeView.Location = new Point(0, 0);
            this.directoryTreeView.Name = "directoryTreeView";
            this.directoryTreeView.SelectedImageIndex = -1;
            this.directoryTreeView.Size = new Size(170, 266);
            this.directoryTreeView.TabIndex = 6;
            this.directoryTreeView.AfterSelect += new TreeViewEventHandler(this.OnAfterSelect);

            this.directoryTreeViewContextMenu = new ContextMenu();
            this.directoryTreeViewContextMenu.Popup += new EventHandler(PopupDirectoryTreeViewContextMenu);

            this.directoryTreeView.MouseDown += new MouseEventHandler(DirectoryViewMouseDown);

//            this.directoryTreeView.ImageList = GetDirectoryTreeViewImageList();

            this.Controls.Add(this.directoryTreeView);

            XmlNodeList files = wixFiles.WxsDocument.SelectNodes("/wix:Wix/wix:Product/wix:Directory", wixFiles.WxsNsmgr);
            foreach (XmlNode file in files) {
                AddTreeNodesRecursive(file, directoryTreeView.Nodes);
            }

        }
        #endregion

        private void AddTreeNodesRecursive(XmlNode file, TreeNodeCollection nodes) {
            TreeNode node = new TreeNode(file.Name);
            nodes.Add(node);

            foreach (XmlNode child in file.ChildNodes) {
                AddTreeNodesRecursive(child, node.Nodes);
            }
            
        }

        private void DirectoryViewMouseDown(object sender, System.Windows.Forms.MouseEventArgs e) {
            if (e.Button == MouseButtons.Right) {
/*
                TreeNode node = dialogTreeView.GetNodeAt(e.X, e.Y);
                if (node == null) {
                    return;
                }
                dialogTreeView.SelectedNode = node;

                Point spot = this.PointToClient(dialogTreeView.PointToScreen(new Point(e.X,e.Y)));
                dialogTreeViewContextMenu.Show(this, spot);
*/
            }
        }

        private void OnAfterSelect(object sender, TreeViewEventArgs e) {
/*
            XmlNode node = e.Node.Tag as XmlNode;
            if (node != null) {
                ShowWixProperties(node);
            }
*/
        }

        protected void PopupDirectoryTreeViewContextMenu(System.Object sender, System.EventArgs e) {
/*
            XmlNode node = dialogTreeView.SelectedNode.Tag as XmlNode;
            if (node == null) {
                return;
            }

            dialogTreeViewContextMenu.MenuItems.Clear();

            switch (node.Name) {
                case "Dialog":
                    dialogTreeViewContextMenu.MenuItems.Add(this.newControlElementMenu);
                    break;
                case "Control":
                    this.newControlSubElementsMenu.MenuItems.Clear();
                    dialogTreeViewContextMenu.MenuItems.Add(this.newControlSubElementsMenu);
                    this.newControlSubElementsMenu.MenuItems.Add(this.newTextElementMenu);
                    this.newControlSubElementsMenu.MenuItems.Add(this.newPublishElementMenu);
                    this.newControlSubElementsMenu.MenuItems.Add(this.newConditionElementMenu);
                    this.newControlSubElementsMenu.MenuItems.Add(this.newSubscribeElementMenu);
                    break;
                default:
                    break;
            }

            dialogTreeViewContextMenu.MenuItems.Add(this.deleteCurrentElementMenu);


            XmlAttributeAdapter attAdapter = propertyGrid.SelectedObject as XmlAttributeAdapter;

            XmlNode documentation = attAdapter.XmlNodeDefinition.SelectSingleNode("xs:annotation/xs:documentation", wixFiles.XsdNsmgr);
            if(documentation == null) {
                documentation = attAdapter.XmlNodeDefinition.SelectSingleNode("xs:simpleContent/xs:extension/xs:annotation/xs:documentation", wixFiles.XsdNsmgr);
            }

            if (documentation != null) {
                dialogTreeViewContextMenu.MenuItems.Add(this.infoAboutCurrentElementMenu);
            }
*/
        }


        private ImageList GetDirectoryTreeViewImageList() {
            ImageList images = new ImageList(); 

            Bitmap bmp = new Bitmap(WixFiles.GetResourceStream("WixEdit.empty.bmp"));
            bmp.MakeTransparent();
            images.Images.Add(bmp);

            bmp = new Bitmap(WixFiles.GetResourceStream("WixEdit.dialog.bmp"));
            bmp.MakeTransparent();
            images.Images.Add(bmp);

            bmp = new Bitmap(WixFiles.GetResourceStream("WixEdit.control.bmp"));
            bmp.MakeTransparent();
            images.Images.Add(bmp);

            bmp = new Bitmap(WixFiles.GetResourceStream("WixEdit.text.bmp"));
            bmp.MakeTransparent();
            images.Images.Add(bmp);

            bmp = new Bitmap(WixFiles.GetResourceStream("WixEdit.condition.bmp"));
            bmp.MakeTransparent();
            images.Images.Add(bmp);

            bmp = new Bitmap(WixFiles.GetResourceStream("WixEdit.subscribe.bmp"));
            bmp.MakeTransparent();
            images.Images.Add(bmp);

            bmp = new Bitmap(WixFiles.GetResourceStream("WixEdit.publish.bmp"));
            bmp.MakeTransparent();
            images.Images.Add(bmp);

            return images;
        }
    }
}
