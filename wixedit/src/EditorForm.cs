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
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using System.Data;
using System.Xml;
using System.IO;
using System.Resources;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;

using WixEdit.About;
using WixEdit.Controls;
using WixEdit.Settings;

namespace WixEdit {
    /// <summary>
    /// The main dialog.
    /// </summary>
    public class EditorForm : Form {
        protected OpenFileDialog openWxsFileDialog;

        protected AboutForm splash;
        protected bool splashIsDone = false;
        protected EventHandler splashScreenHandler;

        protected Panel mainPanel;
        protected TabButtonControl tabButtonControl;
        protected EditUIPanel editUIPanel;
        protected EditPropertiesPanel editPropertiesPanel;
        protected EditResourcesPanel editResourcesPanel;
        protected EditInstallDataPanel editInstallDataPanel;
        protected EditGlobalDataPanel editGlobalDataPanel;
        protected EditActionsPanel editActionsPanel;

        protected MainMenu mainMenu;
        protected IconMenuItem fileMenu;
        protected IconMenuItem fileNew;
        protected IconMenuItem fileLoad;
        protected IconMenuItem fileRecent;
        protected IconMenuItem fileRecentEmpty;
        protected IconMenuItem fileRecentClean;
        protected IconMenuItem fileRecentClear;
        protected IconMenuItem fileSave;
        protected IconMenuItem fileSaveAs;
        protected IconMenuItem fileClose;
        protected IconMenuItem fileSeparator;
        protected IconMenuItem fileExit;
        protected IconMenuItem editMenu;
        protected IconMenuItem editUndo;
        protected IconMenuItem editRedo;
        protected IconMenuItem editFind;
        protected IconMenuItem toolsMenu;
        protected IconMenuItem toolsExternal;
        protected IconMenuItem toolsOptions;
        protected IconMenuItem buildProjectSettings;
        protected IconMenuItem buildWixCompile;
        protected IconMenuItem buildWixInstall;
        protected IconMenuItem buildWixUninstall;
        protected IconMenuItem buildMenu;
        protected IconMenuItem helpMenu;
        protected IconMenuItem helpAbout;
        protected IconMenuItem helpTutorial;
        protected IconMenuItem helpMSIReference;
        
        protected ResultsPanel resultsPanel;
        protected SearchPanel searchPanel;
        protected OutputPanel outputPanel;
        protected Splitter resultsSplitter;

        protected bool fileIsDirty;

        protected int oldTabIndex = -1;

        const int panelCount = 6;
        BasePanel[] panels = new BasePanel[panelCount];

        protected WixFiles wixFiles;

        protected static ArrayList formInstances = new ArrayList();

        public EditorForm() {
            InitializeComponent();
        }

        public EditorForm(string fileToOpen) {
            InitializeComponent();

            FileInfo xmlFileInfo = new FileInfo(fileToOpen);
            if (xmlFileInfo.Exists) {
                LoadWxsFile(xmlFileInfo);
            }
        }

