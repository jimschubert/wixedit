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
using System.Drawing;
using System.IO;
using System.Text;
using System.Xml;
using System.Windows.Forms;

using WixEdit.Import;

namespace WixEdit {
    /// <summary>
    /// Panel for adding and removing files and other installable items.
    /// </summary>
    public class EditFilesPanel : DetailsBasePanel {        
        protected ContextMenu globalTreeViewContextMenu;
        protected IconMenuItem importFilesMenu;
        protected IconMenuItem importFolderMenu;

        public EditFilesPanel(WixFiles wixFiles) : base(wixFiles) {
            globalTreeViewContextMenu = new ContextMenu();
            globalTreeViewContextMenu.Popup += new EventHandler(PopupGlobalTreeViewContextMenu);

            treeView.DragEnter += new DragEventHandler(treeView_DragEnter);
            treeView.DragLeave += new EventHandler(treeView_DragLeave);
            treeView.DragOver += new DragEventHandler(treeView_DragOver);
            treeView.DragDrop += new DragEventHandler(treeView_DragDrop);

            treeView.AllowDrop = true;

            importFilesMenu = new IconMenuItem("&Import Files", new Bitmap(WixFiles.GetResourceStream("bmp.import.bmp")));
            importFilesMenu.Click += new EventHandler(ImportFiles_Click);

            importFolderMenu = new IconMenuItem("&Import Folder", new Bitmap(WixFiles.GetResourceStream("bmp.import.bmp")));
            importFolderMenu.Click += new EventHandler(ImportFolder_Click);
        }

        protected override ArrayList GetXmlNodes() {
            ArrayList nodes = new ArrayList();
            XmlNodeList xmlNodes = wixFiles.WxsDocument.SelectNodes("/wix:Wix/*/wix:Directory", wixFiles.WxsNsmgr);
            foreach (XmlNode xmlNode in xmlNodes) {
                nodes.Add(xmlNode);
            }

            xmlNodes = wixFiles.WxsDocument.SelectNodes("/wix:Wix/*/wix:DirectoryRef", wixFiles.WxsNsmgr);
            foreach (XmlNode xmlNode in xmlNodes) {
                nodes.Add(xmlNode);
            }

            return nodes;
        }

        protected override void OnGlobalTreeViewContextMenu(object sender, System.Windows.Forms.MouseEventArgs e) {
            Point spot = PointToClient(treeView.PointToScreen(new Point(e.X,e.Y)));

            globalTreeViewContextMenu.Show(this, spot);
        }

        protected override void AddCustomTreeViewContextMenuItems(XmlNode node, ContextMenu treeViewContextMenu) {
            if (node.Name == "Component") {
                treeViewContextMenu.MenuItems.Add(1, importFilesMenu);
            } else if (node.Name == "Directory") {
                treeViewContextMenu.MenuItems.Add(1, importFolderMenu);
            }
        }

        protected void PopupGlobalTreeViewContextMenu(System.Object sender, System.EventArgs e) {
            globalTreeViewContextMenu.MenuItems.Clear();

            IconMenuItem subMenuItem1 = new IconMenuItem("New Directory", new Bitmap(WixFiles.GetResourceStream("bmp.new.bmp")));
            IconMenuItem subMenuItem2 = new IconMenuItem("New DirectoryRef", new Bitmap(WixFiles.GetResourceStream("bmp.new.bmp")));

            subMenuItem1.Click += new EventHandler(NewDirectoryElement_Click);
            subMenuItem2.Click += new EventHandler(NewDirectoryRefElement_Click);

            globalTreeViewContextMenu.MenuItems.Add(subMenuItem1);
            globalTreeViewContextMenu.MenuItems.Add(subMenuItem2);
        }

        private void NewDirectoryElement_Click(object sender, System.EventArgs e) {
            NewCustomElement("Directory");
        }

        private void NewDirectoryRefElement_Click(object sender, System.EventArgs e) {
            NewCustomElement("DirectoryRef");
        }

