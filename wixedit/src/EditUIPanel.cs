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
using System.Xml;
using System.Windows.Forms;

namespace WixEdit {
    /// <summary>
    /// Panel to edit install data.
    /// </summary>
    public class EditUIPanel : DisplayBasePanel {
        protected TabControl tabControl;
        protected TabPage editDialogTabPage;
        protected EditDialogPanel editDialogPanel;
        protected TabPage editUISequenceTabPage;
        protected EditUISequencePanel editUISequencePanel;

        public EditUIPanel(WixFiles wixFiles) : base(wixFiles) {
            InitializeComponent();
        }

        #region Initialize Controls
        private void InitializeComponent() {
            tabControl = new TabControl();
            tabControl.Dock = DockStyle.Fill;

            Controls.Add(tabControl);

            editDialogPanel = new EditDialogPanel(wixFiles);
            editDialogPanel.Dock = DockStyle.Fill;

            editDialogTabPage = new TabPage("Dialogs");
            editDialogTabPage.Controls.Add(editDialogPanel);

            tabControl.TabPages.Add(editDialogTabPage);


            editUISequencePanel = new EditUISequencePanel(wixFiles);
            editUISequencePanel.Dock = DockStyle.Fill;

            editUISequenceTabPage = new TabPage("UI Sequence");
            editUISequenceTabPage.Controls.Add(editUISequencePanel);

            tabControl.TabPages.Add(editUISequenceTabPage);

        }
        #endregion

        public override MenuItem Menu {
            get {
                return editDialogPanel.Menu;
            }
        }

        public override bool IsOwnerOfNode(XmlNode node) {
            bool ret = (editDialogPanel.IsOwnerOfNode(node) || editUISequencePanel.IsOwnerOfNode(node));
            if (ret == false) {
                if (node.Name == "UI") {
                    ret = true;
                }
            }

            return ret;
        }

        public override void ShowNode(XmlNode node) {
            if (editDialogPanel.IsOwnerOfNode(node)) {
                tabControl.SelectedTab = editDialogTabPage;
                editDialogPanel.ShowNode(node);
            } else if (editUISequencePanel.IsOwnerOfNode(node)) {
                tabControl.SelectedTab = editUISequenceTabPage;
                editUISequencePanel.ShowNode(node);
            } else {
                tabControl.SelectedTab = editDialogTabPage;
            }
        }

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose(bool disposing) {
            if( disposing ) {
                editDialogPanel.Dispose();
                editDialogPanel = null;
                editUISequencePanel.Dispose();
                editUISequencePanel = null;

                tabControl.TabPages.Clear();
                tabControl.Dispose();
                tabControl = null;
            }
            base.Dispose( disposing );
		}
    }
}
