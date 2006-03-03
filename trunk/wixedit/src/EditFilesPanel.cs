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
using System.Xml;
using System.Windows.Forms;


namespace WixEdit {
    /// <summary>
    /// Panel for adding and removing files and other installable items.
    /// </summary>
    public class EditFilesPanel : DetailsBasePanel {        
        protected ContextMenu globalTreeViewContextMenu;

        public EditFilesPanel(WixFiles wixFiles) : base(wixFiles) {
            globalTreeViewContextMenu = new ContextMenu();
            globalTreeViewContextMenu.Popup += new EventHandler(PopupGlobalTreeViewContextMenu);

            treeView.DragEnter += new DragEventHandler(treeView_DragEnter);
            treeView.DragLeave += new EventHandler(treeView_DragLeave);
            treeView.DragOver += new DragEventHandler(treeView_DragOver);
            treeView.DragDrop += new DragEventHandler(treeView_DragDrop);

            treeView.AllowDrop = true;
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

            XmlElement newElement = wixFiles.WxsDocument.CreateElement(elementName, "http://schemas.microsoft.com/wix/2003/01/wi");
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

        }

        TreeNode oldNode = null;
        private void treeView_DragOver(object sender, DragEventArgs e) {
            TreeNode aNode = treeView.GetNodeAt(treeView.PointToClient(new Point(e.X, e.Y)));

            if (oldNode == aNode) {
                return;
            }

            if (e.Data.GetDataPresent(DataFormats.FileDrop)) {
                if (ImageListFactory.GetImageIndex("Component") == aNode.ImageIndex) {
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
                // } else if (ImageListFactory.GetImageIndex("Directory") == aNode.ImageIndex) {
                //    e.Effect = DragDropEffects.Copy;
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

            wixFiles.UndoManager.BeginNewCommandRange();

            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (ImageListFactory.GetImageIndex("Component") == aNode.ImageIndex) {
                foreach (string file in files) {
                    FileInfo fileInfo = new FileInfo(file);

                    CreateFileElement(fileInfo, aNodeElement);
                }
            }
        }

        protected void CreateFileElement(FileInfo fileInfo, XmlNode aNodeElement) {
            TreeNode fileNode = CreateNewSubElement("File");
            XmlNode fileElement = fileNode.Tag as XmlNode;

            fileNode.Text = fileInfo.Name;

            if (fileInfo.Exists) {
                Icon ico = FileIconFactory.GetFileIcon(fileInfo.FullName);
                treeView.ImageList.Images.Add(ico);
    
                fileNode.ImageIndex = treeView.ImageList.Images.Count - 1;
                fileNode.SelectedImageIndex = treeView.ImageList.Images.Count - 1;
            }

            XmlAttribute idAttr = wixFiles.WxsDocument.CreateAttribute("Id");
            idAttr.Value = fileInfo.Name;
            fileElement.Attributes.Append(idAttr);

            XmlAttribute longNameAttr = wixFiles.WxsDocument.CreateAttribute("LongName");
            longNameAttr.Value = fileInfo.Name;
            fileElement.Attributes.Append(longNameAttr);
                    
            XmlAttribute nameAttr = wixFiles.WxsDocument.CreateAttribute("Name");

            nameAttr.Value = GetShortFileName(fileInfo, aNodeElement);
                    
            fileElement.Attributes.Append(nameAttr);

            XmlAttribute srcAttr = wixFiles.WxsDocument.CreateAttribute("Source");
            srcAttr.Value = RelativePathHelper.GetRelativePath(fileInfo.FullName, wixFiles);
            fileElement.Attributes.Append(srcAttr);
        }

        protected string GetShortFileName(FileInfo fileInfo, XmlNode aNodeElement) {
            string nameStart = Path.GetFileNameWithoutExtension(fileInfo.Name).ToUpper();
            int tooShort = 0;
            if (nameStart.Length > 7) {
                nameStart = nameStart.Substring(0, 7);
            } else {
                tooShort = 7 - nameStart.Length;
            }

            string nameExtension = fileInfo.Extension.ToUpper();
                    
            int i = 1;
            string shortFileName = String.Format("{0}{1}{2}", nameStart, i, nameExtension);

            while (aNodeElement.SelectSingleNode(String.Format("wix:File[@Name='{0}']", shortFileName), wixFiles.WxsNsmgr) != null) {
                if (i%10 == 9) {
                    if (tooShort > 0) {
                        tooShort--;
                    } else {
                        nameStart = nameStart.Substring(0, nameStart.Length - 1);
                    }
                }

                shortFileName = String.Format("{0}{1} {2}", nameStart, ++i, nameExtension);
            }

            return shortFileName;
        }
    }
}