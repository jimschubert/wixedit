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
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;
using System.Xml;
using System.IO;
using System.Resources;
using System.Reflection;

namespace WixEdit
{
	/// <summary>
	/// Summary description for Form1.
	/// </summary>
	public class EditorForm : System.Windows.Forms.Form
	{
        private System.Windows.Forms.OpenFileDialog openWxsFileDialog;

        private TabControl tabControl;
        private TabPage editDialogPage;
        private TabPage editPropertiesPage;
        private EditDialogPanel editDialogPanel;
        private EditPropertiesPanel editPropertiesPanel;

        private System.Windows.Forms.MainMenu mainMenu;
        private System.Windows.Forms.MenuItem fileMenu;
        private System.Windows.Forms.MenuItem fileLoad;
        private System.Windows.Forms.MenuItem fileClose;

		private System.ComponentModel.Container components = null;

        private WixFiles wixFiles;

		public EditorForm() {
			InitializeComponent();
		}

        private void InitializeComponent() {
            this.Text = "WiX Edit";
            this.Icon = new Icon(WixFiles.GetResourceStream("WixEdit.main.ico"));
            this.ClientSize = new System.Drawing.Size(553, 358); // Height of 358 aligns the bottom of the dialog selection list 

            this.openWxsFileDialog = new System.Windows.Forms.OpenFileDialog();

            this.mainMenu = new System.Windows.Forms.MainMenu();
            this.fileMenu = new System.Windows.Forms.MenuItem();
            this.fileLoad = new System.Windows.Forms.MenuItem();
            this.fileClose = new System.Windows.Forms.MenuItem();          

            this.fileLoad.Text = "Load";
            this.fileLoad.Click += new System.EventHandler(this.fileLoad_Click);

            this.fileClose.Text = "Close";
            this.fileClose.Click += new System.EventHandler(this.fileClose_Click);
            this.fileClose.Enabled = false;

            this.fileMenu.Text = "File";
            this.fileMenu.MenuItems.Add(0, this.fileLoad);
            this.fileMenu.MenuItems.Add(1, this.fileClose);
            
            this.mainMenu.MenuItems.Add(0, this.fileMenu);

            this.Menu = this.mainMenu;
        }

        private void fileLoad_Click(object sender, System.EventArgs e) {
            this.openWxsFileDialog.Filter = "xml files (*.xml)|*.xml|wxs files (*.wxs)|*.wxs|All files (*.*)|*.*" ;
            this.openWxsFileDialog.RestoreDirectory = true ;

            if(this.openWxsFileDialog.ShowDialog() == DialogResult.OK) {
                CloseWxsFile();
                LoadWxsFile(this.openWxsFileDialog.FileName);
            }
        }

        private void fileClose_Click(object sender, System.EventArgs e) {
            CloseWxsFile();
        }

        private void LoadWxsFile(string file) {
            this.wixFiles = new WixFiles(new FileInfo(file));

            this.tabControl = new TabControl();
            this.tabControl.Dock = DockStyle.Fill;
            this.Controls.Add(this.tabControl);

            //this.tabControl.Alignment = TabAlignment.Left;
            this.tabControl.Appearance = TabAppearance.FlatButtons;
            
            // Add dialog tab
            this.editDialogPage = new TabPage("Dialogs");
            this.tabControl.TabPages.Add(this.editDialogPage);
            
            this.editDialogPanel = new EditDialogPanel(wixFiles);
            this.editDialogPanel.Dock = DockStyle.Fill;

            this.editDialogPage.Controls.Add(editDialogPanel);

            this.mainMenu.MenuItems.Add(1, this.editDialogPanel.Menu);

            // Add properties tab
            this.editPropertiesPage = new TabPage("Properties");
            this.tabControl.TabPages.Add(this.editPropertiesPage);
            
            this.editPropertiesPanel = new EditPropertiesPanel(wixFiles);
            this.editPropertiesPanel.Dock = DockStyle.Fill;

            this.editPropertiesPage.Controls.Add(editPropertiesPanel);


            // Update menu
            this.fileClose.Enabled = true;
            this.Text = "WiX Edit - " + this.wixFiles.WxsFile.Name;
        }

        private void CloseWxsFile() {
            if (this.tabControl != null) {
                this.Controls.Remove(tabControl);
                this.tabControl.Dispose();
                this.tabControl = null;
            }

            if (this.editDialogPanel != null) {
                if (this.mainMenu != null) {
                    this.mainMenu.MenuItems.Remove(editDialogPanel.Menu);
                }

                this.editDialogPanel.Dispose();
                this.editDialogPanel = null;
            }

            if (this.editPropertiesPanel != null) {
                this.editPropertiesPanel.Dispose();
                this.editPropertiesPanel = null;
            }

            if (this.wixFiles != null) {
                this.wixFiles.Dispose(); 
                this.wixFiles = null;
            }

            this.fileClose.Enabled = false;
            this.Text = "WiX Edit";
        }

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if (components != null) 
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		//[STAThread]
		static void Main() 
		{
			Application.Run(new EditorForm());
		}
    }
}
