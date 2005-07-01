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
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using System.Data;
using System.Xml;
using System.IO;
using System.Resources;
using System.Reflection;

using WixEdit.About;
using WixEdit.Settings;

namespace WixEdit {
	/// <summary>
	/// The main dialog.
	/// </summary>
	public class EditorForm : Form 	{
        private OpenFileDialog openWxsFileDialog;

        private TabControl tabControl;
        private TabPage editDialogPage;
        private TabPage editPropertiesPage;
        private TabPage editResourcesPage;
        private TabPage editFilesPage;
        private EditDialogPanel editDialogPanel;
        private EditPropertiesPanel editPropertiesPanel;
        private EditResourcesPanel editResourcesPanel;
        private EditFilesPanel editFilesPanel;

        private MainMenu mainMenu;
        private MenuItem fileMenu;
        private MenuItem fileLoad;
        private MenuItem fileSave;
        private MenuItem fileClose;
        private MenuItem fileSeparator;
        private MenuItem fileExit;
        private MenuItem toolsMenu;
        private MenuItem toolsOptions;
        private MenuItem toolsWixCompile;
        private MenuItem toolsWixDecompile;
        private MenuItem helpMenu;
        private MenuItem helpAbout;

        protected OutputPanel outputPanel;
        private Splitter outputSplitter;

        private bool fileIsDirty;

        BasePanel[] panels = new BasePanel[4];

        private WixFiles wixFiles;

		public EditorForm() {
            InitializeComponent();
		}

