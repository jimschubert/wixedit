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
using System.Drawing.Drawing2D;
using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Data;
using System.Xml;
using System.IO;
using System.Resources;
using System.Reflection;

using WixEdit.PropertyGridExtensions;

namespace WixEdit {
    /// <summary>
    /// Base panel to edit data with a treeview and a details section.
    /// </summary>
    public abstract class DetailsBasePanel : BasePanel {
        protected TreeView treeView;
        protected IconMenuItem newSubElementsMenu;
        protected IconMenuItem deleteCurrentElementMenu;
        protected IconMenuItem infoAboutCurrentElementMenu;

        protected ContextMenu treeViewContextMenu;
        protected Splitter splitter1;
        protected PropertyGrid propertyGrid;
        protected Panel panel1;
        protected ContextMenu propertyGridContextMenu;

        public DetailsBasePanel(WixFiles wixFiles) : base(wixFiles) {
            InitializeComponent();
        }

        #region Initialize Controls
        private void InitializeComponent() {
            treeView = new TreeView();
            splitter1 = new Splitter();
            propertyGrid = new CustomPropertyGrid();
            panel1 = new Panel();
            propertyGridContextMenu = new ContextMenu();


            // 
            // treeView
            // 
            treeView.Dock = DockStyle.Left;
            treeView.ImageIndex = -1;
            treeView.Location = new Point(0, 0);
            treeView.Name = "treeView";
            treeView.SelectedImageIndex = -1;
            treeView.Size = new Size(256, 266);
            treeView.TabIndex = 6;
            treeView.ImageList = ImageListFactory.GetImageList();

            treeView.AfterSelect += new TreeViewEventHandler(OnAfterSelect);
            treeViewContextMenu = new ContextMenu();
            treeViewContextMenu.Popup += new EventHandler(PopupTreeViewContextMenu);
            treeView.MouseDown += new MouseEventHandler(TreeViewMouseDown);

            newSubElementsMenu = new IconMenuItem("&New", new Bitmap(WixFiles.GetResourceStream("WixEdit.new.bmp")));
            deleteCurrentElementMenu = new IconMenuItem("&Delete", new Bitmap(WixFiles.GetResourceStream("WixEdit.delete.bmp")));
            deleteCurrentElementMenu.Click += new System.EventHandler(DeleteElement_Click);

            infoAboutCurrentElementMenu = new IconMenuItem("&Info", new Bitmap(WixFiles.GetResourceStream("WixEdit.info.bmp")));
            infoAboutCurrentElementMenu.Click += new System.EventHandler(InfoAboutCurrentElement_Click);

            GetXmlNodes();
            IList files = GetXmlNodes();
            foreach (XmlNode file in files) {
                AddTreeNodesRecursive(file, treeView.Nodes);
            }


            treeView.ExpandAll();
            if (treeView.Nodes.Count > 0) {
                treeView.SelectedNode = treeView.Nodes[0];
            }

            // 
            // splitter1
            // 

            splitter1.Dock = DockStyle.Left;
            splitter1.Location = new Point(140, 0);
            splitter1.Name = "splitter1";
            splitter1.Size = new Size(2, 266);
            splitter1.TabIndex = 7;
            splitter1.TabStop = false;


            // 
            // propertyGridContextMenu
            //
            propertyGridContextMenu.Popup += new EventHandler(OnPropertyGridPopupContextMenu);

            // 
            // propertyGrid
            //
            propertyGrid.Dock = DockStyle.Fill;
            propertyGrid.Font = new Font("Tahoma", 8.25F, FontStyle.Regular, GraphicsUnit.Point, ((System.Byte)(0)));
            propertyGrid.Location = new Point(140, 0);
            propertyGrid.Name = "propertyGrid";
            propertyGrid.Size = new Size(250, 266);
            propertyGrid.TabIndex = 1;
            propertyGrid.PropertySort = PropertySort.Alphabetical;
            propertyGrid.ToolbarVisible = false;
            propertyGrid.PropertyValueChanged += new PropertyValueChangedEventHandler(OnPropertyValueChanged);
            propertyGrid.ContextMenu = propertyGridContextMenu;


            // 
            // panel1
            // 
            panel1.Controls.Add(propertyGrid);
            panel1.Dock = DockStyle.Fill;
            this.panel1.Location = new Point(142, 0);
            panel1.Name = "panel1";
            panel1.Size = new Size(409, 266);
            panel1.TabIndex = 9;

            this.Controls.Add(panel1);
            Controls.Add(splitter1);
            Controls.Add(treeView);


        }
        #endregion