        private void InitializeComponent() {
            formInstances.Add(this);

            Text = "WiX Edit";
            Icon = new Icon(WixFiles.GetResourceStream("dialog.source.ico"));
            ClientSize = new Size(630, 480);
            MinimumSize = new Size(250, 200);
            
            openWxsFileDialog = new OpenFileDialog();

            mainMenu = new MainMenu();
            fileMenu = new IconMenuItem();
            fileNew = new IconMenuItem(new Bitmap(WixFiles.GetResourceStream("bmp.new.bmp")));
            fileLoad = new IconMenuItem(new Bitmap(WixFiles.GetResourceStream("bmp.open.bmp")));
            fileRecent = new IconMenuItem();
            fileRecentEmpty = new IconMenuItem();
            fileRecentClean = new IconMenuItem();
            fileRecentClear = new IconMenuItem();
            fileSave = new IconMenuItem(new Bitmap(WixFiles.GetResourceStream("bmp.save.bmp")));
            fileSaveAs = new IconMenuItem(new Bitmap(WixFiles.GetResourceStream("bmp.saveas.bmp")));
            fileClose = new IconMenuItem();
            fileSeparator = new IconMenuItem("-");
            fileExit = new IconMenuItem(new Bitmap(WixFiles.GetResourceStream("bmp.exit.bmp")));

            fileNew.Text = "&New";
            fileNew.Click += new EventHandler(fileNew_Click);
            fileNew.Shortcut = Shortcut.CtrlN;
            fileNew.ShowShortcut = true;

            fileLoad.Text = "&Open";
            fileLoad.Click += new EventHandler(fileLoad_Click);
            fileLoad.Shortcut = Shortcut.CtrlO;
            fileLoad.ShowShortcut = true;

            fileRecentEmpty.Text = "Empty";
            fileRecentEmpty.Enabled = false;
            fileRecentEmpty.ShowShortcut = true;

            fileRecent.Text = "&Reopen";
            fileRecent.Popup += new EventHandler(fileRecent_Popup);
            fileRecent.ShowShortcut = true;
            fileRecent.MenuItems.Add(0, fileRecentEmpty);

            fileRecentClean.Text = "Remove obsolete";
            fileRecentClean.Click += new EventHandler(recentFileClean_Click);
            fileRecentClean.ShowShortcut = true;

            fileRecentClear.Text = "Remove all";
            fileRecentClear.Click += new EventHandler(recentFileClear_Click);
            fileRecentClear.ShowShortcut = true;

            fileSave.Text = "&Save";
            fileSave.Click += new EventHandler(fileSave_Click);
            fileSave.Enabled = false;
            fileSave.Shortcut = Shortcut.CtrlS;
            fileSave.ShowShortcut = true;

            fileSaveAs.Text = "Save &As";
            fileSaveAs.Click += new EventHandler(fileSaveAs_Click);
            fileSaveAs.Enabled = false;
            fileSaveAs.ShowShortcut = false;

            fileIsDirty = false;

            fileClose.Text = "&Close";
            fileClose.Click += new EventHandler(fileClose_Click);
            fileClose.Enabled = false;
            fileClose.Shortcut = Shortcut.CtrlW;
            fileClose.ShowShortcut = true;

            fileExit.Text = "&Exit";
            fileExit.Click += new EventHandler(fileExit_Click);
            fileExit.ShowShortcut = true;

            fileMenu.Text = "&File";
            fileMenu.Popup += new EventHandler(fileMenu_Popup);
            fileMenu.MenuItems.Add(0, fileNew);
            fileMenu.MenuItems.Add(1, fileLoad);
            fileMenu.MenuItems.Add(2, fileRecent);
            fileMenu.MenuItems.Add(3, fileSave);
            fileMenu.MenuItems.Add(4, fileSaveAs);
            fileMenu.MenuItems.Add(5, fileClose);
            fileMenu.MenuItems.Add(6, fileSeparator);
            fileMenu.MenuItems.Add(7, fileExit);
            
            mainMenu.MenuItems.Add(0, fileMenu);


            editMenu = new IconMenuItem();
            editUndo = new IconMenuItem(new Bitmap(WixFiles.GetResourceStream("bmp.undo.bmp")));
            editRedo = new IconMenuItem(new Bitmap(WixFiles.GetResourceStream("bmp.redo.bmp")));
            editFind = new IconMenuItem(new Bitmap(WixFiles.GetResourceStream("bmp.find.bmp")));

            if (wixFiles == null ||
                WixEditSettings.Instance.ExternalXmlEditor == null ||
                File.Exists(WixEditSettings.Instance.ExternalXmlEditor) == false) {
                toolsExternal = new IconMenuItem();
            } else {
                Icon ico = FileIconFactory.GetFileIcon(WixEditSettings.Instance.ExternalXmlEditor);
                toolsExternal = new IconMenuItem(ico);
            }

            editUndo.Text = "&Undo";
            editUndo.Click += new EventHandler(editUndo_Click);
            editUndo.Shortcut = Shortcut.CtrlZ;
            editUndo.ShowShortcut = true;

            editRedo.Text = "&Redo";
            editRedo.Click += new EventHandler(editRedo_Click);
            editRedo.Shortcut = Shortcut.CtrlR;
            editRedo.ShowShortcut = true;

            editFind.Text = "&Find";
            editFind.Click += new EventHandler(editFind_Click);
            editFind.Shortcut = Shortcut.CtrlF;
            editFind.ShowShortcut = true;

            editMenu.Text = "&Edit";
            editMenu.Popup += new EventHandler(editMenu_Popup);
            editMenu.MenuItems.Add(0, editUndo);
            editMenu.MenuItems.Add(1, editRedo);
            editMenu.MenuItems.Add(2, new IconMenuItem("-"));
            editMenu.MenuItems.Add(3, editFind);
            
            mainMenu.MenuItems.Add(1, editMenu);


            toolsMenu = new IconMenuItem();
            toolsOptions = new IconMenuItem(new Bitmap(WixFiles.GetResourceStream("bmp.options.bmp")));
            buildProjectSettings = new IconMenuItem(new Bitmap(WixFiles.GetResourceStream("bmp.settings.bmp")));
            buildWixCompile = new IconMenuItem(new Bitmap(WixFiles.GetResourceStream("compile.compile.bmp")));
            buildWixInstall = new IconMenuItem(new Bitmap(WixFiles.GetResourceStream("compile.uninstall.bmp")));
            buildWixUninstall = new IconMenuItem(new Bitmap(WixFiles.GetResourceStream("compile.install.bmp")));
            buildMenu = new IconMenuItem();

            buildWixCompile.Text = "&Build MSI setup package";
            buildWixCompile.Click += new EventHandler(buildWixCompile_Click);
            buildWixCompile.Enabled = false;
            buildWixCompile.Shortcut = Shortcut.CtrlB;
            buildWixCompile.ShowShortcut = true;

            buildWixInstall.Text = "&Install MSI setup package";
            buildWixInstall.Click += new EventHandler(buildWixInstall_Click);
            buildWixInstall.Enabled = false;
            buildWixInstall.Shortcut = Shortcut.F5;
            buildWixInstall.ShowShortcut = true;

            buildWixUninstall.Text = "&Uninstall MSI setup package";
            buildWixUninstall.Click += new EventHandler(buildWixUninstall_Click);
            buildWixUninstall.Enabled = false;
            buildWixUninstall.Shortcut = Shortcut.ShiftF5;
            buildWixUninstall.ShowShortcut = true;

            buildProjectSettings.Text = "&Build Settings";
            buildProjectSettings.Click += new EventHandler(buildProjectSettings_Click);
            buildProjectSettings.Enabled = false;

            buildMenu.Text = "&Build";
            buildMenu.Popup += new EventHandler(buildMenu_Popup);
            buildMenu.MenuItems.Add(0, buildWixCompile);
            buildMenu.MenuItems.Add(1, new IconMenuItem("-"));
            buildMenu.MenuItems.Add(2, buildWixInstall);
            buildMenu.MenuItems.Add(3, buildWixUninstall);
            buildMenu.MenuItems.Add(4, new IconMenuItem("-"));
            buildMenu.MenuItems.Add(5, buildProjectSettings);

            mainMenu.MenuItems.Add(2, buildMenu);

            toolsOptions.Text = "&Options";
            toolsOptions.Click += new EventHandler(toolsOptions_Click);

            toolsExternal.Text = "Launch &External Editor";
            toolsExternal.Click += new EventHandler(toolsExternal_Click);
            toolsExternal.Shortcut = Shortcut.CtrlE;
            toolsExternal.ShowShortcut = true;

            toolsMenu.Text = "&Tools";
            toolsMenu.Popup += new EventHandler(toolsMenu_Popup);
            toolsMenu.MenuItems.Add(0, toolsExternal);
            toolsMenu.MenuItems.Add(1, new IconMenuItem("-"));
            toolsMenu.MenuItems.Add(2, toolsOptions);

            mainMenu.MenuItems.Add(3, toolsMenu);


            helpMenu = new IconMenuItem();
            helpTutorial = new IconMenuItem(new Bitmap(WixFiles.GetResourceStream("bmp.web.bmp")));
            helpMSIReference = new IconMenuItem(new Bitmap(WixFiles.GetResourceStream("bmp.web.bmp")));
            helpAbout = new IconMenuItem(new Icon(WixFiles.GetResourceStream("dialog.source.ico"), 16, 16));

            helpTutorial.Text = "WiX Tutorial";
            helpTutorial.Click += new EventHandler(helpTutorial_Click);

            helpMSIReference.Text = "Windows Installer Reference";
            helpMSIReference.Click += new EventHandler(helpMSIReference_Click);

            helpAbout.Text = "&About";
            helpAbout.Click += new EventHandler(helpAbout_Click);

            helpMenu.Text = "&Help";
            helpMenu.MenuItems.Add(0, helpTutorial);
            helpMenu.MenuItems.Add(1, helpMSIReference);
            helpMenu.MenuItems.Add(2, new IconMenuItem("-"));
            helpMenu.MenuItems.Add(3, helpAbout);

            mainMenu.MenuItems.Add(4, helpMenu);

            Menu = mainMenu;

            mainPanel = new Panel();
            mainPanel.Dock = DockStyle.Fill;
            Controls.Add(mainPanel);

            resultsSplitter = new Splitter();
            resultsSplitter.Dock = DockStyle.Bottom;
            resultsSplitter.Height = 2;
            Controls.Add(resultsSplitter);

            outputPanel = new OutputPanel();
            outputPanel.Text = "Output";
            searchPanel = new SearchPanel(this);
            searchPanel.Text = "Search Results";

            resultsPanel = new ResultsPanel(new Panel[] { outputPanel, searchPanel });
            resultsPanel.CloseClicked += new EventHandler(HideResultsPanel);            
            resultsPanel.Dock = DockStyle.Bottom;
            resultsPanel.Height = 100;
            resultsPanel.Size = new Size(200, 150);

            Controls.Add(resultsPanel);

            resultsPanel.Visible = false;
            resultsSplitter.Visible = false;

            splashScreenHandler = new EventHandler(EditorForm_Activated);
            this.Activated += splashScreenHandler;
            if (formInstances.Count > 1) {
                splashIsDone = true;
            }
        }

