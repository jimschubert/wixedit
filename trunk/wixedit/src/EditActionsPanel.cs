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
    public class EditActionsPanel : DisplayBasePanel {
        protected TabControl tabControl;
        protected TabPage editCustomActionsTabPage;
        protected EditCustomActionsPanel editCustomActionsPanel;
        protected TabPage editExecuteSequenceTabPage;
        protected EditExecuteSequencePanel editExecuteSequencePanel;

        public EditActionsPanel(WixFiles wixFiles) : base(wixFiles) {
            InitializeComponent();
        }

        #region Initialize Controls
        private void InitializeComponent() {
            tabControl = new TabControl();
            tabControl.Dock = DockStyle.Fill;

            Controls.Add(tabControl);

            editCustomActionsPanel = new EditCustomActionsPanel(wixFiles);
            editCustomActionsPanel.Dock = DockStyle.Fill;

            editCustomActionsTabPage = new TabPage("Custom Actions");
            editCustomActionsTabPage.Controls.Add(editCustomActionsPanel);

            tabControl.TabPages.Add(editCustomActionsTabPage);


            editExecuteSequencePanel = new EditExecuteSequencePanel(wixFiles);
            editExecuteSequencePanel.Dock = DockStyle.Fill;

            editExecuteSequenceTabPage = new TabPage("Execute Sequence");
            editExecuteSequenceTabPage.Controls.Add(editExecuteSequencePanel);

            tabControl.TabPages.Add(editExecuteSequenceTabPage);

        }
        #endregion

        public override bool IsOwnerOfNode(XmlNode node) {
            return (editCustomActionsPanel.IsOwnerOfNode(node) || editExecuteSequencePanel.IsOwnerOfNode(node));
        }

        public override void ShowNode(XmlNode node) {
            if (editCustomActionsPanel.IsOwnerOfNode(node)) {
                tabControl.SelectedTab = editCustomActionsTabPage;
                editCustomActionsPanel.ShowNode(node);
            } else {
                editExecuteSequencePanel.ShowNode(node);
                tabControl.SelectedTab = editExecuteSequenceTabPage;
            }
        }

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose(bool disposing) {
            if( disposing ) {
                tabControl.TabPages.Clear();
                tabControl.Dispose();
                tabControl = null;
            }
            base.Dispose( disposing );
		}
    }
}