        protected virtual StringCollection SkipElements {
            get {
                return new StringCollection();
            }
        }

        protected abstract ArrayList GetXmlNodes();

        public void OnPropertyValueChanged(object s, PropertyValueChangedEventArgs e) {
            if (treeView.SelectedNode == null) {
                return;
            }

            XmlNode node = (XmlNode) treeView.SelectedNode.Tag;
            
            string displayName = GetDisplayName(node);
            if (displayName != null && displayName.Length > 0 &&
                treeView.SelectedNode.Text != displayName) {
                treeView.SelectedNode.Text = displayName;
            }
        }

        public void OnPropertyGridPopupContextMenu(object sender, EventArgs e) {
            if (propertyGrid.SelectedObject == null) {
                return;
            }

            MenuItem menuItemSeparator = new IconMenuItem("-");

            // Define the MenuItem objects to display for the TextBox.
            MenuItem menuItem1 = new IconMenuItem("&New", new Bitmap(WixFiles.GetResourceStream("WixEdit.new.bmp")));
            MenuItem menuItem2 = new IconMenuItem("&Delete", new Bitmap(WixFiles.GetResourceStream("WixEdit.delete.bmp")));
            MenuItem menuItem3 = new IconMenuItem("Description");
            
            menuItem3.Checked = propertyGrid.HelpVisible;

            menuItem1.Click += new EventHandler(OnNewPropertyGridItem);
            menuItem2.Click += new EventHandler(OnDeletePropertyGridItem);
            menuItem3.Click += new EventHandler(OnToggleDescriptionPropertyGrid);
        
            // Clear all previously added MenuItems.
            propertyGridContextMenu.MenuItems.Clear();

            propertyGridContextMenu.MenuItems.Add(menuItem1);
            propertyGridContextMenu.MenuItems.Add(menuItem2);
            propertyGridContextMenu.MenuItems.Add(menuItemSeparator);
            propertyGridContextMenu.MenuItems.Add(menuItem3);
        }

        public void OnNewPropertyGridItem(object sender, EventArgs e) {
            // Temporarily store the XmlAttributeAdapter
            XmlAttributeAdapter attAdapter = (XmlAttributeAdapter) propertyGrid.SelectedObject;

            ArrayList attributes = new ArrayList();

            XmlNodeList xmlAttributes = attAdapter.XmlNodeDefinition.SelectNodes("xs:attribute", wixFiles.XsdNsmgr);
            foreach (XmlNode at in xmlAttributes) {
                string attName = at.Attributes["name"].Value;
                if (attAdapter.XmlNode.Attributes[attName] == null) {
                    attributes.Add(attName);
                }
            }

            if (attAdapter.XmlNodeDefinition.Name == "xs:extension") {
                bool hasInnerText = false;
                foreach (GridItem it in propertyGrid.SelectedGridItem.Parent.GridItems) {
                    if (it.Label == "InnerText") {
                        hasInnerText = true;
                        break;
                    }
                }
                if (hasInnerText == false) {
                    attributes.Add("InnerText");
                }
            }

            SelectStringForm frm = new SelectStringForm();
            frm.PossibleStrings = attributes.ToArray(typeof(String)) as String[];
            if (DialogResult.OK != frm.ShowDialog()) {
                return;
            }

            // Show dialog to choose from available items.
            string newAttributeName = frm.SelectedString;

            if (newAttributeName == "InnerText") {
                attAdapter.ShowInnerTextIfEmpty = true;
            } else {
                // Get the XmlAttribute from the PropertyDescriptor
                XmlAttributePropertyDescriptor desc = propertyGrid.SelectedGridItem.PropertyDescriptor as XmlAttributePropertyDescriptor;
                XmlAttribute att = wixFiles.WxsDocument.CreateAttribute(newAttributeName);
    
                // resetting the propertyGrid.
                propertyGrid.SelectedObject = null;
    
                // Add the attribute
                attAdapter.XmlNode.Attributes.Append(att);
            }

            // Update the propertyGrid.
            propertyGrid.SelectedObject = attAdapter;
            propertyGrid.Update();

            foreach (GridItem it in propertyGrid.SelectedGridItem.Parent.GridItems) {
                if (it.Label == newAttributeName) {
                    propertyGrid.SelectedGridItem = it;
                    break;
                }
            }
        }

