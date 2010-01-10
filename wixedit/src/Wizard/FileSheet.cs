using System;
using System.Collections.Generic;
using System.Text;
using WixEdit.Wizard;
using System.Windows.Forms;
using System.Drawing;
using System.Xml;
using WixEdit.Import;
using WixEdit.Server;
using System.IO;

namespace WixEdit.Wizard
{
    class FileSheet : BaseSheet
    {
        Label titleLabel;
        Label descriptionLabel;
        Label lineLabel;
        TreeView tree;
        ContextMenu contextMenu;

        public FileSheet(WizardForm creator)
            : base(creator)
        {
            this.AutoScroll = true;

            titleLabel = new Label();
            titleLabel.Text = "Add Files and Directories";
            titleLabel.Dock = DockStyle.Top;
            titleLabel.Height = 15;
            titleLabel.Left = 0;
            titleLabel.Top = 0;
            titleLabel.Padding = new Padding(5, 0, 5, 0);
            titleLabel.Font = new Font("Verdana",
                        10,
                        FontStyle.Bold,
                        GraphicsUnit.Point
                    );
            titleLabel.BackColor = Color.White;

            descriptionLabel = new Label();
            descriptionLabel.Text = "Blablabla";
            descriptionLabel.Dock = DockStyle.Top;
            descriptionLabel.Height = 50 - titleLabel.Height;
            descriptionLabel.Left = 0;
            descriptionLabel.Top = titleLabel.Height;
            descriptionLabel.Padding = new Padding(8, 3, 5, 0);
            descriptionLabel.BackColor = Color.White;

            this.Controls.Add(descriptionLabel);

            this.Controls.Add(titleLabel);


            lineLabel = new Label();
            lineLabel.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            lineLabel.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            lineLabel.Location = new Point(0, titleLabel.Height + descriptionLabel.Height);
            lineLabel.Size = new Size(this.Width, 2);

            this.Controls.Add(lineLabel);

            tree = new TreeView();
            tree.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            tree.Location = new Point(4, titleLabel.Height + descriptionLabel.Height + lineLabel.Height + 4);
            tree.Width = this.Width - 8;
            tree.Height = this.Height - tree.Top - 4;
            tree.ImageList = ImageListFactory.GetImageList();
            tree.MouseDown += new MouseEventHandler(tree_MouseDown);

            this.Controls.Add(tree);

            contextMenu = new ContextMenu();
            contextMenu.Popup += new EventHandler(contextMenu_Popup);
            tree.ContextMenu = contextMenu;

            XmlDocument wxsDoc = Wizard.WixFiles.WxsDocument;
            XmlNamespaceManager wxsNsmgr = Wizard.WixFiles.WxsNsmgr;

            XmlNodeList dirNodes = wxsDoc.SelectNodes("/wix:Wix/*/wix:Directory", wxsNsmgr);
            TreeNodeCollection treeNodes = tree.Nodes;

            InitTreeView(dirNodes);
        }

        private void tree_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (this.Visible == false)
            {
                return;
            }
            if (e.Button == MouseButtons.Right)
            {
                TreeNode node = tree.GetNodeAt(e.X, e.Y);
                if (node == null)
                {
                    return;
                }
                tree.SelectedNode = node;

                Point spot = PointToClient(tree.PointToScreen(new Point(e.X, e.Y)));
                contextMenu.Show(this, spot);
            }
        }

