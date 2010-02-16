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
    /// Panel to edit CustomTable definition data.
    /// </summary>
    public class EditCustomTableDefinitionPanel : DisplayTreeBasePanel {
        public EditCustomTableDefinitionPanel(WixFiles wixFiles) : base(wixFiles, "/wix:Wix//wix:CustomTable", "Id") {
            LoadData();
        }

        private StringCollection skipElements;
        protected override StringCollection SkipElements
        {
            get
            {
                if (skipElements == null)
                {
                    skipElements = new StringCollection();
                    skipElements.Add("Row");
                }

                return skipElements;
            }
        }

        protected override void PopupPanelContextMenu(System.Object sender, System.EventArgs e) {
            //clear menu and add import menu
            base.PopupPanelContextMenu(sender,e);
     
            //add custom menu, index has to be used!!!
            IconMenuItem subMenuItem = new IconMenuItem("New CustomTable", new Bitmap(WixFiles.GetResourceStream("bmp.new.bmp")));

            subMenuItem.Click += new EventHandler(NewCustomElement_Click);

            PanelContextMenu.MenuItems.Add(0,subMenuItem);
        }
        
        protected override void NewCustomElement_Click(object sender, System.EventArgs e) {
            MenuItem item = (MenuItem) sender;
            CreateNewCustomElement("CustomTable");
        }

        public override bool IsOwnerOfNode(XmlNode node)
        {
            if (node.Name == "Row")
            {
                return false;
            }

            return base.IsOwnerOfNode(node);
        }
    }
}