        protected void OnDeletePropertyGridItem(object sender, EventArgs e) {
            // Get the XmlAttribute from the PropertyDescriptor
            XmlAttributePropertyDescriptor desc = propertyGrid.SelectedGridItem.PropertyDescriptor as XmlAttributePropertyDescriptor;
            XmlAttribute att = desc.Attribute;

            // Temporarily store the XmlAttributeAdapter, while resetting the propertyGrid.
            XmlAttributeAdapter attAdapter = (XmlAttributeAdapter) propertyGrid.SelectedObject;
            propertyGrid.SelectedObject = null;

            // Remove the attribute
            attAdapter.XmlNode.Attributes.Remove(att);

            // Update the propertyGrid.
            propertyGrid.SelectedObject = attAdapter;
            propertyGrid.Update();
        }

        protected void OnToggleDescriptionPropertyGrid(object sender, EventArgs e) {
            propertyGrid.HelpVisible = !propertyGrid.HelpVisible;
        }

        protected void AddTreeNodesRecursive(XmlNode file, TreeNodeCollection nodes) {
            if (file.Name.StartsWith("#") ||
                SkipElements.Contains(file.Name)) {
                return;
            }

            TreeNode node = new TreeNode(GetDisplayName(file));

            node.Tag = file;

            if (file.Name == "File" && file.Attributes["src"] != null) {
                string filePath = Path.Combine(wixFiles.WxsDirectory.FullName, file.Attributes["src"].Value);

                FileInfo fileInfo = new FileInfo(filePath);
                if (fileInfo.Exists) {
                    Icon ico = FileIconFactory.GetFileIcon(filePath, false);
                    treeView.ImageList.Images.Add(ico);
    
                    node.ImageIndex = treeView.ImageList.Images.Count - 1;
                    node.SelectedImageIndex = treeView.ImageList.Images.Count - 1;
                } else {
                    int imageIndex = ImageListFactory.GetImageIndex(file.Name);
                    if (imageIndex >= 0) {
                        node.ImageIndex = imageIndex;
                        node.SelectedImageIndex = imageIndex;
                    }
                }
            } else {
                int imageIndex = ImageListFactory.GetImageIndex(file.Name);
                if (imageIndex >= 0) {
                    node.ImageIndex = imageIndex;
                    node.SelectedImageIndex = imageIndex;
                }
            }

            nodes.Add(node);

            foreach (XmlNode child in file.ChildNodes) {
                AddTreeNodesRecursive(child, node.Nodes);
            }
            
        }

        protected string GetDisplayName(XmlNode element) {
            string displayName = null;

            switch (element.Name) {
                case "Directory":
                case "File":
                    XmlAttribute nameAtt = element.Attributes["LongName"];
                    if (nameAtt == null) {
                        nameAtt = element.Attributes["Name"];
                    }
                    displayName = nameAtt.Value;
                    break;
                case "Registry":
                    string root = element.Attributes["Root"].Value;
                    string key = element.Attributes["Key"].Value;
                    XmlAttribute name = element.Attributes["Name"];
                    if (name != null) {
                        if (key.EndsWith("\\") == false) {
                            key = key + "\\";
                        }
                        key = key + name.Value;
                    }

                    displayName = root + "\\" + key;
                    break;
                case "Component":
                case "CustomAction":
                    XmlAttribute idAtt = element.Attributes["Id"];
                    if (idAtt != null) {
                        displayName = idAtt.Value;
                    } else {
                        displayName = element.Name;
                    }
                    break;
                case "Show":
                    XmlAttribute dlgAtt = element.Attributes["Dialog"];
                    if (dlgAtt != null) {
                        displayName = dlgAtt.Value;
                    } else {
                        displayName = element.Name;
                    }
                    break;
                case "Custom":
                    XmlAttribute actionAtt = element.Attributes["Action"];
                    if (actionAtt != null) {
                        displayName = actionAtt.Value;
                    } else {
                        displayName = element.Name;
                    }
                    break;
                default:
                    displayName = element.Name;
                    break;
            }

            return displayName;
        }

        protected virtual void OnGlobalTreeViewContextMenu(object sender, System.Windows.Forms.MouseEventArgs e) {
        }

        private void TreeViewMouseDown(object sender, System.Windows.Forms.MouseEventArgs e) {
            if (e.Button == MouseButtons.Right) {
                TreeNode node = treeView.GetNodeAt(e.X, e.Y);
                if (node == null) {
                    OnGlobalTreeViewContextMenu(sender, e);
                    return;
                }
                treeView.SelectedNode = node;

                Point spot = PointToClient(treeView.PointToScreen(new Point(e.X,e.Y)));
                treeViewContextMenu.Show(this, spot);
            }
        }

