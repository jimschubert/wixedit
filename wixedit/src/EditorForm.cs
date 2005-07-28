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
        protected OpenFileDialog openWxsFileDialog;

        protected TabButtonControl tabButtonControl;
        protected EditUIPanel editUIPanel;
        protected EditPropertiesPanel editPropertiesPanel;
        protected EditResourcesPanel editResourcesPanel;
        protected EditInstallDataPanel editInstallDataPanel;
        protected EditGlobalDataPanel editGlobalDataPanel;
        protected EditActionsPanel editActionsPanel;

        protected MainMenu mainMenu;
        protected MenuItem fileMenu;
        protected MenuItem fileNew;
        protected MenuItem fileLoad;
        protected MenuItem fileSave;
        protected MenuItem fileClose;
        protected MenuItem fileSeparator;
        protected MenuItem fileExit;
        protected MenuItem editMenu;
        protected MenuItem editUndo;
        protected MenuItem editRedo;
        protected MenuItem toolsMenu;
        protected MenuItem toolsOptions;
        protected MenuItem toolsWixCompile;
        protected MenuItem helpMenu;
        protected MenuItem helpAbout;

        protected OutputPanel outputPanel;
        protected Splitter outputSplitter;

        protected bool fileIsDirty;

        protected int oldTabIndex = 0;

        const int panelCount = 6;
        BasePanel[] panels = new BasePanel[panelCount];

        protected WixFiles wixFiles;

		public EditorForm() {
            InitializeComponent();
		}

        private void InitializeComponent() {
            Text = "WiX Edit";
            Icon = new Icon(WixFiles.GetResourceStream("dialog.main.ico"));
            ClientSize = new System.Drawing.Size(630, 480);

            openWxsFileDialog = new OpenFileDialog();

            mainMenu = new MainMenu();
            fileMenu = new IconMenuItem();
            fileNew = new IconMenuItem(new Bitmap(WixFiles.GetResourceStream("bmp.new.bmp")));
            fileLoad = new IconMenuItem(new Bitmap(WixFiles.GetResourceStream("bmp.open.bmp")));
            fileSave = new IconMenuItem(new Bitmap(WixFiles.GetResourceStream("bmp.save.bmp")));
            fileClose = new IconMenuItem();
            fileSeparator = new IconMenuItem("-");
            fileExit = new IconMenuItem(new Bitmap(WixFiles.GetResourceStream("bmp.exit.bmp")));

            fileNew.Text = "New";
            fileNew.Click += new System.EventHandler(fileNew_Click);
            fileNew.Shortcut = Shortcut.CtrlN;
            fileNew.ShowShortcut = true;

            fileLoad.Text = "Open";
            fileLoad.Click += new System.EventHandler(fileLoad_Click);
            fileLoad.Shortcut = Shortcut.CtrlO;
            fileLoad.ShowShortcut = true;

            fileSave.Text = "Save";
            fileSave.Click += new System.EventHandler(fileSave_Click);
            fileSave.Enabled = false;
            fileSave.Shortcut = Shortcut.CtrlS;
            fileSave.ShowShortcut = true;

            fileIsDirty = false;

            fileClose.Text = "Close";
            fileClose.Click += new System.EventHandler(fileClose_Click);
            fileClose.Enabled = false;
            fileClose.Shortcut = Shortcut.CtrlW;
            fileClose.ShowShortcut = true;

            fileExit.Text = "Exit";
            fileExit.Click += new System.EventHandler(fileExit_Click);
            fileExit.ShowShortcut = true;

            fileMenu.Text = "&File";
            fileMenu.Popup += new EventHandler(fileMenu_Popup);
            fileMenu.MenuItems.Add(0, fileNew);
            fileMenu.MenuItems.Add(1, fileLoad);
            fileMenu.MenuItems.Add(2, fileSave);
            fileMenu.MenuItems.Add(3, fileClose);
            fileMenu.MenuItems.Add(4, fileSeparator);
            fileMenu.MenuItems.Add(5, fileExit);
            
            mainMenu.MenuItems.Add(0, fileMenu);


            editMenu = new IconMenuItem();
            editUndo = new IconMenuItem(new Bitmap(WixFiles.GetResourceStream("bmp.undo.bmp")));
            editRedo = new IconMenuItem(new Bitmap(WixFiles.GetResourceStream("bmp.redo.bmp")));

            editUndo.Text = "&Undo";
            editUndo.Click += new System.EventHandler(editUndo_Click);
            editUndo.Shortcut = Shortcut.CtrlZ;
            editUndo.ShowShortcut = true;

            editRedo.Text = "&Redo";
            editRedo.Click += new System.EventHandler(editRedo_Click);
            editRedo.Shortcut = Shortcut.CtrlR;
            editRedo.ShowShortcut = true;


            editMenu.Text = "&Edit";
            editMenu.Popup += new EventHandler(editMenu_Popup);
            editMenu.MenuItems.Add(0, editUndo);
            editMenu.MenuItems.Add(1, editRedo);
            
            mainMenu.MenuItems.Add(1, editMenu);


            toolsMenu = new IconMenuItem();
            toolsOptions = new IconMenuItem(new Bitmap(WixFiles.GetResourceStream("bmp.options.bmp")));
            toolsWixCompile = new IconMenuItem(new Bitmap(WixFiles.GetResourceStream("compile.compile.bmp")));

            toolsWixCompile.Text = "Wix Compile";
            toolsWixCompile.Click += new System.EventHandler(toolsWixCompile_Click);
            toolsWixCompile.Enabled = false;

            toolsOptions.Text = "&Options";
            toolsOptions.Click += new System.EventHandler(toolsOptions_Click);

            toolsMenu.Text = "&Tools";
            toolsMenu.MenuItems.Add(0, toolsWixCompile);
            toolsMenu.MenuItems.Add(1, new IconMenuItem("-"));
            toolsMenu.MenuItems.Add(2, toolsOptions);

            mainMenu.MenuItems.Add(2, toolsMenu);


            helpMenu = new IconMenuItem();
            helpAbout = new IconMenuItem(new Icon(WixFiles.GetResourceStream("dialog.main.ico"), 16, 16));

            helpAbout.Text = "About";
            helpAbout.Click += new System.EventHandler(helpAbout_Click);

            helpMenu.Text = "&Help";
            helpMenu.MenuItems.Add(0, helpAbout);

            mainMenu.MenuItems.Add(3, helpMenu);

            Menu = mainMenu;


            tabButtonControl = new TabButtonControl();
            tabButtonControl.Dock = DockStyle.Fill;
            Controls.Add(tabButtonControl);
            tabButtonControl.Visible = false;

            tabButtonControl.TabChange += new EventHandler(OnTabChanged) ;


            outputSplitter = new Splitter();
            outputSplitter.Dock = DockStyle.Bottom;
            outputSplitter.Height = 2;
            Controls.Add(outputSplitter);

            outputPanel = new OutputPanel(wixFiles);
            outputPanel.Dock = DockStyle.Bottom;
            outputPanel.Height = 100;
            outputPanel.Size = new Size(200, 150);

            outputPanel.CloseClicked += new EventHandler(HideOutputPanel);

            Controls.Add(outputPanel);

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

        private void fileNew_Click(object sender, System.EventArgs e) {
            NewFile();
        }

        private void NewFile() {
            if (HandlePendingChanges() == false) {
                return;
            }

            NewProjectForm frm = new NewProjectForm();
            if (frm.ShowDialog() == DialogResult.OK) {
                CloseWxsFile();
                LoadWxsFile(frm.NewFileName);

                ShowProductProperties();               
            }
        }

        private void fileLoad_Click(object sender, System.EventArgs e) {
            OpenFile();
        }

        private void OpenFile() {
            if (HandlePendingChanges() == false) {
                return;
            }

            openWxsFileDialog.Filter = "WiX Files (*.xml;*.wxs;*.wxi)|*.XML;*.WXS;*.WXI|MSI Files (*.msi;*.msm)|*.MSI;*.MSM|All files (*.*)|*.*" ;
            openWxsFileDialog.RestoreDirectory = true ;

            if(openWxsFileDialog.ShowDialog() == DialogResult.OK) {
                CloseWxsFile();

                string fileToOpen = openWxsFileDialog.FileName;
                if (fileToOpen.ToLower().EndsWith("msi") || fileToOpen.ToLower().EndsWith("msm")) {
                    try {
                        // Either the wxs file doesn't exist or the user gives permission to overwrite the wxs file
                        if (File.Exists(Path.ChangeExtension(fileToOpen, "wxs")) == false ||
                            MessageBox.Show("The existing wxs file will be overwritten.\r\n\r\nAre you sure you want to continue?", "Overwrite?", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes) {
                            Decompile(fileToOpen);
                            LoadWxsFile(Path.ChangeExtension(fileToOpen, "wxs"));
                        }
                    } catch (Exception ex) {
                        MessageBox.Show(ex.Message, "Failed to decompile", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                } else {
                    LoadWxsFile(openWxsFileDialog.FileName);
                }
            }
        }

        private void fileSave_Click(object sender, System.EventArgs e) {
            wixFiles.Save();
        }

        private void fileClose_Click(object sender, System.EventArgs e) {
            if (HandlePendingChanges() == false) {
                return;
            }

            CloseWxsFile();
        }

        private void fileExit_Click(object sender, System.EventArgs e) {
            Application.Exit();
        }

        private void fileMenu_Popup(object sender, System.EventArgs e) {
            if (wixFiles != null) {
                fileSave.Enabled = wixFiles.UndoManager.HasChanges();
            }
        }

        private void editMenu_Popup(object sender, System.EventArgs e) {
            // Clear the menu, so when we change the text the 
            // IconMenuItem.OnMeasureItem will be fired.
            editMenu.MenuItems.Clear();

            if (wixFiles == null || wixFiles.UndoManager.CanUndo() == false) {
                editUndo.Enabled = false;
                editUndo.Text = "Undo";
            } else {
                editUndo.Enabled = true;
                editUndo.Text = "Undo " + wixFiles.UndoManager.GetNextUndoActionString();
            } 

            if (wixFiles == null || wixFiles.UndoManager.CanRedo() == false) {
                editRedo.Enabled = false;
                editRedo.Text = "Redo";
            } else {
                editRedo.Enabled = true;
                editRedo.Text = "Redo " + wixFiles.UndoManager.GetNextRedoActionString();
            }

            editMenu.MenuItems.Add(0, editUndo);
            editMenu.MenuItems.Add(1, editRedo);
        }

        private void editUndo_Click(object sender, System.EventArgs e) {
            XmlNode node = wixFiles.UndoManager.Undo();

            ShowNode(node);
        }
        
        
        private void editRedo_Click(object sender, System.EventArgs e) {
            XmlNode node = wixFiles.UndoManager.Redo();

            ShowNode(node);
        }

        private void ShowNode(XmlNode node) {
            if (node != null) {
                foreach (DisplayBasePanel panel in panels) {
                    if (panel.IsOwnerOfNode(node)) {
                        tabButtonControl.SelectedPanel = panel;
                        panel.ShowNode(node);
                        break;
                    }
                }
            }
        }

        private void toolsWixCompile_Click(object sender, System.EventArgs e) {
            try {
                Compile();
            } catch (Exception ex) {
                MessageBox.Show(ex.Message, "Failed to compile", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void Compile() {
            string candleExe = WixEditSettings.Instance.WixBinariesDirectory.Candle;
            if (File.Exists(candleExe) == false) {
                throw new Exception("The executable \"candle.exe\" could not be found.\r\n\r\nPlease specify the correct path to the Wix binaries in the settings dialog.");
            }

            ProcessStartInfo psiCandle = new ProcessStartInfo();
            psiCandle.FileName = candleExe;
            psiCandle.CreateNoWindow = true;
            psiCandle.UseShellExecute = false;
            psiCandle.RedirectStandardOutput = true;
            psiCandle.RedirectStandardError = false;
            psiCandle.Arguments = String.Format("-nologo \"{0}\" -out \"{1}\"", wixFiles.WxsFile.FullName, Path.ChangeExtension(wixFiles.WxsFile.FullName, "wixobj"));

            string lightExe = WixEditSettings.Instance.WixBinariesDirectory.Candle;
            if (File.Exists(lightExe) == false) {
                throw new Exception("The executable \"light.exe\" could not be found.\r\n\r\nPlease specify the correct path to the Wix binaries in the settings dialog.");
            }

            ProcessStartInfo psiLight = new ProcessStartInfo();
            psiLight.FileName = lightExe;
            psiLight.CreateNoWindow = true;
            psiLight.UseShellExecute = false;
            psiLight.RedirectStandardOutput = true;
            psiLight.RedirectStandardError = false;            
            psiLight.Arguments = String.Format("-nologo \"{0}\" -out \"{1}\"", Path.ChangeExtension(wixFiles.WxsFile.FullName, "wixobj"), Path.ChangeExtension(wixFiles.WxsFile.FullName, "msi"));

            ShowOutputPanel(null, null);
            outputPanel.Clear();
            Update();

            outputPanel.Run(new ProcessStartInfo[] {psiCandle, psiLight});
        }

        private void Decompile(string fileName) {
            FileInfo msiFile = new FileInfo(fileName);

            string darkExe = WixEditSettings.Instance.WixBinariesDirectory.Dark;
            if (File.Exists(darkExe) == false) {
                throw new Exception("The executable \"dark.exe\" could not be found.\r\n\r\nPlease specify the correct path to the Wix binaries in the settings dialog.");
            }

            ProcessStartInfo psiDark = new ProcessStartInfo();
            psiDark.FileName = darkExe;
            psiDark.CreateNoWindow = true;
            psiDark.UseShellExecute = false;
            psiDark.RedirectStandardOutput = true;
            psiDark.RedirectStandardError = false;
            psiDark.Arguments = String.Format("-nologo -x \"{0}\" \"{1}\" \"{2}\"", msiFile.DirectoryName, msiFile.FullName, Path.ChangeExtension(msiFile.FullName, "wxs"));

            ShowOutputPanel(null, null);
            outputPanel.Clear();
            Update();

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

        protected void ShowProductProperties() {
            XmlNode product = wixFiles.WxsDocument.SelectSingleNode("/wix:Wix/wix:Product", wixFiles.WxsNsmgr);
            ProductPropertiesForm frm = new ProductPropertiesForm(product, wixFiles);
            frm.ShowDialog();

            editGlobalDataPanel.Reload();
        }

        private void toolsOptions_Click(object sender, System.EventArgs e) {
            SettingsForm frm = new SettingsForm();
            frm.ShowDialog();
        }

        private void helpAbout_Click(object sender, System.EventArgs e) {
            AboutForm frm = new AboutForm();
            frm.ShowDialog();
        }

        private void OnTabChanged(object sender, EventArgs e) {
            if (oldTabIndex == tabButtonControl.SelectedIndex) {
                return;
            }

            if (panels[panels.Length - 1] == null) {
                return;
            }

            if (panels[oldTabIndex].Menu != null) {
                mainMenu.MenuItems.RemoveAt(2);
            }

            if (panels[tabButtonControl.SelectedIndex].Menu != null) {
                mainMenu.MenuItems.Add(2, panels[tabButtonControl.SelectedIndex].Menu);
            }

            oldTabIndex = tabButtonControl.SelectedIndex;
        }

        private bool HandlePendingChanges() {
            if (wixFiles != null && wixFiles.UndoManager.HasChanges() == true) {
                DialogResult result = MessageBox.Show("Save the changes you made to \""+ wixFiles.WxsFile.Name +"\"?", "Save changes?", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
                if (result == DialogResult.Yes) {
                    wixFiles.Save();
                } else if (result == DialogResult.Cancel) {
                    return false;
                }   
            }

            return true;
        }

        private void LoadWxsFile(string file) {
            wixFiles = new WixFiles(new FileInfo(file));

            tabButtonControl.Visible = true;

            // Add Global tab
            editGlobalDataPanel = new EditGlobalDataPanel(wixFiles);
            editGlobalDataPanel.Dock = DockStyle.Fill;

            tabButtonControl.AddTab("Global", editGlobalDataPanel, new Bitmap(WixFiles.GetResourceStream("tabbuttons.tabbutton.global.bmp")));

            panels[0] = editGlobalDataPanel;


            // Add Files tab
            editInstallDataPanel = new EditInstallDataPanel(wixFiles);
            editInstallDataPanel.Dock = DockStyle.Fill;

            tabButtonControl.AddTab("Files", editInstallDataPanel, new Bitmap(WixFiles.GetResourceStream("tabbuttons.tabbutton.files.bmp")));

            panels[1] = editInstallDataPanel;

            if (editInstallDataPanel.Menu != null) {
                mainMenu.MenuItems.Add(2, editInstallDataPanel.Menu);
            }

            // Add properties tab
            editPropertiesPanel = new EditPropertiesPanel(wixFiles);
            editPropertiesPanel.Dock = DockStyle.Fill;

            tabButtonControl.AddTab("Properties", editPropertiesPanel, new Bitmap(WixFiles.GetResourceStream("tabbuttons.tabbutton.properties.bmp")));

            panels[2] = editPropertiesPanel;


            // Add dialog tab
            editUIPanel = new EditUIPanel(wixFiles);
            editUIPanel.Dock = DockStyle.Fill;

            tabButtonControl.AddTab("Dialogs", editUIPanel, new Bitmap(WixFiles.GetResourceStream("tabbuttons.tabbutton.dialogs.bmp")));

            panels[3] = editUIPanel;


            // Add Resources tab
            editResourcesPanel = new EditResourcesPanel(wixFiles);
            editResourcesPanel.Dock = DockStyle.Fill;

            tabButtonControl.AddTab("Resources", editResourcesPanel, new Bitmap(WixFiles.GetResourceStream("tabbuttons.tabbutton.resources.bmp")));

            panels[4] = editResourcesPanel;


            // Add Resources tab
            editActionsPanel = new EditActionsPanel(wixFiles);
            editActionsPanel.Dock = DockStyle.Fill;

            tabButtonControl.AddTab("Actions", editActionsPanel, new Bitmap(WixFiles.GetResourceStream("tabbuttons.tabbutton.actions.bmp")));

            panels[5] = editActionsPanel;



            // Update menu
            fileClose.Enabled = true;
            Text = "WiX Edit - " + wixFiles.WxsFile.Name;

            fileSave.Enabled = true;

            toolsWixCompile.Enabled = true;
        }

        private void CloseWxsFile() {
            toolsWixCompile.Enabled = false;
            
            tabButtonControl.Visible = false;
            tabButtonControl.ClearTabs();

            panels = new BasePanel[panelCount];

            if (editUIPanel != null) {
/*
                if (mainMenu != null) {
                    mainMenu.MenuItems.Remove(editUIPanel.Menu);
                }
*/
                editUIPanel.Dispose();
                editUIPanel = null;
            }

            if (editPropertiesPanel != null) {
                editPropertiesPanel.Dispose();
                editPropertiesPanel = null;
            }

            if (wixFiles != null) {
                wixFiles.Dispose(); 
                wixFiles = null;
            }

            fileClose.Enabled = false;
            Text = "WiX Edit";

            fileSave.Enabled = false;
        }

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose(bool disposing) {
            // if( disposing ) {
            // }
            base.Dispose( disposing );
		}

		
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main() {
            
            Application.EnableVisualStyles();
            Application.DoEvents();

			Application.Run(new EditorForm());
		}
    }
}
