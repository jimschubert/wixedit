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
using System.Xml;

namespace WixEdit {
    /// <summary>
    /// Panel to edit global data.
    /// </summary>
    public class EditGlobalDataPanel : DisplayTreeBasePanel {
        public EditGlobalDataPanel(WixFiles wixFiles) : base(wixFiles, "/wix:Wix/*", "Id") {
            LoadData();
        }

        private StringCollection skipElements;
        protected override StringCollection SkipElements {
            get {
                if (skipElements == null) {
                    skipElements = new StringCollection();
                    skipElements.Add("Property");
                    skipElements.Add("UI");
                    skipElements.Add("Icon");
                    skipElements.Add("Binary");
                    skipElements.Add("Feature");
                    skipElements.Add("Directory");
                    skipElements.Add("DirectoryRef");
                    skipElements.Add("InstallExecuteSequence");
                    skipElements.Add("InstallUISequence");
                    skipElements.Add("AdminExecuteSequence");
                    skipElements.Add("AdminUISequence");
                    skipElements.Add("AdvertiseExecuteSequence");
                    skipElements.Add("CustomAction");
                    skipElements.Add("include");
                }

                return skipElements;
            }
        }
        
        //I am not sure if we should impelement Import function in this panel.
        //if yes it should be implemented either new function.
        protected override void PopupPanelContextMenu(System.Object sender, System.EventArgs e){
            //clear menu and add import menu
            base.PopupPanelContextMenu(sender,e);
            //add custom menu, index has to be used!!!
        }

        /// <summary>
        /// Method override a parent method from base. In this case is necesery change xpath to /*
        /// </summary>
        protected override void AssignParentNode(){
            CurrentParent = WixFiles.WxsDocument.SelectSingleNode("/*", WixFiles.WxsNsmgr);
        }

        protected override void InsertNewXmlNode(XmlNode parentElement, XmlNode newElement) {
            if (newElement.Name == "Package") {
                if (parentElement.FirstChild != null) {
                    parentElement.InsertBefore(newElement, parentElement.FirstChild);
                } else {
                    parentElement.AppendChild(newElement);
                }
            } else {
                base.InsertNewXmlNode (parentElement, newElement);
            }
        }
    }
}