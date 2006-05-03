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
    public class EditErrorPanel : DisplaySimpleBasePanel {
        public EditErrorPanel(WixFiles wixFiles) : base(wixFiles, "/wix:Wix/*/wix:UI/wix:Error", "Error", "Id") {
            LoadData();
        }

        protected override void AssignParentNode() {
            CurrentParent = ElementLocator.GetUIElement(WixFiles);
        }

        protected override XmlNode GetSelectedPropertyDescriptor(){
            ErrorElementPropertyDescriptor desc = CurrentGrid.SelectedGridItem.PropertyDescriptor as ErrorElementPropertyDescriptor;
            return desc.XmlElement;
        }

        protected override object GetPropertyAdapter(){
            return new ErrorElementAdapter(CurrentList, WixFiles);
        }

        public override void OnNewPropertyGridItem(object sender, EventArgs e) {
            EnterIntegerForm frm = new EnterIntegerForm();
            frm.Text = "Enter Error Number";

            if (DialogResult.OK == frm.ShowDialog()) {
                if (CurrentParent == null) {
                    MessageBox.Show("No location found to add UI element, need element like module or product!");
                    return;
                }

                WixFiles.UndoManager.BeginNewCommandRange();

                XmlElement newProp = WixFiles.WxsDocument.CreateElement(CurrentElementName, WixFiles.WixNamespaceUri);
                XmlAttribute newAttr = WixFiles.WxsDocument.CreateAttribute(CurrentKeyName);

                newAttr.Value = frm.SelectedString;

                newProp.Attributes.Append(newAttr);

                InsertNewXmlNode(CurrentParent, newProp);

                RefreshGrid(frm.SelectedString);
            }
        }

        public override void OnRenamePropertyGridItem(object sender, EventArgs e) {
            XmlNode element = GetSelectedPropertyDescriptor();
            if (element != null){
                EnterIntegerForm frm = new EnterIntegerForm(element.Attributes[CurrentKeyName].Value);
                frm.Text = "Enter Error Number";

                if (DialogResult.OK == frm.ShowDialog()) {    
                    WixFiles.UndoManager.BeginNewCommandRange();

                    element.Attributes[CurrentKeyName].Value = frm.SelectedString;

                    RefreshGrid();
                }
            }
        }
    }
}