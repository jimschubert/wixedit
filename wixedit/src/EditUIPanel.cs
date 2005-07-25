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
    public class EditUIPanel : BasePanel {
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
            this.tabControl = new TabControl();
            this.tabControl.Dock = DockStyle.Fill;

            this.Controls.Add(tabControl);

            this.editDialogPanel = new EditDialogPanel(this.wixFiles);
            this.editDialogPanel.Dock = DockStyle.Fill;

            this.editDialogTabPage = new TabPage("Dialogs");
            this.editDialogTabPage.Controls.Add(this.editDialogPanel);

            this.tabControl.TabPages.Add(this.editDialogTabPage);


            this.editUISequencePanel = new EditUISequencePanel(this.wixFiles);
            this.editUISequencePanel.Dock = DockStyle.Fill;

            this.editUISequenceTabPage = new TabPage("UI Sequence");
            this.editUISequenceTabPage.Controls.Add(this.editUISequencePanel);

            this.tabControl.TabPages.Add(this.editUISequenceTabPage);

        }
        #endregion

        public override MenuItem Menu {
            get {
                return editDialogPanel.Menu;
            }
        }

    }
}