        void contextMenu_Popup(object sender, EventArgs e)
        {
            contextMenu.MenuItems.Clear();
            
            XmlNode node = tree.SelectedNode.Tag as XmlNode;
            if (node == null)
            {
                return;
            }

            //add import menu
            //IconMenuItem itemImport = new IconMenuItem("&Import", new Bitmap(WixFiles.GetResourceStream("bmp.import.bmp")));
            //itemImport.Click += new System.EventHandler(ImportElement_Click);

            //contextMenu.MenuItems.Add(new IconMenuItem("-"));
            //contextMenu.MenuItems.Add(itemImport);

            if (node.Name == "Component")
            {
                IconMenuItem importFilesMenu = new IconMenuItem("&Import Files", new Bitmap(WixFiles.GetResourceStream("bmp.import.bmp")));
                importFilesMenu.Click += new System.EventHandler(ImportFiles_Click);
                contextMenu.MenuItems.Add(0, importFilesMenu);
            }
            else if (node.Name == "Directory")
            {
                IconMenuItem importFolderMenu = new IconMenuItem("&Import Folder", new Bitmap(WixFiles.GetResourceStream("bmp.import.bmp")));
                importFolderMenu.Click += new System.EventHandler(ImportFolder_Click);
                contextMenu.MenuItems.Add(0, importFolderMenu);
            }
        }
        private void InitTreeView(XmlNodeList dirNodes)
        {
            this.SuspendLayout();

            tree.Nodes.Clear();

            foreach (XmlNode file in dirNodes)
            {
                if (file.NodeType == XmlNodeType.ProcessingInstruction)
                {
                    continue;
                }

                AddTreeNodesRecursive(file, tree.Nodes);
            }

            tree.ExpandAll();

            if (tree.Nodes.Count > 0)
            {
                tree.SelectedNode = tree.Nodes[0];
            }

            this.ResumeLayout();
        }


        protected void AddTreeNodesRecursive(XmlNode file, TreeNodeCollection nodes)
        {
            if (file.Name.StartsWith("#"))
            {
                return;
            }

            TreeNode node = new TreeNode(GetDisplayName(file));
            node.Tag = file;

            int imageIndex = -1;
            if (file.Name == "File" && file.Attributes["Source"] != null)
            {
                string filePath = Path.Combine(Wizard.WixFiles.WxsDirectory.FullName, file.Attributes["Source"].Value);
                if (File.Exists(filePath))
                {
                    string key = Path.GetExtension(filePath).ToUpper();
                    imageIndex = tree.ImageList.Images.IndexOfKey(key);
                    if (imageIndex < 0)
                    {
                        try
                        {
                            Icon ico = FileIconFactory.GetFileIcon(filePath);
                            if (ico != null)
                            {
                                tree.ImageList.Images.Add(key, ico);
                                imageIndex = tree.ImageList.Images.Count - 1;
                            }
                        } // if icon retrieved from icon factory
                        catch { }
                    } // if image not already in tree image list
                } // if file exists
            } // node is a file and Source attribute is not null
            if (imageIndex < 0)
            {
                imageIndex = ImageListFactory.GetImageIndex(file.Name);
            }
            if (imageIndex >= 0)
            {
                node.ImageIndex = imageIndex;
                node.SelectedImageIndex = imageIndex;
            }

            nodes.Add(node);

            if (file.ChildNodes.Count > 10000)
            {
                TreeNode tooManyNodes = new TreeNode("<< Too many children to display >>");
                node.ImageIndex = ImageListFactory.GetUnsupportedImageIndex();
                node.SelectedImageIndex = ImageListFactory.GetUnsupportedImageIndex();
                node.Nodes.Add(tooManyNodes);

                return;
            }

            foreach (XmlNode child in file.ChildNodes)
            {
                if (child.NodeType == XmlNodeType.ProcessingInstruction)
                {
                    continue;
                }

                AddTreeNodesRecursive(child, node.Nodes);
            }
        }