        private void OnAfterSelect(object sender, TreeViewEventArgs e) {
            XmlNode node = e.Node.Tag as XmlNode;
            if (node != null) {
                ShowProperties(node);
            }
        }

        protected void ShowProperties(XmlNode xmlNode) {
            XmlAttributeAdapter attAdapter = null;
            if (xmlNode != null) {
                attAdapter = new XmlAttributeAdapter(xmlNode, wixFiles);
            }

            propertyGrid.SelectedObject = attAdapter;
            propertyGrid.Update();

            return;
        }

        protected void PopupTreeViewContextMenu(System.Object sender, System.EventArgs e) {
            XmlNode node = treeView.SelectedNode.Tag as XmlNode;
            if (node == null) {
                return;
            }

            treeViewContextMenu.MenuItems.Clear();

            newSubElementsMenu.MenuItems.Clear();

            XmlNodeList xmlSubElements = wixFiles.XsdDocument.SelectNodes(String.Format("/xs:schema/xs:element[@name='{0}']/xs:complexType//xs:element", node.Name), wixFiles.XsdNsmgr);
            foreach (XmlNode xmlSubElement in xmlSubElements) {
                XmlAttribute refAtt = xmlSubElement.Attributes["ref"];
                if (refAtt != null) {
                    if (refAtt.Value != null && refAtt.Value.Length > 0) {
                        if (SkipElements.Contains(refAtt.Value)) {
                            continue;
                        }

                        IconMenuItem subMenuItem = new IconMenuItem(refAtt.Value, new Bitmap(WixFiles.GetResourceStream("WixEdit.empty.bmp")));
                        subMenuItem.Click += new EventHandler(NewElement_Click);

                        newSubElementsMenu.MenuItems.Add(subMenuItem);
                    }
                }
            }

            if (newSubElementsMenu.MenuItems.Count > 0) {
                treeViewContextMenu.MenuItems.Add(newSubElementsMenu);
            }

            treeViewContextMenu.MenuItems.Add(deleteCurrentElementMenu);

            XmlAttributeAdapter attAdapter = (XmlAttributeAdapter) propertyGrid.SelectedObject;

            XmlDocumentationManager docManager = new XmlDocumentationManager(wixFiles);
            if (docManager.HasDocumentation(attAdapter.XmlNodeDefinition)) {
                treeViewContextMenu.MenuItems.Add(new IconMenuItem("-"));
                treeViewContextMenu.MenuItems.Add(this.infoAboutCurrentElementMenu);
            }
        }

        private void NewElement_Click(object sender, System.EventArgs e) {
            MenuItem menuItem = sender as MenuItem;
            if (menuItem != null) {
                CreateNewSubElement(menuItem.Text);
            }
        }

        private void DeleteElement_Click(object sender, System.EventArgs e) {
            XmlNode node = treeView.SelectedNode.Tag as XmlNode;
            if (node == null) {
                return;
            }

            node.ParentNode.RemoveChild(node);

            treeView.Nodes.Remove(treeView.SelectedNode);

            ShowProperties(treeView.SelectedNode.Tag as XmlNode);
        }

        private void InfoAboutCurrentElement_Click(object sender, System.EventArgs e) {
            XmlNode xmlNode = (XmlNode) treeView.SelectedNode.Tag;

            XmlDocumentationManager docManager = new XmlDocumentationManager(wixFiles);
            XmlAttributeAdapter attAdapter = (XmlAttributeAdapter) propertyGrid.SelectedObject;
            
            string title = String.Format("Info about '{0}' element", xmlNode.Name);
            string message = docManager.GetDocumentation(attAdapter.XmlNodeDefinition);

            MessageBox.Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }


        protected void CreateNewSubElement(string typeName) {
            XmlNode node = treeView.SelectedNode.Tag as XmlNode;
            if (node == null) {
                return;
            }

            XmlElement newElement = node.OwnerDocument.CreateElement(typeName, "http://schemas.microsoft.com/wix/2003/01/wi");
            TreeNode control = new TreeNode(typeName);
            control.Tag = newElement;

            int imageIndex = ImageListFactory.GetImageIndex(typeName);
            if (imageIndex >= 0) {
                control.ImageIndex = imageIndex;
                control.SelectedImageIndex = imageIndex;
            }

            node.AppendChild(newElement);

            treeView.SelectedNode.Nodes.Add(control);
            treeView.SelectedNode = control;

            ShowProperties(newElement);       
        }
    }
}
