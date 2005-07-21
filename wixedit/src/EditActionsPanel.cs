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
using System.Windows.Forms;

namespace WixEdit {
    /// <summary>
    /// Panel to edit install data.
    /// </summary>
    public class EditActionsPanel : BasePanel {
        protected TabControl tabControl;
        protected TabPage editCustomActionsTabPage;
        protected Panel editCustomActionsPanel;
        protected TabPage editExecuteSequenceTabPage;
        protected Panel editExecuteSequencePanel;

        public EditActionsPanel(WixFiles wixFiles) : base(wixFiles) {
            InitializeComponent();
        }

        #region Initialize Controls
        private void InitializeComponent() {
            this.tabControl = new TabControl();
            this.tabControl.Dock = DockStyle.Fill;

            this.Controls.Add(tabControl);

            this.editCustomActionsPanel = new EditCustomActionsPanel(this.wixFiles);
            this.editCustomActionsPanel.Dock = DockStyle.Fill;

            this.editCustomActionsTabPage = new TabPage("Custom Actions");
            this.editCustomActionsTabPage.Controls.Add(this.editCustomActionsPanel);

            this.tabControl.TabPages.Add(this.editCustomActionsTabPage);


            this.editExecuteSequencePanel = new EditExecuteSequencePanel(this.wixFiles);
            this.editExecuteSequencePanel.Dock = DockStyle.Fill;

            this.editExecuteSequenceTabPage = new TabPage("Execute Sequence");
            this.editExecuteSequenceTabPage.Controls.Add(this.editExecuteSequencePanel);

            this.tabControl.TabPages.Add(this.editExecuteSequenceTabPage);

        }
        #endregion
    }
}