        private void InitializeComponent() {
            this.Text = "WiX Edit";
            this.Icon = new Icon(WixFiles.GetResourceStream("WixEdit.main.ico"));
            this.ClientSize = new System.Drawing.Size(583, 358); // Height of 358 aligns the bottom of the dialog selection list 

            this.openWxsFileDialog = new OpenFileDialog();

            this.mainMenu = new MainMenu();
            this.fileMenu = new IconMenuItem();
            this.fileLoad = new IconMenuItem(new Bitmap(WixFiles.GetResourceStream("WixEdit.open.bmp")));
            this.fileSave = new IconMenuItem(new Bitmap(WixFiles.GetResourceStream("WixEdit.save.bmp")));
            this.fileClose = new IconMenuItem();
            this.fileSeparator = new IconMenuItem("-");
            this.fileExit = new IconMenuItem();

            this.fileLoad.Text = "Open";
            this.fileLoad.Click += new System.EventHandler(this.fileLoad_Click);
            this.fileLoad.Shortcut = Shortcut.CtrlO;
            this.fileLoad.ShowShortcut = true;

            this.fileSave.Text = "Save";
            this.fileSave.Click += new System.EventHandler(this.fileSave_Click);
            this.fileSave.Enabled = false;
            this.fileSave.Shortcut = Shortcut.CtrlS;
            this.fileSave.ShowShortcut = true;

            this.fileIsDirty = false;

            this.fileClose.Text = "Close";
            this.fileClose.Click += new System.EventHandler(this.fileClose_Click);
            this.fileClose.Enabled = false;
            this.fileClose.Shortcut = Shortcut.CtrlW;
            this.fileClose.ShowShortcut = true;

            this.fileExit.Text = "Exit";
            this.fileExit.Click += new System.EventHandler(this.fileExit_Click);
            this.fileExit.Shortcut = Shortcut.AltF4;
            this.fileExit.ShowShortcut = true;

            this.fileMenu.Text = "File";
            this.fileMenu.MenuItems.Add(0, this.fileLoad);
            this.fileMenu.MenuItems.Add(1, this.fileSave);
            this.fileMenu.MenuItems.Add(2, this.fileClose);
            this.fileMenu.MenuItems.Add(3, this.fileSeparator);
            this.fileMenu.MenuItems.Add(4, this.fileExit);
            
            this.mainMenu.MenuItems.Add(0, this.fileMenu);




            this.toolsMenu = new IconMenuItem();
            this.toolsOptions = new IconMenuItem();
            this.toolsWixCompile = new IconMenuItem(new Bitmap(WixFiles.GetResourceStream("WixEdit.compile.bmp")));
            this.toolsWixDecompile = new IconMenuItem(new Bitmap(WixFiles.GetResourceStream("WixEdit.decompile.bmp")));

            this.toolsWixCompile.Text = "Wix Compile";
            this.toolsWixCompile.Click += new System.EventHandler(this.toolsWixCompile_Click);
            this.toolsWixCompile.Enabled = false;


            this.toolsWixDecompile.Text = "Wix Decompile";
            this.toolsWixDecompile.Click += new System.EventHandler(this.toolsWixDecompile_Click);
            this.toolsWixDecompile.Enabled = false;


            this.toolsOptions.Text = "Options";
            this.toolsOptions.Click += new System.EventHandler(this.toolsOptions_Click);

            this.toolsMenu.Text = "Tools";
            this.toolsMenu.MenuItems.Add(0, this.toolsWixCompile);
            this.toolsMenu.MenuItems.Add(1, this.toolsWixDecompile);
            this.toolsMenu.MenuItems.Add(2, new IconMenuItem("-"));
            this.toolsMenu.MenuItems.Add(3, this.toolsOptions);
            
            this.mainMenu.MenuItems.Add(1, this.toolsMenu);


            this.helpMenu = new IconMenuItem();
            this.helpAbout = new IconMenuItem(new Icon(WixFiles.GetResourceStream("WixEdit.main.ico"), 16, 16));

            this.helpAbout.Text = "About";
            this.helpAbout.Click += new System.EventHandler(this.helpAbout_Click);

            this.helpMenu.Text = "Help";
            this.helpMenu.MenuItems.Add(0, this.helpAbout);

            this.mainMenu.MenuItems.Add(2, this.helpMenu);

            this.Menu = this.mainMenu;


            this.tabControl = new TabControl();
            this.tabControl.Dock = DockStyle.Fill;
            this.Controls.Add(this.tabControl);
            this.tabControl.Visible = false;

            //this.tabControl.Alignment = TabAlignment.Left;
            //this.tabControl.Appearance = TabAppearance.FlatButtons;
            this.tabControl.Click += new EventHandler(OnTabChanged) ;


            outputSplitter = new Splitter();
            outputSplitter.Dock = DockStyle.Bottom;
            outputSplitter.Height = 2;
            this.Controls.Add(outputSplitter);

            outputPanel = new OutputPanel(wixFiles);
            outputPanel.Dock = DockStyle.Bottom;
            outputPanel.Height = 100;
            outputPanel.Size = new Size(200, 150);

            outputPanel.CloseClicked += new EventHandler(HideOutputPanel);

            this.Controls.Add(outputPanel);

            outputPanel.Visible = false;
            outputSplitter.Visible = false;


            string[] args = Environment.GetCommandLineArgs();
            if (args.Length == 2) {
                string xmlFile = args[1];
                if (xmlFile != null && xmlFile.Length > 0 && 
                   (xmlFile.ToLower().EndsWith(".xml") || xmlFile.ToLower().EndsWith(".wxs"))) {
                    FileInfo xmlFileInfo = new FileInfo(xmlFile);
                    if (xmlFileInfo.Exists) {
                        LoadWxsFile(xmlFileInfo.FullName);
                    }
                }
            }
        }

        private void fileLoad_Click(object sender, System.EventArgs e) {
            this.openWxsFileDialog.Filter = "WiX Files (*.xml;*.wxs;*.wxi)|*.XML;*.WXS;*.WXI|All files (*.*)|*.*" ;
            this.openWxsFileDialog.RestoreDirectory = true ;

            if(this.openWxsFileDialog.ShowDialog() == DialogResult.OK) {
                CloseWxsFile();
                LoadWxsFile(this.openWxsFileDialog.FileName);
            }
        }

        private void fileSave_Click(object sender, System.EventArgs e) {
            ToggleDirty(false);
            this.fileSave.Enabled = true;

            this.wixFiles.Save();
        }

        private void fileClose_Click(object sender, System.EventArgs e) {
            CloseWxsFile();
        }

