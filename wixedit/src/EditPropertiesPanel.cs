
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
    /// Summary description for EditPropertiesPanel.
    /// </summary>
    public class EditPropertiesPanel : DisplaySimpleBasePanel {
        // Constructing properties as innerHTML but should be Value in later versions of WiX.
        public EditPropertiesPanel(WixFiles wixFiles) : base(wixFiles, "/wix:Wix/*/wix:Property", "Property", "Id", GetValueAttributeName(wixFiles)) {
            LoadData();
        }


        protected static string GetValueAttributeName(WixFiles wixFiles) {
            if (wixFiles.XsdDocument.SelectSingleNode("/xs:schema/xs:element[@name='Property']/xs:complexType/xs:attribute[@name='Value']", wixFiles.XsdNsmgr) != null) {
                return "Value";
            } else  {
                return null;
            }
        }

        protected override object GetPropertyAdapter(){
            return new PropertyElementAdapter(CurrentList, WixFiles);
        }

        public override void OnPropertyGridPopupContextMenu(object sender, EventArgs e) {
            if (CurrentGrid.SelectedObject == null) {
                return;
            }

            base.OnPropertyGridPopupContextMenu(sender, e);

            if (CurrentGrid.SelectedGridItem.PropertyDescriptor != null) {
                XmlNode selectedElement = GetSelectedGridObject();
                XmlNodeList selectedSubElements = selectedElement.SelectNodes("*", WixFiles.WxsNsmgr);

                MenuItem menuItemSeparator1 = new IconMenuItem("-");
                CurrentGridContextMenu.MenuItems.Add(1, menuItemSeparator1);

                if (selectedSubElements.Count == 0) {
                    MenuItem subMenuItem = new IconMenuItem("Insert", new Bitmap(WixFiles.GetResourceStream("bmp.new.bmp")));
                    CurrentGridContextMenu.MenuItems.Add(1, subMenuItem);
    
                    XmlNode definition = WixFiles.GetXsdElementNode("Property");
                    XmlNodeList subElements = definition.SelectNodes("xs:complexType/xs:sequence/xs:element", WixFiles.XsdNsmgr);
                    foreach (XmlNode sub in subElements) {
                        string subName = sub.Attributes["ref"].Value;
    
                        MenuItem subSubMenuItem = new IconMenuItem(subName);
                        subSubMenuItem.Click += new EventHandler(OnNewSubPropertyGridItem);
    
                        subMenuItem.MenuItems.Add(subSubMenuItem);
                    }
                } else if (selectedSubElements.Count == 1) {
                    MenuItem subMenuItem = new IconMenuItem("Remove " + selectedSubElements[0].Name, new Bitmap(WixFiles.GetResourceStream("bmp.new.bmp")));
                    CurrentGridContextMenu.MenuItems.Add(1, subMenuItem);

                    subMenuItem.Click += new EventHandler(OnRemoveSubPropertyGridItem);
                } else {
                    MenuItem subMenuItem = new IconMenuItem("Multiple subitems in property are unsupported!", new Bitmap(WixFiles.GetResourceStream("bmp.new.bmp")));
                    subMenuItem.Enabled = false;
                    CurrentGridContextMenu.MenuItems.Add(1, subMenuItem);
                }

                MenuItem menuItemSeparator2 = new IconMenuItem("-");
                CurrentGridContextMenu.MenuItems.Add(1, menuItemSeparator2);
            }
        }

        public void OnRemoveSubPropertyGridItem(object sender, EventArgs e) {
            XmlNode selectedElement = GetSelectedGridObject();

            WixFiles.UndoManager.BeginNewCommandRange();
  
            selectedElement.RemoveChild(selectedElement.FirstChild);

            RefreshGrid();
        }

        public void OnNewSubPropertyGridItem(object sender, EventArgs e) {
            XmlElement selectedElement = (XmlElement) GetSelectedGridObject();

            WixFiles.UndoManager.BeginNewCommandRange();
            
            MenuItem menuItem = sender as MenuItem;
            string typeName = menuItem.Text;

            XmlElement newElement = selectedElement.OwnerDocument.CreateElement(typeName, WixFiles.GetNamespaceUri(typeName));
            selectedElement.AppendChild(newElement);

            // Remove the value attribute.
            if (selectedElement.HasAttribute("Value")) {
                selectedElement.RemoveAttribute("Value");
            }

            RefreshGrid();
        }
    }
}