        private void EditorForm_Activated(object sender, System.EventArgs e) {
            if (splashIsDone) {
                HideSplash();
            } else {
                ShowSplash();

                System.Windows.Forms.Timer t = new System.Windows.Forms.Timer();
                t.Interval = 1500;
                t.Tick += new EventHandler(t_Tick);
                t.Start();
            }
        }

        private void ShowSplash() {
            splash = new AboutForm();
            splash.StartPosition = FormStartPosition.Manual;
            splash.Left = this.Left + (this.Width/2) - (splash.Width/2);
            splash.Top = this.Top + (this.Height/2) - (splash.Height/2);
            splash.Show();

            splashIsDone = true;
        }

        private void t_Tick(object sender, EventArgs e) {
            System.Windows.Forms.Timer t = sender as System.Windows.Forms.Timer;
            t.Stop();
            t.Enabled = false;

            HideSplash();
        }

        private void HideSplash() {
            // Need to lock for threading.
            // Cannot lock splash because that can be null.
            lock (splashScreenHandler) {
                if (splash != null) {
                    splash.Close();
                    splash = null;
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
                try {
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
                } catch (Exception ex) {
                    MessageBox.Show(String.Format("Failed to open {0}.\r\n({1}\r\n{2})", fileToOpen, ex.Message, ex.StackTrace)); 
                }
            }
        }

        private void fileSave_Click(object sender, System.EventArgs e) {
            wixFiles.Save();
        }

        private void fileSaveAs_Click(object sender, System.EventArgs e) {
            SaveFileDialog dlg = new SaveFileDialog();
            dlg.OverwritePrompt = true;
            dlg.FileName = wixFiles.WxsFile.FullName;
            dlg.Filter = "WiX Files (*.xml;*.wxs;*.wxi)|*.XML;*.WXS;*.WXI|All files (*.*)|*.*" ;
            dlg.AddExtension = true;
            dlg.DefaultExt = "wxs";
            if (dlg.ShowDialog(this) == DialogResult.OK) {
                wixFiles.SaveAs(dlg.FileName);
            }
        }

        private void fileClose_Click(object sender, System.EventArgs e) {
            if (HandlePendingChanges() == false) {
                return;
            }

            CloseWxsFile();

            if (formInstances.Count > 1) {
                this.Close();
            }
        }

        public delegate void VoidDelegate();

        private void fileExit_Click(object sender, System.EventArgs e) {
            EditorForm[] constEditorArray = new EditorForm[formInstances.Count];
            formInstances.CopyTo(constEditorArray);
            for (int i = 0; i < constEditorArray.Length; i++) {

                EditorForm edit = constEditorArray[i];

                if (edit == this) {
                    continue;
                }

                edit.Invoke(new VoidDelegate(edit.Close));
            }

            this.Close();
        }

        private void fileMenu_Popup(object sender, System.EventArgs e) {
            if (wixFiles != null) {
                fileSave.Enabled = wixFiles.HasChanges();
            }
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e) {
            if (wixFiles != null) {
                if (wixFiles.HasChanges()) {
                    if (HandlePendingChanges() == false) {
                        e.Cancel = true;
                        return;
                    }
                }

                CloseWxsFile();
            }

            this.Hide();

            if (formInstances.Contains(this)) {
                formInstances.Remove(this);
            }

            if (formInstances.Count == 0) {
                Application.Exit();
            }
        }

        private void fileRecent_Popup(object sender, System.EventArgs e) {
            // Clear the menu
            fileRecent.MenuItems.Clear();

            string[] recentFiles = WixEditSettings.Instance.GetRecentlyUsedFiles();
            if (recentFiles.Length == 0) {
                fileRecent.MenuItems.Add(0, fileRecentEmpty);
            } else {
                int i = 0;
                foreach (string recentFile in recentFiles) {
                    string recentFileText = recentFile;
                    if (recentFile.Length > 100) {
                        recentFileText = "..." + recentFile.Substring(recentFile.Length - 98, 98);
                    }

                    IconMenuItem recentFileMenuItem = new IconMenuItem();
                    recentFileMenuItem.Text = String.Format("&{0} {1}", i+1, recentFileText);
                    recentFileMenuItem.Click += new EventHandler(recentFile_Click);

                    if (File.Exists(recentFile)) {
                        Icon ico = FileIconFactory.GetFileIcon(recentFile);
                        recentFileMenuItem.Bitmap = ico.ToBitmap();
                    } else {
                        recentFileMenuItem.Enabled = false;
                    }

                    fileRecent.MenuItems.Add(i, recentFileMenuItem);

                    i++;
                }

                fileRecent.MenuItems.Add(i, new IconMenuItem("-"));
                fileRecent.MenuItems.Add(i+1, fileRecentClean);
                fileRecent.MenuItems.Add(i+2, fileRecentClear);
            }
        }

        private void editMenu_Popup(object sender, System.EventArgs e) {
            // Clear the menu, so when we change the text the 
            // IconMenuItem.OnMeasureItem will be fired.
            editMenu.MenuItems.Clear();

            if (wixFiles == null || wixFiles.UndoManager.CanUndo() == false) {
                editUndo.Enabled = false;
                editUndo.Text = "&Undo";
            } else {
                editUndo.Enabled = true;
                editUndo.Text = "&Undo " + wixFiles.UndoManager.GetNextUndoActionString();
            } 

            if (wixFiles == null || wixFiles.UndoManager.CanRedo() == false) {
                editRedo.Enabled = false;
                editRedo.Text = "&Redo";
            } else {
                editRedo.Enabled = true;
                editRedo.Text = "&Redo " + wixFiles.UndoManager.GetNextRedoActionString();
            }

            editMenu.MenuItems.Add(0, editUndo);
            editMenu.MenuItems.Add(1, editRedo);
            editMenu.MenuItems.Add(2, new IconMenuItem("-"));
            editMenu.MenuItems.Add(3, editFind);
            editFind.Enabled = (wixFiles != null);
        }

        private void toolsMenu_Popup(object sender, System.EventArgs e) {
            bool hasExternalEditor = (WixEditSettings.Instance.ExternalXmlEditor != null && File.Exists(WixEditSettings.Instance.ExternalXmlEditor));

            if (wixFiles == null || hasExternalEditor == false) {
                toolsExternal.Enabled = false;
            } else {
                toolsExternal.Enabled = true;
            }

            if (toolsExternal.HasIcon() == false && hasExternalEditor) {
                Icon ico = FileIconFactory.GetFileIcon(WixEditSettings.Instance.ExternalXmlEditor);
                toolsExternal.Bitmap = ico.ToBitmap();
            }
            if (toolsExternal.HasIcon() == true && hasExternalEditor == false) {
                toolsExternal.Bitmap = null;
            }
        }

        private void buildMenu_Popup(object sender, EventArgs e) {
            bool isEnabled = false;
            if (wixFiles != null) {
                if (wixFiles.OutputFile.Exists) {
                    isEnabled = true;
                }
            }

            buildWixInstall.Enabled = isEnabled;
            buildWixUninstall.Enabled = isEnabled;
        }

        private void recentFileClear_Click(object sender, System.EventArgs e) {
            WixEditSettings.Instance.ClearRecentlyUsedFiles();
            WixEditSettings.Instance.SaveChanges();
        }

        private void recentFileClean_Click(object sender, System.EventArgs e) {
            WixEditSettings.Instance.CleanRecentlyUsedFiles();
            WixEditSettings.Instance.SaveChanges();
        }

        private void recentFile_Click(object sender, System.EventArgs e) {
            MenuItem item = sender as MenuItem;
            if (item == null) {
                return;
            }

            if (HandlePendingChanges() == false) {
                return;
            }

            CloseWxsFile();

            string[] recentFiles = WixEditSettings.Instance.GetRecentlyUsedFiles();
            if (File.Exists(recentFiles[item.Index])) {
                LoadWxsFile(recentFiles[item.Index]);
            } else {
                MessageBox.Show("File could not be found."); 
            }
        }

        private void editUndo_Click(object sender, System.EventArgs e) {
            XmlNode node = wixFiles.UndoManager.Undo();

            ShowNode(node, true);
        }

        private void editRedo_Click(object sender, System.EventArgs e) {
            XmlNode node = wixFiles.UndoManager.Redo();

            ShowNode(node, true);
        }

        private void editFind_Click(object sender, System.EventArgs e) {
            EnterStringForm frm = new EnterStringForm();
            frm.Text = "Find what text:";
            if (DialogResult.OK == frm.ShowDialog()) {
                string search = frm.SelectedString;
                ShowSearchPanel(null, null);
                searchPanel.Search(wixFiles, search);
            }
        }

        private void toolsExternal_Click(object sender, System.EventArgs e) {
            if (wixFiles == null ||
                WixEditSettings.Instance.ExternalXmlEditor == null ||
                File.Exists(WixEditSettings.Instance.ExternalXmlEditor) == false) {
                return;
            }

            ProcessStartInfo psiExternal = new ProcessStartInfo();
            psiExternal.FileName = WixEditSettings.Instance.ExternalXmlEditor;
            psiExternal.Arguments = String.Format("\"{0}\"", wixFiles.WxsFile.FullName);

            Process.Start(psiExternal);
        }

        public void ShowNode(XmlNode node) {
            ShowNode(node, false);
        }

        private void ShowNode(XmlNode node, bool forceReload) {
            if (node != null) {
                foreach (DisplayBasePanel panel in panels) {
                    if (node.Name == "Product") {
                        panel.ReloadData();
                    } else if (panel.IsOwnerOfNode(node)) {
                        tabButtonControl.SelectedPanel = panel;
                        panel.ShowNode(node);
                        if (forceReload) {
                            panel.ReloadData();
                        }
                        break;
                    }
                }
            }
        }

        private void buildWixCompile_Click(object sender, System.EventArgs e) {
            try {
                if (HandlePendingChanges("You need to save all changes before you can compile.")) {
                    Compile();
                }
            } catch (Exception ex) {
                MessageBox.Show(ex.Message, "Failed to compile", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
       
        private void buildWixInstall_Click(object sender, System.EventArgs e) {
            try {
                if (wixFiles != null) {
                    if (wixFiles.OutputFile.Exists) {
                        MessageBox.Show("Install package doesn't exist. Compile the package first.", "Need to compile", MessageBoxButtons.OK, MessageBoxIcon.Information);
    
                        return;
                    }
    
                    if (wixFiles.HasChanges() == true) {
                        if (DialogResult.Cancel == MessageBox.Show("In memory changes to \""+ wixFiles.WxsFile.Name +"\" will be discared with this install.", "Discard changes", MessageBoxButtons.OKCancel, MessageBoxIcon.Information)) {
                            return;
                        }
                    }
    
                    if (wixFiles.WxsFile.LastWriteTime.CompareTo(wixFiles.OutputFile.LastWriteTime) >= 0) {
                        DialogResult outOfDate = MessageBox.Show("The MSI file is out of date, continue?", "Discard changes", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Information);
                        if (outOfDate == DialogResult.Cancel || outOfDate == DialogResult.No) {
                            return;
                        }
                    }
    
                    Install(wixFiles.OutputFile.FullName);
                }
            } catch (Exception ex) {
                MessageBox.Show(ex.Message, "Failed to compile", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void buildWixUninstall_Click(object sender, System.EventArgs e) {
            try {
                if (wixFiles != null) {
                    if (wixFiles.OutputFile.Exists) {
                        MessageBox.Show("Install package doesn't exist. Compile and install the package first.", "Need to compile", MessageBoxButtons.OK, MessageBoxIcon.Information);
    
                        return;
                    }
    
                    Uninstall(wixFiles.OutputFile.FullName);
                }
            } catch (Exception ex) {
                MessageBox.Show(ex.Message, "Failed to compile", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        
        private void buildProjectSettings_Click(object sender, EventArgs e) {
            ProjectSettingsForm frm = new ProjectSettingsForm(wixFiles);
            frm.ShowDialog();
        }

        
        private void Install(string packagePath) {
            //msiexec /i Product.msi /l*v! Product.log
            string msiexec = "msiexec.exe";
            string logFile = Path.ChangeExtension(packagePath, "log");

            ProcessStartInfo psiInstall = new ProcessStartInfo();            
            psiInstall.FileName = msiexec;
            psiInstall.WorkingDirectory = wixFiles.WxsFile.Directory.FullName;
            psiInstall.Arguments = String.Format("/i \"{0}\" /l*v! \"{1}\"", packagePath, logFile);
            psiInstall.CreateNoWindow = true;
            psiInstall.UseShellExecute = false;
            psiInstall.RedirectStandardOutput = false;
            psiInstall.RedirectStandardError = false;

            ShowOutputPanel(null, null);
            outputPanel.Clear();
            Update();

            if (File.Exists(logFile)) {
                File.Delete(logFile);
            }

            outputPanel.RunWithLogFile(psiInstall, logFile);
        }

        private void Uninstall(string packagePath) {
            //msiexec /x Product.msi /l*v! Product.log
            string msiexec = "msiexec.exe";
            string logFile = Path.ChangeExtension(packagePath, "log");

            ProcessStartInfo psUninstall = new ProcessStartInfo();
            psUninstall.FileName = msiexec;
            psUninstall.WorkingDirectory = wixFiles.WxsFile.Directory.FullName;
            psUninstall.Arguments = String.Format("/x \"{0}\" /l*v! \"{1}\"", packagePath, logFile);
            psUninstall.CreateNoWindow = true;
            psUninstall.UseShellExecute = false;
            psUninstall.RedirectStandardOutput = false;
            psUninstall.RedirectStandardError = false;

            ShowOutputPanel(null, null);
            outputPanel.Clear();
            Update();

            if (File.Exists(logFile)) {
                File.Delete(logFile);
            }

            outputPanel.RunWithLogFile(psUninstall, logFile);
        }

        private void Compile() {
            string candleExe = WixEditSettings.Instance.WixBinariesDirectory.Candle;
            if (File.Exists(candleExe) == false) {
                throw new Exception("The executable \"candle.exe\" could not be found.\r\n\r\nPlease specify the correct path to the Wix binaries in the settings dialog.");
            }

            ProcessStartInfo psiCandle = new ProcessStartInfo();
            psiCandle.FileName = candleExe;
            psiCandle.WorkingDirectory = wixFiles.WxsFile.Directory.FullName;
            psiCandle.CreateNoWindow = true;
            psiCandle.UseShellExecute = false;
            psiCandle.RedirectStandardOutput = true;
            psiCandle.RedirectStandardError = false;
            psiCandle.Arguments = wixFiles.GetCandleArguments();

            string lightExe = WixEditSettings.Instance.WixBinariesDirectory.Light;
            if (File.Exists(lightExe) == false) {
                throw new Exception("The executable \"light.exe\" could not be found.\r\n\r\nPlease specify the correct path to the Wix binaries in the settings dialog.");
            }

            ProcessStartInfo psiLight = new ProcessStartInfo();
            psiLight.FileName = lightExe;
            psiLight.WorkingDirectory = wixFiles.WxsFile.Directory.FullName;
            psiLight.CreateNoWindow = true;
            psiLight.UseShellExecute = false;
            psiLight.RedirectStandardOutput = true;
            psiLight.RedirectStandardError = false;
            psiLight.Arguments = wixFiles.GetLightArguments();

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

        private void ShowResultsPanel(object sender, System.EventArgs e) {
            resultsSplitter.Visible = true;
            resultsPanel.Visible = true;
        }

        private void ShowOutputPanel(object sender, System.EventArgs e) {
            resultsSplitter.Visible = true;
            resultsPanel.Visible = true;

            resultsPanel.ShowPanel(outputPanel);
        }

        private void ShowSearchPanel(object sender, System.EventArgs e) {
            resultsSplitter.Visible = true;
            resultsPanel.Visible = true;

            resultsPanel.ShowPanel(searchPanel);
        }

        private void HideResultsPanel(object sender, System.EventArgs e) {
            resultsSplitter.Visible = false;
            resultsPanel.Visible = false;
        }

        protected void ShowProductProperties() {
            XmlNode product = wixFiles.WxsDocument.SelectSingleNode("/wix:Wix/*", wixFiles.WxsNsmgr);
            ProductPropertiesForm frm = new ProductPropertiesForm(product, wixFiles);
            frm.ShowDialog();

            editGlobalDataPanel.ReloadData();
        }

        private void toolsOptions_Click(object sender, System.EventArgs e) {
            SettingsForm frm = new SettingsForm();
            frm.ShowDialog();
        }

        private void helpTutorial_Click(object sender, System.EventArgs e) {
            string target = "http://www.tramontana.co.hu/wix/";

            // Navigate to it.
            try {
                Process.Start(target);
            } catch (Win32Exception) {
                // Workaround for:
                // "Win32Exception: The requested lookup key was not found in any active activation context"   
                Process process = new Process();
                process.StartInfo.FileName = "cmd.exe"; // Win2K+
                process.StartInfo.Arguments = "/c start " + target;
                process.StartInfo.CreateNoWindow = true;
                process.StartInfo.UseShellExecute = false;
                process.Start();
            }
        }

        private void helpMSIReference_Click(object sender, System.EventArgs e) {
            string target = "http://msdn.microsoft.com/library/en-us/msi/setup/windows_installer_reference.asp";

            // Navigate to it.
            try {
                Process.Start(target);
            } catch (Win32Exception) {
                // Workaround for:
                // "Win32Exception: The requested lookup key was not found in any active activation context"   
                Process process = new Process();
                process.StartInfo.FileName = "cmd.exe"; // Win2K+
                process.StartInfo.Arguments = "/c start " + target;
                process.StartInfo.CreateNoWindow = true;
                process.StartInfo.UseShellExecute = false;
                process.Start();
            }
        }

        private void helpAbout_Click(object sender, System.EventArgs e) {
            ShowSplash();
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
            return HandlePendingChanges(null);
        }
        private bool HandlePendingChanges(string message) {
            if (wixFiles != null && wixFiles.HasChanges() == true) {
                string messageText = "";
                if (message != null) {
                    messageText = message + "\r\n\r\n";
                }

                DialogResult result = MessageBox.Show(messageText + "Save the changes you made to \""+ wixFiles.WxsFile.Name +"\"?", "Save changes?", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
                if (result == DialogResult.Yes) {
                    wixFiles.Save();
                } else if (result == DialogResult.Cancel) {
                    return false;
                }   
            }

            return true;
        }

        private void LoadWxsFile(string filename) {
            LoadWxsFile(new FileInfo(filename));
        }

        private void LoadWxsFile(FileInfo file) {
            if (file.Exists == false) {
                MessageBox.Show(String.Format("File does not exist. ({0}))", file.Name), "File not found", MessageBoxButtons.OK, MessageBoxIcon.Exclamation); 
                return;
            }

            try {
                wixFiles = new WixFiles(file);
            } catch (UnauthorizedAccessException) {
                MessageBox.Show(String.Format("Access is denied. ({0}))", file.Name), "Acces denied", MessageBoxButtons.OK, MessageBoxIcon.Exclamation); 
            } catch {
                MessageBox.Show(String.Format("Failed to open file. ({0}))", file.Name), "Open failed", MessageBoxButtons.OK, MessageBoxIcon.Exclamation); 
            }

            wixFiles.wxsChanged += new EventHandler(wixFiles_wxsChanged);

            WixEditSettings.Instance.AddRecentlyUsedFile(file);
            WixEditSettings.Instance.SaveChanges();

            tabButtonControl = new TabButtonControl();
            tabButtonControl.Dock = DockStyle.Fill;

            mainPanel.Controls.Add(tabButtonControl);
            tabButtonControl.Visible = false;

            tabButtonControl.TabChange += new EventHandler(OnTabChanged) ;

            tabButtonControl.Visible = true;

            // Add Global tab
            editGlobalDataPanel = new EditGlobalDataPanel(wixFiles);
            editGlobalDataPanel.Dock = DockStyle.Fill;

            tabButtonControl.AddTab("Global", editGlobalDataPanel, new Bitmap(WixFiles.GetResourceStream("tabbuttons.global.png")));

            panels[0] = editGlobalDataPanel;

            oldTabIndex = 0;


            // Add Files tab
            editInstallDataPanel = new EditInstallDataPanel(wixFiles);
            editInstallDataPanel.Dock = DockStyle.Fill;

            tabButtonControl.AddTab("Files", editInstallDataPanel, new Bitmap(WixFiles.GetResourceStream("tabbuttons.files.png")));

            panels[1] = editInstallDataPanel;

            if (editInstallDataPanel.Menu != null) {
                mainMenu.MenuItems.Add(2, editInstallDataPanel.Menu);
            }

            // Add properties tab
            editPropertiesPanel = new EditPropertiesPanel(wixFiles);
            editPropertiesPanel.Dock = DockStyle.Fill;

            tabButtonControl.AddTab("Properties", editPropertiesPanel, new Bitmap(WixFiles.GetResourceStream("tabbuttons.properties.png")));

            panels[2] = editPropertiesPanel;


            // Add dialog tab
            editUIPanel = new EditUIPanel(wixFiles);
            editUIPanel.Dock = DockStyle.Fill;

            tabButtonControl.AddTab("Dialogs", editUIPanel, new Bitmap(WixFiles.GetResourceStream("tabbuttons.dialogs.png")));

            panels[3] = editUIPanel;


            // Add Resources tab
            editResourcesPanel = new EditResourcesPanel(wixFiles);
            editResourcesPanel.Dock = DockStyle.Fill;

            tabButtonControl.AddTab("Resources", editResourcesPanel, new Bitmap(WixFiles.GetResourceStream("tabbuttons.resources.png")));

            panels[4] = editResourcesPanel;


            // Add Resources tab
            editActionsPanel = new EditActionsPanel(wixFiles);
            editActionsPanel.Dock = DockStyle.Fill;

            tabButtonControl.AddTab("Actions", editActionsPanel, new Bitmap(WixFiles.GetResourceStream("tabbuttons.actions.png")));

            panels[5] = editActionsPanel;


            // Update menu
            fileClose.Enabled = true;
            if (WixEditSettings.Instance.DisplayFullPathInTitlebar) {
                Text = "WiX Edit - " + wixFiles.WxsFile.FullName;
            } else {
                Text = "WiX Edit - " + wixFiles.WxsFile.Name;
            }

            fileSave.Enabled = true;
            fileSaveAs.Enabled = true;

            buildWixCompile.Enabled = true;
            buildWixInstall.Enabled = true;
            buildWixUninstall.Enabled = true;
            buildProjectSettings.Enabled = true;
        }

        private void CloseWxsFile() {
            if (oldTabIndex >= 0 && oldTabIndex < panels.Length && panels[oldTabIndex].Menu != null) {
                mainMenu.MenuItems.RemoveAt(2);
            }

            buildWixCompile.Enabled = false;
            buildWixInstall.Enabled = false;
            buildWixUninstall.Enabled = false;
            buildProjectSettings.Enabled = false;
            
            if (tabButtonControl != null) {
                mainPanel.Controls.Remove(tabButtonControl);
                tabButtonControl.Visible = false;
                tabButtonControl = null;
            }

            oldTabIndex = -1;

            panels = new BasePanel[panelCount];

            if (editUIPanel != null) {
                editUIPanel.Visible = false;
                editUIPanel = null;
            }
            if (editPropertiesPanel != null) {
                editPropertiesPanel.Visible = false;
                editPropertiesPanel = null;
            }
            if (editResourcesPanel != null) {
                editResourcesPanel.Visible = false;
                editResourcesPanel = null;
            }
            if (editInstallDataPanel != null) {
                editInstallDataPanel.Visible = false;
                editInstallDataPanel = null;
            }
            if (editGlobalDataPanel != null) {
                editGlobalDataPanel.Visible = false;
                editGlobalDataPanel = null;
            }
            if (editActionsPanel != null) {
                editActionsPanel.Visible = false;
                editActionsPanel = null;
            }

            if (wixFiles != null) {
                wixFiles = null;
            }

            fileClose.Enabled = false;
            Text = "WiX Edit";

            fileSave.Enabled = false;
            fileSaveAs.Enabled = false;

            searchPanel.Clear();
        }

        [DllImport("User32")] 
        private static extern bool SetForegroundWindow(IntPtr hWnd);
        [DllImport("User32")]
        private static extern bool ShowWindowAsync(IntPtr hWnd, int nCmdShow);
        [DllImport("User32")] 
        private static extern bool IsIconic(IntPtr hWnd);

        private const int SW_RESTORE = 9;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main() {
            string fileToOpen = null;
            string[] args = Environment.GetCommandLineArgs();
            if (args.Length == 2) {
                string xmlFile = args[1];
                if (xmlFile != null && xmlFile.Length > 0 && File.Exists(xmlFile)) {
                    fileToOpen = xmlFile;
                }
            }

            Process otherProcess = FindOtherProcess();
            if (otherProcess != null) {
                IntPtr hWnd = otherProcess.MainWindowHandle;

                if (fileToOpen == null) {
                    if (IsIconic(hWnd)) {
                        ShowWindowAsync(hWnd, SW_RESTORE);
                    }
    
                    SetForegroundWindow(hWnd);
                } else {
                    CopyDataMessenger.SendMessage(hWnd, "open|"+fileToOpen);
                }

                return;
            }

            Application.EnableVisualStyles();
            Application.DoEvents();

            Application.ThreadException += new System.Threading.ThreadExceptionEventHandler(Application_ThreadException);
            try {
                EditorForm editorForm = null;
                if (fileToOpen == null) {
                    editorForm = new EditorForm();
                } else {
                    editorForm = new EditorForm(fileToOpen);
                }

                Application.Run(editorForm);
            } catch (Exception ex) {
                string message = "Caught unhandled exception! Please press OK to report this error to the WixEdit website, so this error can be fixed.";
                ExceptionForm form = new ExceptionForm(message, ex);
                if (form.ShowDialog() == DialogResult.OK) {
                    ErrorReporter reporter = new ErrorReporter();
                    reporter.Report(ex);
                }
            }
        }

        private static Process FindOtherProcess() {
            Process otherProcess = null;
            Process thisProcess = Process.GetCurrentProcess();
            
            string processName = thisProcess.ProcessName;
            Process[] processes = Process.GetProcessesByName(processName);
            
            if (processes.Length > 1) {
                for (int i = 0; i < processes.Length; i++) {
                    Process aProcess = processes[i];
                    if (aProcess != null && aProcess.MainWindowHandle != IntPtr.Zero) {
                        otherProcess = aProcess;
                    }
                }

                return otherProcess;
            }

            return null;
        }

        protected override void WndProc(ref Message message) {
            //filter the WM_COPYDATA
            if (message.Msg == CopyDataMessenger.WM_COPYDATA) {
                string messageText = CopyDataMessenger.DecodeMessage(ref message);
                
                string[] messageItems = messageText.Split('|');
                DoAction(messageItems[0], messageItems[1]);
            } else {
                //be sure to pass along all messages to the base also
                base.WndProc(ref message);
            }
        }

        private void DoAction(string action, string argument) {
            switch (action) {
                case "open":
                    EditorForm foundForm = null;
                    foreach (EditorForm edit in formInstances) {
                        // Hmmm, how can you compare 2 paths?!
                        if (edit.wixFiles != null && edit.wixFiles.WxsFile.FullName.ToLower() == new FileInfo(argument).FullName.ToLower()) {
                            foundForm = edit;
                        }
                    }

                    if (foundForm != null) {
                        if (IsIconic(foundForm.Handle)) {
                            ShowWindowAsync(foundForm.Handle, SW_RESTORE);
                        }
                        SetForegroundWindow(foundForm.Handle);
                    } else {
                        if (wixFiles == null) {
                            LoadWxsFile(argument);
                        } else {
                            EditorForm newForm = new EditorForm(argument);
    
                            Thread t = new Thread(new ThreadStart(newForm.StartMessageLoop));
                            t.Start();
                        }
                    }
                    break;
            }
        }

        protected void StartMessageLoop() {
            Application.Run(this);
        }

        private delegate void InvokeDelegate();
        private void wixFiles_wxsChanged(object sender, EventArgs e) {
            try {
                foreach (DisplayBasePanel panel in panels) {
                    panel.BeginInvoke(new InvokeDelegate(panel.ReloadData));
                }
            } catch (Exception ex) {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private static void Application_ThreadException(object sender, System.Threading.ThreadExceptionEventArgs e) {
            string message = "Unable to perform your action, an error occured! Please press OK to report this error to the WixEdit website, so this error can be fixed.";
            ExceptionForm form = new ExceptionForm(message, e.Exception);
            if (form.ShowDialog() == DialogResult.OK) {
                ErrorReporter reporter = new ErrorReporter();
                reporter.Report(e.Exception);
            }
        }
    }
}