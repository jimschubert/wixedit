
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
    /// Summary description for EditDialogPanel.
    /// </summary>
    public class EditPropertiesPanel : Panel {
        #region Controls
        private PropertyGrid propertyGrid;
        private ContextMenu propertyGridContextMenu;
        #endregion

        private WixFiles wixFiles;

        public EditPropertiesPanel(WixFiles wixFiles) {
            this.wixFiles = wixFiles;

            InitializeComponent();
        }

        #region Initialize Controls
        private void InitializeComponent() {
            XmlNodeList properties = wixFiles.WxsDocument.SelectNodes("/wix:Wix/wix:Product/wix:Property", wixFiles.WxsNsmgr);

            this.propertyGrid = new PropertyGrid();
            this.propertyGridContextMenu = new ContextMenu();

            // 
            // propertyGrid
            //
            this.propertyGrid.Dock = DockStyle.Fill;
            this.propertyGrid.Font = new Font("Tahoma", 8.25F, FontStyle.Regular, GraphicsUnit.Point, ((System.Byte)(0)));
            this.propertyGrid.Location = new Point(140, 0);
            this.propertyGrid.Name = "propertyGrid";
            this.propertyGrid.Size = new Size(269, 266);
            this.propertyGrid.TabIndex = 1;
            this.propertyGrid.PropertySort = PropertySort.Alphabetical;
            this.propertyGrid.ToolbarVisible = false;
            this.propertyGrid.HelpVisible = false;
            this.propertyGrid.ContextMenu = this.propertyGridContextMenu;

            // 
            // propertyGridContextMenu
            //
            this.propertyGridContextMenu.Popup += new EventHandler(OnPropertyGridPopupContextMenu);

            PropertyElementAdapter propAdapter = new PropertyElementAdapter(properties, wixFiles);
            this.propertyGrid.SelectedObject = propAdapter;

            this.Controls.Add(this.propertyGrid);
        }
        #endregion

        public void OnPropertyGridPopupContextMenu(object sender, EventArgs e) {
            if (propertyGrid.SelectedObject == null) {
                return;
            }

            MenuItem menuItemSeparator = new IconMenuItem("-");

            // Define the MenuItem objects to display for the TextBox.
            MenuItem menuItem1 = new IconMenuItem("&New", new Bitmap(WixFiles.GetResourceStream("WixEdit.new.bmp")));
            MenuItem menuItem2 = new IconMenuItem("&Delete", new Bitmap(WixFiles.GetResourceStream("WixEdit.delete.bmp")));

            menuItem1.Click += new EventHandler(OnNewPropertyGridItem);
            menuItem2.Click += new EventHandler(OnDeletePropertyGridItem);
        
            // Clear all previously added MenuItems.
            propertyGridContextMenu.MenuItems.Clear();

            propertyGridContextMenu.MenuItems.Add(menuItem1);
            propertyGridContextMenu.MenuItems.Add(menuItem2);
        }

        public void OnNewPropertyGridItem(object sender, EventArgs e) {
            EnterStringForm frm = new EnterStringForm();
            if (DialogResult.OK == frm.ShowDialog()) {
                XmlElement newProp = wixFiles.WxsDocument.CreateElement("wix:Property", "http://schemas.microsoft.com/wix/2003/01/wi");

                XmlAttribute newAttr = wixFiles.WxsDocument.CreateAttribute("Id");
                newAttr.Value = frm.SelectedString;
                newProp.Attributes.Append(newAttr);

                XmlNode product = wixFiles.WxsDocument.SelectSingleNode("/wix:Wix/wix:Product", wixFiles.WxsNsmgr);
                

                XmlNodeList properties = wixFiles.WxsDocument.SelectNodes("/wix:Wix/wix:Product/wix:Property", wixFiles.WxsNsmgr);

                product.AppendChild(newProp);
                properties = wixFiles.WxsDocument.SelectNodes("/wix:Wix/wix:Product/wix:Property", wixFiles.WxsNsmgr);

                PropertyElementAdapter propAdapter = new PropertyElementAdapter(properties, wixFiles);

                this.propertyGrid.SelectedObject = propAdapter;
                propertyGrid.Update();

                foreach (GridItem it in propertyGrid.SelectedGridItem.Parent.GridItems) {
                    if (it.Label == frm.SelectedString) {
                        propertyGrid.SelectedGridItem = it;
                        break;
                    }
                }
            }
        }

        public void OnDeletePropertyGridItem(object sender, EventArgs e) {
            // Get the XmlAttribute from the PropertyDescriptor
            PropertyElementPropertyDescriptor desc = propertyGrid.SelectedGridItem.PropertyDescriptor as PropertyElementPropertyDescriptor;
            XmlNode element = desc.PropertyElement;

            // Temporarily store the XmlAttributeAdapter, while resetting the propertyGrid.
            PropertyElementAdapter propAdapter = propertyGrid.SelectedObject as PropertyElementAdapter;
            propertyGrid.SelectedObject = null;

            // Remove the attribute
//            propAdapter.PropertyNodes.Remove(element);
            element.ParentNode.RemoveChild(element);

            XmlNodeList properties = wixFiles.WxsDocument.SelectNodes("/wix:Wix/wix:Product/wix:Property", wixFiles.WxsNsmgr);
            propAdapter = new PropertyElementAdapter(properties, wixFiles);
            this.propertyGrid.SelectedObject = propAdapter;
            propertyGrid.Update();
        }
    }
}