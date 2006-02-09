
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
    public class EditResourcesPanel : DisplayBasePanel {
        #region Controls
        private PropertyGrid binaryGrid;
        private ContextMenu binaryGridContextMenu;
        #endregion

        public EditResourcesPanel(WixFiles wixFiles) : base(wixFiles) {
            InitializeComponent();
        }

        #region Initialize Controls
        private void InitializeComponent() {
            binaryGrid = new CustomPropertyGrid();
            binaryGridContextMenu = new ContextMenu();

            // 
            // binaryGrid
            //
            binaryGrid.Dock = DockStyle.Fill;
            binaryGrid.Font = new Font("Tahoma", 8.25F, FontStyle.Regular, GraphicsUnit.Point, ((System.Byte)(0)));
            binaryGrid.Location = new Point(140, 0);
            binaryGrid.Name = "binaryGrid";
            binaryGrid.Size = new Size(269, 266);
            binaryGrid.TabIndex = 1;
            binaryGrid.PropertySort = PropertySort.Alphabetical;
            binaryGrid.ToolbarVisible = false;
            binaryGrid.HelpVisible = false;
            binaryGrid.ContextMenu = binaryGridContextMenu;

            // 
            // binaryGridContextMenu
            //
            binaryGridContextMenu.Popup += new EventHandler(OnPropertyGridPopupContextMenu);

            Controls.Add(binaryGrid);

            LoadData();
        }
        #endregion

        protected void LoadData() {
            XmlNodeList binaries = wixFiles.WxsDocument.SelectNodes("/wix:Wix/*/wix:Binary", wixFiles.WxsNsmgr);

            BinaryElementAdapter binAdapter = new BinaryElementAdapter(binaries, wixFiles);
            binaryGrid.SelectedObject = binAdapter;
        }

        public void OnPropertyGridPopupContextMenu(object sender, EventArgs e) {
            if (binaryGrid.SelectedObject == null) {
                return;
            }

            MenuItem menuItemSeparator = new IconMenuItem("-");

            // Define the MenuItem objects to display for the TextBox.
            MenuItem menuItem1 = new IconMenuItem("&New", new Bitmap(WixFiles.GetResourceStream("bmp.new.bmp")));
            MenuItem menuItem2 = new IconMenuItem("&Delete", new Bitmap(WixFiles.GetResourceStream("bmp.delete.bmp")));

            menuItem1.Click += new EventHandler(OnNewPropertyGridItem);
            menuItem2.Click += new EventHandler(OnDeletePropertyGridItem);
        
            // Clear all previously added MenuItems.
            binaryGridContextMenu.MenuItems.Clear();

            binaryGridContextMenu.MenuItems.Add(menuItem1);
            binaryGridContextMenu.MenuItems.Add(menuItem2);
        }

        public void OnNewPropertyGridItem(object sender, EventArgs e) {
            EnterStringForm frm = new EnterStringForm();
            frm.Text = "Enter Resource Name";
            if (DialogResult.OK == frm.ShowDialog()) {
                wixFiles.UndoManager.BeginNewCommandRange();

                XmlElement newProp = wixFiles.WxsDocument.CreateElement("Binary", "http://schemas.microsoft.com/wix/2003/01/wi");

                XmlAttribute newAttr = wixFiles.WxsDocument.CreateAttribute("Id");
                newAttr.Value = frm.SelectedString;
                newProp.Attributes.Append(newAttr);

                newAttr = wixFiles.WxsDocument.CreateAttribute("src");
                newAttr.Value = "";
                newProp.Attributes.Append(newAttr);

                XmlNode product = wixFiles.WxsDocument.SelectSingleNode("/wix:Wix/*", wixFiles.WxsNsmgr);
                

                XmlNodeList binaries = wixFiles.WxsDocument.SelectNodes("/wix:Wix/*/wix:Binary", wixFiles.WxsNsmgr);

                XmlNodeList sameNodes = product.SelectNodes("wix:Binary", wixFiles.WxsNsmgr);
                if (sameNodes.Count > 0) {
                    product.InsertAfter(newProp, sameNodes[sameNodes.Count - 1]);
                } else {
                    product.AppendChild(newProp);
                }

                binaries = wixFiles.WxsDocument.SelectNodes("/wix:Wix/*/wix:Binary", wixFiles.WxsNsmgr);

                BinaryElementAdapter binAdapter = new BinaryElementAdapter(binaries, wixFiles);

                binaryGrid.SelectedObject = binAdapter;
                binaryGrid.Update();

                foreach (GridItem it in binaryGrid.SelectedGridItem.Parent.GridItems) {
                    if (it.Label == frm.SelectedString) {
                        binaryGrid.SelectedGridItem = it;
                        break;
                    }
                }
            }
        }

        public void OnDeletePropertyGridItem(object sender, EventArgs e) {
            // Get the XmlAttribute from the PropertyDescriptor
            BinaryElementPropertyDescriptor desc = binaryGrid.SelectedGridItem.PropertyDescriptor as BinaryElementPropertyDescriptor;
            XmlNode element = desc.XmlElement;

            // Temporarily store the XmlAttributeAdapter, while resetting the binaryGrid.
            BinaryElementAdapter binAdapter = binaryGrid.SelectedObject as BinaryElementAdapter;
            binaryGrid.SelectedObject = null;

            // Remove the attribute
//            binAdapter.PropertyNodes.Remove(element);
            element.ParentNode.RemoveChild(element);

            XmlNodeList binaries = wixFiles.WxsDocument.SelectNodes("/wix:Wix/*/wix:Binary", wixFiles.WxsNsmgr);
            binAdapter = new BinaryElementAdapter(binaries, wixFiles);
            binaryGrid.SelectedObject = binAdapter;
            binaryGrid.Update();
        }


        public override bool IsOwnerOfNode(XmlNode node) {
            XmlNode showable = GetShowableNode(node);

            foreach (XmlNode xmlNode in wixFiles.WxsDocument.SelectNodes("/wix:Wix/*/wix:Binary", wixFiles.WxsNsmgr)) {
                if (showable == xmlNode) {
                    return true;
                }
            }

            return false;
        }

        public override void ShowNode(XmlNode node) {
            XmlNodeList binaries = wixFiles.WxsDocument.SelectNodes("/wix:Wix/*/wix:Binary", wixFiles.WxsNsmgr);
            BinaryElementAdapter binAdapter = new BinaryElementAdapter(binaries, wixFiles);
            binaryGrid.SelectedObject = binAdapter;
        }

        public override void ReloadData() {
            binaryGrid.SelectedObject = null;

            LoadData();
        }


        private XmlNode GetShowableNode(XmlNode node) {
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


		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose(bool disposing) {
            if( disposing ) {
                binaryGrid.Dispose();
                binaryGrid = null;
                binaryGridContextMenu.Dispose();
                binaryGridContextMenu = null;
            }
            base.Dispose( disposing );
		}
    }
}