        private void NewCustomElement(string elementName) {
            wixFiles.UndoManager.BeginNewCommandRange();           

            XmlNode xmlNode = wixFiles.WxsDocument.SelectSingleNode("/wix:Wix/*", wixFiles.WxsNsmgr);

            XmlElement newElement = wixFiles.WxsDocument.CreateElement(elementName, WixFiles.WixNamespaceUri);
            TreeNode action = new TreeNode(elementName);
            action.Tag = newElement;

            int imageIndex = ImageListFactory.GetImageIndex(elementName);
            if (imageIndex >= 0) {
                action.ImageIndex = imageIndex;
                action.SelectedImageIndex = imageIndex;
            }

            XmlNodeList sameNodes = xmlNode.SelectNodes("wix:" + elementName, wixFiles.WxsNsmgr);
            if (sameNodes.Count > 0) {
                xmlNode.InsertAfter(newElement, sameNodes[sameNodes.Count - 1]);
            } else {
                xmlNode.AppendChild(newElement);
            }

            treeView.Nodes.Add(action);
            treeView.SelectedNode = action;

            ShowProperties(newElement); 
        }

        private void treeView_DragEnter(object sender, DragEventArgs e) {

        }

        private void treeView_DragLeave(object sender, EventArgs e) {
            if (oldNode != null) {
                TreeNode newNode = new TreeNode();
                oldNode.BackColor = newNode.BackColor;
                oldNode.ForeColor = newNode.ForeColor;
                oldNode = null;
            }
        }

        TreeNode oldNode = null;
        private void treeView_DragOver(object sender, DragEventArgs e) {
            TreeNode aNode = treeView.GetNodeAt(treeView.PointToClient(new Point(e.X, e.Y)));

            if (oldNode == aNode) {
                return;
            }

            if (e.Data.GetDataPresent(DataFormats.FileDrop)) {
                XmlNode aNodeElement = aNode.Tag as XmlNode;
                if (aNodeElement.Name == "Component") {
                    bool filesOnly = true;

                    string[] filesAndFolders = (string[])e.Data.GetData(DataFormats.FileDrop);
                    foreach (string fileOrFolder in filesAndFolders) {
                        if (File.Exists(fileOrFolder) == false) {
                            filesOnly = false;
                            break;
                        }
                    }

                    if (filesOnly) {
                        e.Effect = DragDropEffects.Copy;
                    } else {
                        e.Effect = DragDropEffects.None;
                    }
                } else if (aNodeElement.Name == "Directory") {
                    bool dirsOnly = true;

                    string[] filesAndFolders = (string[])e.Data.GetData(DataFormats.FileDrop);
                    foreach (string fileOrFolder in filesAndFolders) {
                        if (Directory.Exists(fileOrFolder) == false) {
                            dirsOnly = false;
                            break;
                        }
                    }

                    if (dirsOnly) {
                        e.Effect = DragDropEffects.Copy;
                    } else {
                        e.Effect = DragDropEffects.None;
                    }
                } else {
                    e.Effect = DragDropEffects.None;
                }
            }

            if (oldNode != null) {
                oldNode.BackColor = aNode.BackColor;
                oldNode.ForeColor = aNode.ForeColor;
            }

            aNode.BackColor = Color.DarkBlue;
            aNode.ForeColor = Color.White;

            oldNode = aNode;
        }

        private void treeView_DragDrop(object sender, DragEventArgs e) {
            TreeNode aNode = treeView.GetNodeAt(treeView.PointToClient(new Point(e.X, e.Y)));
            treeView.SelectedNode = aNode;

            XmlNode aNodeElement = aNode.Tag as XmlNode;

            if (oldNode != null) {
                oldNode.BackColor = aNode.BackColor;
                oldNode.ForeColor = aNode.ForeColor;
            }

            string[] filesOrFolders = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (aNodeElement.Name == "Component") {
                ImportFilesInComponent(aNode, aNodeElement, filesOrFolders);
            } else if (aNodeElement.Name == "Directory") {
                ImportFoldersInDirectory(aNode, aNodeElement, filesOrFolders);
            }
        }

        private void ImportFiles_Click(object sender, System.EventArgs e) {
            TreeNode aNode = treeView.SelectedNode;
            XmlNode aNodeElement = aNode.Tag as XmlNode;

            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "All files (*.*)|*.*|Registration Files (*.reg)|*.REG" ;
            ofd.RestoreDirectory = true ;
            ofd.Multiselect = true;
            
            if (ofd.ShowDialog() == DialogResult.OK) {
                string[] files = ofd.FileNames;
    
                ImportFilesInComponent(aNode, aNodeElement, files);
            }
        }