        protected string GetDisplayName(XmlNode element)
        {
            string displayName = null;
            try
            {
                switch (element.Name)
                {
                    case "Directory":
                    case "DirectoryRef":
                    case "File":
                        XmlAttribute nameAtt = element.Attributes["LongName"];
                        if (nameAtt == null)
                        {
                            nameAtt = element.Attributes["Name"];
                        }
                        if (nameAtt == null)
                        {
                            nameAtt = element.Attributes["Id"];
                        }
                        displayName = nameAtt.Value;
                        break;
                    case "Registry":
                        string root = element.Attributes["Root"].Value;
                        string key = element.Attributes["Key"].Value;
                        XmlAttribute name = element.Attributes["Name"];
                        if (name != null)
                        {
                            if (key.EndsWith("\\") == false)
                            {
                                key = key + "\\";
                            }
                            key = key + name.Value;
                        }

                        displayName = root + "\\" + key;
                        break;
                    case "RegistryValue":
                        if (element.Attributes["Root"] == null ||
                            element.Attributes["Key"] == null)
                        {
                            if (element.Attributes["Name"] != null)
                            {
                                displayName = element.Attributes["Name"].Value;
                            }
                            else
                            {
                                displayName = element.Name;
                            }
                        }
                        else
                        {
                            string valueRoot = element.Attributes["Root"].Value;
                            string valueKey = element.Attributes["Key"].Value;

                            displayName = valueRoot + "\\" + valueKey;
                            if (element.Attributes["Name"] != null)
                            {
                                displayName = valueRoot + "\\" + valueKey + "\\" + element.Attributes["Name"].Value;
                            }
                        }
                        break;
                    case "RegistryKey":
                        if (element.Attributes["Root"] == null ||
                            element.Attributes["Key"] == null)
                        {
                            displayName = element.Name;
                        }
                        else
                        {
                            string keyRoot = element.Attributes["Root"].Value;
                            string keyKey = element.Attributes["Key"].Value;

                            displayName = keyRoot + "\\" + keyKey;
                        }
                        break;
                    case "Component":
                    case "CustomAction":
                    case "Feature":
                    case "ComponentRef":
                        XmlAttribute idAtt = element.Attributes["Id"];
                        if (idAtt != null)
                        {
                            displayName = idAtt.Value;
                        }
                        else
                        {
                            displayName = element.Name;
                        }
                        break;
                    case "Show":
                        XmlAttribute dlgAtt = element.Attributes["Dialog"];
                        if (dlgAtt != null)
                        {
                            displayName = dlgAtt.Value;
                        }
                        else
                        {
                            displayName = element.Name;
                        }
                        break;
                    case "Custom":
                        XmlAttribute actionAtt = element.Attributes["Action"];
                        if (actionAtt != null)
                        {
                            displayName = actionAtt.Value;
                        }
                        else
                        {
                            displayName = element.Name;
                        }
                        break;
                    case "Condition":
                        string innerText = element.InnerText;
                        if (innerText != null && innerText.Length > 1)
                        {
                            displayName = String.Format("({0})", innerText);
                        }
                        else
                        {
                            displayName = element.Name;
                        }
                        break;
                    default:
                        displayName = element.Name;
                        break;
                }
            }
            catch
            {
                displayName = element.Name;
            }

            if (displayName == null || displayName == "")
            {
                displayName = element.Name;
            }

            return displayName;
        }

        private static void AddDirectoryTreeNodes(XmlNodeList dirNodes, TreeNodeCollection treeNodes)
        {
            foreach (XmlNode dirNode in dirNodes)
            {
                if (dirNode.Name != "Directory")
                {
                    continue;
                }

                XmlElement dirElement = (XmlElement)dirNode;

                TreeNode treeNode = new TreeNode();
                
                treeNode.Text = dirElement.GetAttribute("Name");
                treeNode.ImageIndex = ImageListFactory.GetImageIndex("Directory");
                
                treeNodes.Add(treeNode);
                treeNode.Expand();

                AddDirectoryTreeNodes(dirNode.ChildNodes, treeNode.Nodes);
            }
        }


