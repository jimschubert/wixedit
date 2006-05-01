
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
    /// Summary description for EditUITextPanel.
    /// </summary>
    public class EditUITextPanel : DisplaySimpleBasePanel {
        public EditUITextPanel(WixFiles wixFiles) : base(wixFiles, "/wix:Wix/*/wix:UI/wix:UIText", "UIText", "Id") {
            LoadData();
        }

        protected override void AssignParentNode() {
            CurrentParent = ElementLocator.GetUIElement(WixFiles);
        }

        protected override XmlNode GetSelectedPropertyDescriptor(){
            UITextElementPropertyDescriptor desc = CurrentGrid.SelectedGridItem.PropertyDescriptor as UITextElementPropertyDescriptor;
            return desc.XmlElement;
        }

        protected override object GetPropertyAdapter(){
            return new UITextElementAdapter(CurrentList, WixFiles);
        }

        public override void OnNewPropertyGridItem(object sender, EventArgs e) {
            if (CurrentParent == null) {
                MessageBox.Show("No location found to add UI element, need element like module or product!");
                return;
            }
            base.OnNewPropertyGridItem(sender,e);            
        }
    }
}
