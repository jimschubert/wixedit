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
using System.Collections;
using System.Windows.Forms;
using System.Data;
using System.Xml;
using System.IO;
using System.Resources;
using System.Reflection;

using WixEdit.PropertyGridExtensions;

namespace WixEdit.Settings {
	/// <summary>
	/// Form for WixEdit Settings.
	/// </summary>
	public class SettingsForm : Form 	{
        #region Controls
        protected PropertyGrid propertyGrid;
        protected ContextMenu propertyGridContextMenu;
        protected Button ok;
        protected Button cancel;
        
        #endregion

		public SettingsForm() {
            
            InitializeComponent();
		}

        #region Initialize Controls

        private void InitializeComponent() {
            this.Text = "WiX Edit Settings";
            this.Icon = new Icon(WixFiles.GetResourceStream("WixEdit.main.ico"));
            this.ClientSize = new System.Drawing.Size(360, 256); 
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.MinimizeBox = false;
            this.MaximizeBox = false;
/*
Fixed3D 
FixedDialog 
FixedSingle 
FixedToolWindow 
None 
Sizable 
SizableToolWindow 
*/

            this.ShowInTaskbar = false;

            this.propertyGrid = new CustomPropertyGrid();
            this.propertyGridContextMenu = new ContextMenu();

            // 
            // propertyGrid
            //
            this.propertyGrid.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            this.propertyGrid.Font = new Font("Tahoma", 8.25F, FontStyle.Regular, GraphicsUnit.Point, ((System.Byte)(0)));
            this.propertyGrid.Location = new Point(0, 0);
            this.propertyGrid.Name = "propertyGrid";
            this.propertyGrid.TabIndex = 1;
            this.propertyGrid.PropertySort = PropertySort.CategorizedAlphabetical;
            this.propertyGrid.ToolbarVisible = false;
            this.propertyGrid.HelpVisible = false;
            this.propertyGrid.ContextMenu = this.propertyGridContextMenu;

            // 
            // propertyGridContextMenu
            //
            this.propertyGridContextMenu.Popup += new EventHandler(OnPropertyGridPopupContextMenu);

            this.propertyGrid.SelectedObject = WixEditSettings.Instance;

            this.Controls.Add(this.propertyGrid);

            this.ok = new Button();
            this.ok.Text = "OK";
            this.ok.FlatStyle = FlatStyle.System;
            this.ok.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            this.Controls.Add(ok);
            this.ok.Click += new EventHandler(OnOk);


            this.cancel = new Button();
            this.cancel.Text = "Cancel";
            this.cancel.FlatStyle = FlatStyle.System;
            this.cancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            this.Controls.Add(cancel);       
            this.cancel.Click += new EventHandler(OnCancel);

            this.AcceptButton = ok;
            this.CancelButton = cancel;     

            int padding = 2;

            int w = (this.ClientSize.Width - this.cancel.ClientSize.Width) - padding;
            int h = (this.ClientSize.Height - this.cancel.ClientSize.Height) - padding;

            this.cancel.Location = new Point(w, h);

            w -= this.ok.ClientSize.Width + padding;
            this.ok.Location = new Point(w, h);

            h -= this.ok.ClientSize.Height + padding;

            this.propertyGrid.Size = new Size(this.ClientSize.Width, this.ClientSize.Height - (padding*2) - this.ok.ClientSize.Height);
        }

        #endregion

        public void OnPropertyGridPopupContextMenu(object sender, EventArgs e) {
        }

        private void OnOk(object sender, EventArgs e) {
            WixEditSettings.Instance.SaveChanges();
            this.DialogResult = DialogResult.OK;
        }
       
        private void OnCancel(object sender, EventArgs e) {
            WixEditSettings.Instance.DiscardChanges();
            this.DialogResult = DialogResult.Cancel;
        }        
    }
}