        private void ImportFiles_Click(object sender, System.EventArgs e)
        {
            TreeNode aNode = tree.SelectedNode;
            XmlNode aNodeElement = aNode.Tag as XmlNode;

            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "All files (*.*)|*.*|Registration Files (*.reg)|*.REG";
            ofd.RestoreDirectory = true;
            ofd.Multiselect = true;

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                string[] files = ofd.FileNames;

                ImportFilesInComponent(aNode, aNodeElement, files);
            }
        }

        private void ImportFolder_Click(object sender, System.EventArgs e)
        {
            TreeNode aNode = tree.SelectedNode;
            XmlNode aNodeElement = aNode.Tag as XmlNode;

            FolderBrowserDialog ofd = new FolderBrowserDialog();
            ofd.Description = "Select folder to import";
            ofd.ShowNewFolderButton = false;
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                ImportFoldersInDirectory(aNode, aNodeElement, new string[] { ofd.SelectedPath });
            }
        }

        private void ImportFoldersInDirectory(TreeNode node, XmlNode directoryNode, string[] folders)
        {
            if (directoryNode.Name == "Directory")
            {
                bool mustExpand = (node.Nodes.Count == 0);

                tree.SuspendLayout();

                Wizard.WixFiles.UndoManager.BeginNewCommandRange();
                try
                {
                    DirectoryImport dirImport = new DirectoryImport(Wizard.WixFiles, folders, directoryNode);
                    dirImport.Import(node);
                }
                catch (WixEditException ex)
                {
                    MessageBox.Show(String.Format("Failed to complete import: {0}\r\n\r\nThe import is aborted and could be partially completed.", ex.Message), "Import Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                catch (Exception ex)
                {
                    ErrorReportHandler r = new ErrorReportHandler(ex, this.TopLevelControl, "An exception occured during the import! Please press OK to report this error to the WixEdit website, so this error can be fixed.");
                    r.ReportInSeparateThread();
                }

                tree.ResumeLayout();
            }
        }

        private void ImportFilesInComponent(TreeNode node, XmlNode componentNode, string[] files)
        {
            if (componentNode.Name == "Component")
            {
                bool mustExpand = (node.Nodes.Count == 0);

                tree.SuspendLayout();

                bool foundReg = false;
                foreach (string file in files)
                {
                    if (Path.GetExtension(file).ToLower() == ".reg")
                    {
                        foundReg = true;
                        break;
                    }
                }

                bool importRegistryFiles = false;
                if (foundReg == true)
                {
                    DialogResult result = MessageBox.Show(this, "Import Registry (*.reg) files to Registry elements?", "Import?", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
                    if (result == DialogResult.Cancel)
                    {
                        return;
                    }
                    else if (result == DialogResult.Yes)
                    {
                        importRegistryFiles = true;
                    }
                }

                Wizard.WixFiles.UndoManager.BeginNewCommandRange();
                StringBuilder errorMessageBuilder = new StringBuilder();

                foreach (string file in files)
                {
                    FileInfo fileInfo = new FileInfo(file);
                    try
                    {
                        if (fileInfo.Extension.ToLower() == ".reg" && importRegistryFiles)
                        {
                            RegistryImport regImport = new RegistryImport(Wizard.WixFiles, fileInfo, componentNode);
                            regImport.Import(node);
                        }
                        else
                        {
                            FileImport fileImport = new FileImport(Wizard.WixFiles, fileInfo, componentNode);
                            fileImport.Import(node);
                        }
                    }
                    catch (WixEditException ex)
                    {
                        errorMessageBuilder.AppendFormat("{0} ({1})\r\n", fileInfo.Name, ex.Message);
                    }
                    catch (Exception ex)
                    {
                        string message = String.Format("An exception occured during the import of \"{0}\"! Please press OK to report this error to the WixEdit website, so this error can be fixed.", fileInfo.Name);
                        ExceptionForm form = new ExceptionForm(message, ex);
                        if (form.ShowDialog() == DialogResult.OK)
                        {
                            ErrorReporter reporter = new ErrorReporter();
                            reporter.Report(ex);
                        }
                    }
                }

                if (errorMessageBuilder.Length > 0)
                {
                    MessageBox.Show(this, "Import failed for the following files:\r\n\r\n" + errorMessageBuilder.ToString(), "Import failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

                // ShowNode(componentNode);

                if (mustExpand)
                {
                    node.Expand();
                }

                tree.ResumeLayout();
            }
        }
    }
}
