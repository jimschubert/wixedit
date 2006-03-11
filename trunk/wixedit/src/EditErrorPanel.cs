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
    /// Summary description for EditErrorPanel.
    /// </summary>
    public class EditErrorPanel : DisplayBasePanel {
        #region Controls
        private PropertyGrid propertyGrid;
        private ContextMenu propertyGridContextMenu;
        #endregion

        public EditErrorPanel(WixFiles wixFiles) : base(wixFiles) {
            InitializeComponent();
        }

        #region Initialize Controls
        private void InitializeComponent() {
            propertyGrid = new CustomPropertyGrid();
            propertyGridContextMenu = new ContextMenu();

            // 
            // propertyGrid
            //
            propertyGrid.Dock = DockStyle.Fill;
            propertyGrid.Font = new Font("Tahoma", 8.25F, FontStyle.Regular, GraphicsUnit.Point, ((System.Byte)(0)));
            propertyGrid.Location = new Point(140, 0);
            propertyGrid.Name = "propertyGrid";
            propertyGrid.Size = new Size(269, 266);
            propertyGrid.TabIndex = 1;
            propertyGrid.PropertySort = PropertySort.Alphabetical;
            propertyGrid.ToolbarVisible = false;
            propertyGrid.HelpVisible = false;
            propertyGrid.ContextMenu = propertyGridContextMenu;

            // 
            // propertyGridContextMenu
            //
            propertyGridContextMenu.Popup += new EventHandler(OnPropertyGridPopupContextMenu);

            Controls.Add(propertyGrid);

            LoadData();
        }
        #endregion

        protected void LoadData() {
            XmlNodeList errors = wixFiles.WxsDocument.SelectNodes("/wix:Wix/*/wix:UI/wix:Error", wixFiles.WxsNsmgr);

            ErrorElementAdapter errorAdapter = new ErrorElementAdapter(errors, wixFiles);
            propertyGrid.SelectedObject = errorAdapter;
        }

        public void OnPropertyGridPopupContextMenu(object sender, EventArgs e) {
            if (propertyGrid.SelectedObject == null) {
                return;
            }

            MenuItem menuItemSeparator = new IconMenuItem("-");

            // Define the MenuItem objects to display for the TextBox.
            MenuItem menuItem1 = new IconMenuItem("&New", new Bitmap(WixFiles.GetResourceStream("bmp.new.bmp")));
            MenuItem menuItem2 = new IconMenuItem("&Delete", new Bitmap(WixFiles.GetResourceStream("bmp.delete.bmp")));
            MenuItem menuItem3 = new IconMenuItem("&Rename");

            menuItem1.Click += new EventHandler(OnNewPropertyGridItem);
            menuItem2.Click += new EventHandler(OnDeletePropertyGridItem);
            menuItem3.Click += new EventHandler(OnRenamePropertyGridItem);
        
            // Clear all previously added MenuItems.
            propertyGridContextMenu.MenuItems.Clear();

            propertyGridContextMenu.MenuItems.Add(menuItem1);
            if (propertyGrid.SelectedGridItem.PropertyDescriptor is ErrorElementPropertyDescriptor) {
                propertyGridContextMenu.MenuItems.Add(menuItem2);
                propertyGridContextMenu.MenuItems.Add(menuItem3);
            }
        }

        public void OnNewPropertyGridItem(object sender, EventArgs e) {
            EnterIntegerForm frm = new EnterIntegerForm();
            frm.Text = "Enter Error Number";
            if (DialogResult.OK == frm.ShowDialog()) {
                wixFiles.UndoManager.BeginNewCommandRange();

                XmlNode ui = ElementLocator.GetUIElement(wixFiles);
                if (ui == null) {
                    MessageBox.Show("No location found to add UI element, need element like module or product!");

                    return;
                }

                XmlElement newProp = wixFiles.WxsDocument.CreateElement("Error", "http://schemas.microsoft.com/wix/2003/01/wi");

                XmlAttribute newAttr = wixFiles.WxsDocument.CreateAttribute("Id");
                newAttr.Value = frm.SelectedString;
                newProp.Attributes.Append(newAttr);

                InsertNewXmlNode(ui, newProp);

                XmlNodeList errors = wixFiles.WxsDocument.SelectNodes("/wix:Wix/*/wix:UI/wix:Error", wixFiles.WxsNsmgr);

                ErrorElementAdapter errorAdapter = new ErrorElementAdapter(errors, wixFiles);

                propertyGrid.SelectedObject = errorAdapter;
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
            ErrorElementPropertyDescriptor desc = propertyGrid.SelectedGridItem.PropertyDescriptor as ErrorElementPropertyDescriptor;
            XmlNode element = desc.XmlElement;

            // Temporarily store the XmlAttributeAdapter, while resetting the propertyGrid.
            ErrorElementAdapter errorAdapter = propertyGrid.SelectedObject as ErrorElementAdapter;
            propertyGrid.SelectedObject = null;

            // Remove the attribute
            element.ParentNode.RemoveChild(element);

            XmlNodeList properties = wixFiles.WxsDocument.SelectNodes("/wix:Wix/*/wix:UI/wix:Error", wixFiles.WxsNsmgr);
            errorAdapter = new ErrorElementAdapter(properties, wixFiles);
            propertyGrid.SelectedObject = errorAdapter;
            propertyGrid.Update();
        }

        public void OnRenamePropertyGridItem(object sender, EventArgs e) {
            // Get the XmlAttribute from the PropertyDescriptor
            ErrorElementPropertyDescriptor desc = propertyGrid.SelectedGridItem.PropertyDescriptor as ErrorElementPropertyDescriptor;
            XmlNode element = desc.XmlElement;

            EnterIntegerForm frm = new EnterIntegerForm(element.Attributes["Id"].Value);
            frm.Text = "Enter Error Number";
            if (DialogResult.OK == frm.ShowDialog()) {    
                wixFiles.UndoManager.BeginNewCommandRange();

                element.Attributes["Id"].Value = frm.SelectedString;
    
                // Temporarily store the XmlAttributeAdapter, while resetting the propertyGrid.
                ErrorElementAdapter errorAdapter = propertyGrid.SelectedObject as ErrorElementAdapter;
                propertyGrid.SelectedObject = null;

                propertyGrid.SelectedObject = errorAdapter;
                propertyGrid.Update();
            }
        }

        public override bool IsOwnerOfNode(XmlNode node) {
            XmlNode showable = GetShowableNode(node);
            foreach (XmlNode xmlNode in wixFiles.WxsDocument.SelectNodes("/wix:Wix/*/wix:UI/wix:Error", wixFiles.WxsNsmgr)) {
                if (showable == xmlNode) {
                    return true;
                }
            }

            return false;
        }

        public override void ShowNode(XmlNode node) {
            XmlNodeList properties = wixFiles.WxsDocument.SelectNodes("/wix:Wix/*/wix:UI/wix:Error", wixFiles.WxsNsmgr);
            ErrorElementAdapter errorAdapter = new ErrorElementAdapter(properties, wixFiles);
    
            propertyGrid.SelectedObject = errorAdapter;
            propertyGrid.Update();
        }

        public override void ReloadData() {
            propertyGrid.SelectedObject = null;

            LoadData();
        }
    }
}