        private void ImportFolder_Click(object sender, System.EventArgs e) {
            TreeNode aNode = treeView.SelectedNode;
            XmlNode aNodeElement = aNode.Tag as XmlNode;

            FolderBrowserDialog  ofd = new FolderBrowserDialog ();
            ofd.Description = "Select folder to import";
            ofd.ShowNewFolderButton = false;
            if (ofd.ShowDialog() == DialogResult.OK) {
                ImportFoldersInDirectory(aNode, aNodeElement, new string[] { ofd.SelectedPath });
            }
        }
        
        private void ImportFoldersInDirectory(TreeNode node, XmlNode directoryNode, string[] folders) {
            if (directoryNode.Name == "Directory") {
                bool mustExpand = (node.Nodes.Count == 0);

                treeView.SuspendLayout();

                wixFiles.UndoManager.BeginNewCommandRange();
                try {
                    DirectoryImport dirImport = new DirectoryImport(wixFiles, folders, directoryNode);
                    dirImport.Import(node);
                } catch (ImportException ex) {
                    MessageBox.Show(String.Format("Failed to complete import: {0}\r\n\r\nThe import is aborted and could be partially completed.", ex.Message), "Import Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                } catch (Exception ex) {
                    string message = "An exception occured during the import! Please press OK to report this error to the WixEdit website, so this error can be fixed.";
                    ExceptionForm form = new ExceptionForm(message, ex);
                    if (form.ShowDialog() == DialogResult.OK) {
                        ErrorReporter reporter = new ErrorReporter();
                        reporter.Report(ex);
                    }
                }

                treeView.ResumeLayout();
            }
        }

        private void ImportFilesInComponent(TreeNode node, XmlNode componentNode, string[] files) {
            if (componentNode.Name == "Component") {
                bool mustExpand = (node.Nodes.Count == 0);

                treeView.SuspendLayout();

                bool foundReg = false;
                foreach (string file in files) {
                    if (Path.GetExtension(file).ToLower() == ".reg") {
                        foundReg = true;
                        break;
                    }
                }

                bool importRegistryFiles = false;
                if (foundReg == true) {
                    DialogResult result = MessageBox.Show(this, "Import Registry (*.reg) files to Registry elements?", "Import?", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
                    if (result == DialogResult.Cancel) {
                        return;
                    } else if (result == DialogResult.Yes) {
                        importRegistryFiles = true;
                    }
                }

                wixFiles.UndoManager.BeginNewCommandRange();
                StringBuilder errorMessageBuilder = new StringBuilder();
                
                foreach (string file in files) {
                    FileInfo fileInfo = new FileInfo(file);
                    try {
                        if (fileInfo.Extension.ToLower() == ".reg" && importRegistryFiles) {
                            RegistryImport regImport = new RegistryImport(wixFiles, fileInfo, componentNode);
                            regImport.Import(node);
                        } else {
                            FileImport fileImport = new FileImport(wixFiles, fileInfo, componentNode);
                            fileImport.Import(node);
                        }
                    } catch (ImportException ex) {
                        errorMessageBuilder.AppendFormat("{0} ({1})\r\n", fileInfo.Name, ex.Message);
                    } catch (Exception ex) {
                        string message = String.Format("An exception occured during the import of \"{0}\"! Please press OK to report this error to the WixEdit website, so this error can be fixed.", fileInfo.Name);
                        ExceptionForm form = new ExceptionForm(message, ex);
                        if (form.ShowDialog() == DialogResult.OK) {
                            ErrorReporter reporter = new ErrorReporter();
                            reporter.Report(ex);
                        }
                    }
                }

                if (errorMessageBuilder.Length > 0) {
                    MessageBox.Show(this, "Import failed for the following files:\r\n\r\n" + errorMessageBuilder.ToString(), "Import failed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }

                ShowNode(componentNode);

                if (mustExpand) {
                    node.Expand();
                }

                treeView.ResumeLayout();
            }
        }
    }
}