        private void fileExit_Click(object sender, System.EventArgs e) {
            Application.Exit();
        }

        private void toolsWixCompile_Click(object sender, System.EventArgs e) {
            if (WixEditSettings.Instance.BinDirectory == null || Directory.Exists(WixEditSettings.Instance.BinDirectory) == false) {
                MessageBox.Show("Please specify the path to the Wix binaries in the settings dialog.");

                return;
            }

            ShowOutputPanel(null, null);

            outputPanel.Clear();
            this.Update();

            string candleExe = Path.Combine(WixEditSettings.Instance.BinDirectory, "Candle.exe");
            if (File.Exists(candleExe) == false) {
                MessageBox.Show("candle.exe not found. Please specify the correct path to the Wix binaries in the settings dialog.");

                return;
            }

            ProcessStartInfo psiCandle = new ProcessStartInfo();
            psiCandle.FileName = candleExe;
            psiCandle.CreateNoWindow = true;
            psiCandle.UseShellExecute = false;
            psiCandle.RedirectStandardOutput = true;
            psiCandle.RedirectStandardError = false;
            psiCandle.Arguments = String.Format("-nologo \"{0}\" -out \"{1}\"", wixFiles.WxsFile.FullName, Path.ChangeExtension(wixFiles.WxsFile.FullName, "wixobj"));

            string lightExe = Path.Combine(WixEditSettings.Instance.BinDirectory, "Light.exe");
            if (File.Exists(lightExe) == false) {
                MessageBox.Show("light.exe not found. Please specify the correct path to the Wix binaries in the settings dialog.");

                return;
            }

            ProcessStartInfo psiLight = new ProcessStartInfo();
            psiLight.FileName = lightExe;
            psiLight.CreateNoWindow = true;
            psiLight.UseShellExecute = false;
            psiLight.RedirectStandardOutput = true;
            psiLight.RedirectStandardError = false;            
            psiLight.Arguments = String.Format("-nologo \"{0}\" -out \"{1}\"", Path.ChangeExtension(wixFiles.WxsFile.FullName, "wixobj"), Path.ChangeExtension(wixFiles.WxsFile.FullName, "msi"));

            outputPanel.Run(new ProcessStartInfo[] {psiCandle, psiLight});
        }

        private void toolsWixDecompile_Click(object sender, System.EventArgs e) {
            if (WixEditSettings.Instance.BinDirectory == null || Directory.Exists(WixEditSettings.Instance.BinDirectory) == false) {
                MessageBox.Show("Please specify the path to the Wix binaries in the settings dialog.");

                return;
            }

            OpenFileDialog openMsiFileDialog = new OpenFileDialog();

            openMsiFileDialog.Filter = "msi files (*.msi)|*.msi|msm files (*.msm)|*.msm" ;
            openMsiFileDialog.FilterIndex = 1 ;

            openMsiFileDialog.RestoreDirectory = true ;

            if(openMsiFileDialog.ShowDialog() != DialogResult.OK) {
                return;
            }

            FileInfo msiFile = new FileInfo(openMsiFileDialog.FileName);

            ShowOutputPanel(null, null);

            outputPanel.Clear();
            this.Update();

            string darkExe = Path.Combine(WixEditSettings.Instance.BinDirectory, "Dark.exe");
            if (File.Exists(darkExe) == false) {
                MessageBox.Show("dark.exe not found. Please specify the correct path to the Wix binaries in the settings dialog.");

                return;
            }

            ProcessStartInfo psiDark = new ProcessStartInfo();
            psiDark.FileName = darkExe;
            psiDark.CreateNoWindow = true;
            psiDark.UseShellExecute = false;
            psiDark.RedirectStandardOutput = true;
            psiDark.RedirectStandardError = false;
            psiDark.Arguments = String.Format("-nologo -x \"{0}\" \"{1}\" \"{2}\"", msiFile.DirectoryName, msiFile.FullName, Path.ChangeExtension(msiFile.FullName, "wxs"));

            outputPanel.Run(psiDark);
        }

        private void ShowOutputPanel(object sender, System.EventArgs e) {
            outputSplitter.Visible = true;
            outputPanel.Visible = true;
        }

        private void HideOutputPanel(object sender, System.EventArgs e) {
            outputSplitter.Visible = false;
            outputPanel.Visible = false;
        }

        private void toolsOptions_Click(object sender, System.EventArgs e) {
            SettingsForm frm = new SettingsForm();
            frm.ShowDialog();
        }

        private void helpAbout_Click(object sender, System.EventArgs e) {
            AboutForm frm = new AboutForm();
            frm.ShowDialog();
        }

        int oldTabIndex = 0;
        private void OnTabChanged(object sender, EventArgs e) {
            if (oldTabIndex == tabControl.SelectedIndex) {
                return;
            }

            if (panels[panels.Length - 1] == null) {
                return;
            }

            if (panels[oldTabIndex].Menu != null) {
                this.mainMenu.MenuItems.RemoveAt(1);
            }

            if (panels[tabControl.SelectedIndex].Menu != null) {
                this.mainMenu.MenuItems.Add(1, panels[tabControl.SelectedIndex].Menu);
            }

            oldTabIndex = tabControl.SelectedIndex;
        }

        private void LoadWxsFile(string file) {
            this.wixFiles = new WixFiles(new FileInfo(file));

            this.tabControl.Visible = true;

            // Add dialog tab
            this.editDialogPage = new TabPage("Dialogs");
            this.tabControl.TabPages.Add(this.editDialogPage);
            
            this.editDialogPanel = new EditDialogPanel(wixFiles);
            this.editDialogPanel.Dock = DockStyle.Fill;

            this.editDialogPage.Controls.Add(editDialogPanel);

            panels[0] = editDialogPanel;

            this.mainMenu.MenuItems.Add(1, this.editDialogPanel.Menu);


            // Add properties tab
            this.editPropertiesPage = new TabPage("Properties");
            this.tabControl.TabPages.Add(this.editPropertiesPage);
            
            this.editPropertiesPanel = new EditPropertiesPanel(wixFiles);
            this.editPropertiesPanel.Dock = DockStyle.Fill;

            this.editPropertiesPage.Controls.Add(editPropertiesPanel);

            panels[1] = editPropertiesPanel;

            // Add Resources tab
            this.editResourcesPage = new TabPage("Resources");
            this.tabControl.TabPages.Add(this.editResourcesPage);

            this.editResourcesPanel = new EditResourcesPanel(wixFiles);
            this.editResourcesPanel.Dock = DockStyle.Fill;

            this.editResourcesPage.Controls.Add(editResourcesPanel);

            panels[2] = editResourcesPanel;


            // Add Files tab
            this.editFilesPage = new TabPage("Files");
            this.tabControl.TabPages.Add(this.editFilesPage);

            this.editFilesPanel = new EditFilesPanel(wixFiles);
            this.editFilesPanel.Dock = DockStyle.Fill;

            this.editFilesPage.Controls.Add(editFilesPanel);

            panels[3] = editFilesPanel;


            // Update menu
            this.fileClose.Enabled = true;
            this.Text = "WiX Edit - " + this.wixFiles.WxsFile.Name;

            ToggleDirty(false);
            this.fileSave.Enabled = true;

            this.toolsWixCompile.Enabled = true;
            this.toolsWixDecompile.Enabled = true;

        }

        private void ToggleDirty(bool dirty) {
            if (dirty != this.fileIsDirty) {
                this.fileIsDirty = dirty;
                this.fileSave.Enabled = dirty;
            }
        }

        private void CloseWxsFile() {
            this.toolsWixCompile.Enabled = false;
            this.toolsWixDecompile.Enabled = false;
            
            this.tabControl.Visible = false;
            this.tabControl.TabPages.Clear();

            panels = new BasePanel[4];

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

            ToggleDirty(false);
            this.fileSave.Enabled = false;
        }

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing ) {
			}
			base.Dispose( disposing );
		}

		
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main() 
		{
			Application.Run(new EditorForm());
		}
    }
}
