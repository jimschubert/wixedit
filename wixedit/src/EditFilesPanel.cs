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
    /// Summary description for EditResourcesPanel.
    /// </summary>
    public class EditFilesPanel : BasePanel {
        private TreeView directoryTreeView;
        private ContextMenu directoryTreeViewContextMenu;
        private StringCollection types;
        private Splitter splitter1;
        private PropertyGrid propertyGrid;
        private Panel panel1;
        private ContextMenu propertyGridContextMenu;

        public EditFilesPanel(WixFiles wixFiles) : base(wixFiles) {
            types = new StringCollection();
            
            RecurseTypes("Directory", types);

            InitializeComponent();
        }

        private void RecurseTypes(string name, StringCollection types) {
            XmlNodeList xmlSubElements = wixFiles.XsdDocument.SelectNodes(String.Format("/xs:schema/xs:element[@name='{0}']/xs:complexType/xs:choice/xs:element", name), wixFiles.XsdNsmgr);

            foreach (XmlNode xmlSubElement in xmlSubElements) {
                XmlAttribute refAtt = xmlSubElement.Attributes["ref"];
                if (refAtt != null) {
                    if (refAtt.Value != null && refAtt.Value.Length > 0) {
                        if (types.Contains(refAtt.Value) == false) {
                            types.Add(refAtt.Value);
                            RecurseTypes(refAtt.Value, types);
                        }
                    }
                }
            }
        }

        private ImageList GetTypesImageList() {
            ImageList images = new ImageList(); 

            Bitmap unknownBmp = new Bitmap(WixFiles.GetResourceStream("WixEdit.unknown.bmp"));
            unknownBmp.MakeTransparent();

            Bitmap typeBmp;
            foreach (string type in types) {
                try {
                    typeBmp = new Bitmap(WixFiles.GetResourceStream(String.Format("WixEdit.{0}.bmp", type.ToLower())));
                    typeBmp.MakeTransparent();
                } catch {
                    typeBmp = unknownBmp;
                }

                images.Images.Add(typeBmp);
            }

            return images;
        }

        #region Initialize Controls
        private void InitializeComponent() {
            this.directoryTreeView = new TreeView();
            this.splitter1 = new Splitter();
            this.propertyGrid = new CustomPropertyGrid();
            this.panel1 = new Panel();
            this.propertyGridContextMenu = new ContextMenu();


            // 
            // directoryTreeView
            // 
            this.directoryTreeView.Dock = DockStyle.Left;
            this.directoryTreeView.ImageIndex = -1;
            this.directoryTreeView.Location = new Point(0, 0);
            this.directoryTreeView.Name = "directoryTreeView";
            this.directoryTreeView.SelectedImageIndex = -1;
            this.directoryTreeView.Size = new Size(350, 266);
            this.directoryTreeView.TabIndex = 6;

            this.directoryTreeView.AfterSelect += new TreeViewEventHandler(this.OnAfterSelect);
            this.directoryTreeViewContextMenu = new ContextMenu();
            this.directoryTreeViewContextMenu.Popup += new EventHandler(PopupDirectoryTreeViewContextMenu);
            this.directoryTreeView.MouseDown += new MouseEventHandler(DirectoryViewMouseDown);

            this.directoryTreeView.ImageList = GetTypesImageList();


            XmlNodeList files = wixFiles.WxsDocument.SelectNodes("/wix:Wix/wix:Product/wix:Directory", wixFiles.WxsNsmgr);
            foreach (XmlNode file in files) {
                AddTreeNodesRecursive(file, directoryTreeView.Nodes);
            }

            directoryTreeView.ExpandAll();
            if (directoryTreeView.Nodes.Count > 0) {
                directoryTreeView.SelectedNode = directoryTreeView.Nodes[0];
            }

            // 
            // splitter1
            // 

            this.splitter1.Dock = DockStyle.Left;
            this.splitter1.Location = new Point(140, 0);
            this.splitter1.Name = "splitter1";
            this.splitter1.Size = new Size(2, 266);
            this.splitter1.TabIndex = 7;
            this.splitter1.TabStop = false;


            // 
            // propertyGridContextMenu
            //
            this.propertyGridContextMenu.Popup += new EventHandler(OnPropertyGridPopupContextMenu);

            // 
            // propertyGrid
            //
            this.propertyGrid.Dock = DockStyle.Fill;
            this.propertyGrid.Font = new Font("Tahoma", 8.25F, FontStyle.Regular, GraphicsUnit.Point, ((System.Byte)(0)));
            this.propertyGrid.Location = new Point(140, 0);
            this.propertyGrid.Name = "propertyGrid";
            this.propertyGrid.Size = new Size(250, 266);
            this.propertyGrid.TabIndex = 1;
            this.propertyGrid.PropertySort = PropertySort.Alphabetical;
            this.propertyGrid.ToolbarVisible = false;
//            this.propertyGrid.PropertyValueChanged += new PropertyValueChangedEventHandler(OnPropertyValueChanged);
            this.propertyGrid.ContextMenu = this.propertyGridContextMenu;


            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.propertyGrid);
            this.panel1.Dock = DockStyle.Fill;
            this.panel1.Location = new Point(142, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new Size(409, 266);
            this.panel1.TabIndex = 9;

            this.Controls.Add(panel1);
            this.Controls.Add(this.splitter1);
            this.Controls.Add(this.directoryTreeView);


        }
        #endregion

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
            XmlAttributeAdapter attAdapter = propertyGrid.SelectedObject as XmlAttributeAdapter;

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

        public void OnToggleDescriptionPropertyGrid(object sender, EventArgs e) {
            propertyGrid.HelpVisible = !propertyGrid.HelpVisible;
        }

        public void OnDeletePropertyGridItem(object sender, EventArgs e) {
            // Get the XmlAttribute from the PropertyDescriptor
            XmlAttributePropertyDescriptor desc = propertyGrid.SelectedGridItem.PropertyDescriptor as XmlAttributePropertyDescriptor;
            XmlAttribute att = desc.Attribute;

            // Temporarily store the XmlAttributeAdapter, while resetting the propertyGrid.
            XmlAttributeAdapter attAdapter = propertyGrid.SelectedObject as XmlAttributeAdapter;
            propertyGrid.SelectedObject = null;

            // Remove the attribute
            attAdapter.XmlNode.Attributes.Remove(att);

            // Update the propertyGrid.
            propertyGrid.SelectedObject = attAdapter;
            propertyGrid.Update();
        }

        private void AddTreeNodesRecursive(XmlNode file, TreeNodeCollection nodes) {
            if (file.Name == "#comment") {
                return;
            }

            TreeNode node;

            switch (file.Name) {
                case "Directory":
                case "File":
                    XmlAttribute nameAtt = file.Attributes["LongName"];
                    if (nameAtt == null) {
                        nameAtt = file.Attributes["Name"];
                    }
                    node = new TreeNode(nameAtt.Value);
                    break;
                case "Registry":
                    string root = file.Attributes["Root"].Value;
                    string key = file.Attributes["Key"].Value;
                    XmlAttribute name = file.Attributes["Name"];
                    if (name != null) {
                        if (key.EndsWith("\\") == false) {
                            key = key + "\\";
                        }
                        key = key + name.Value;
                    }

                    node = new TreeNode(root + "\\" + key);
                    break;
                case "Component":
                    XmlAttribute idAtt = file.Attributes["Id"];
                    if (idAtt != null) {
                        node = new TreeNode(idAtt.Value);
                    } else {
                        node = new TreeNode(file.Name);
                    }
                    break;
                default:
                    node = new TreeNode(file.Name);
                    break;
            }

            node.Tag = file;
            int imageIndex = types.IndexOf(file.Name);
            if (imageIndex >= 0) {
                node.ImageIndex = imageIndex;
                node.SelectedImageIndex = imageIndex;
            }

            nodes.Add(node);

            foreach (XmlNode child in file.ChildNodes) {
                AddTreeNodesRecursive(child, node.Nodes);
            }
            
        }

        private void DirectoryViewMouseDown(object sender, System.Windows.Forms.MouseEventArgs e) {
            if (e.Button == MouseButtons.Right) {
                TreeNode node = directoryTreeView.GetNodeAt(e.X, e.Y);
                if (node == null) {
                    return;
                }
                directoryTreeView.SelectedNode = node;

                Point spot = this.PointToClient(directoryTreeView.PointToScreen(new Point(e.X,e.Y)));
                directoryTreeViewContextMenu.Show(this, spot);
            }
        }

        private void OnAfterSelect(object sender, TreeViewEventArgs e) {
            XmlNode node = e.Node.Tag as XmlNode;
            if (node != null) {
                ShowProperties(node);
            }
        }

        private void ShowProperties(XmlNode xmlNode) {
            XmlAttributeAdapter attAdapter = new XmlAttributeAdapter(xmlNode, wixFiles);

            propertyGrid.SelectedObject = attAdapter;
            propertyGrid.Update();

            return;
        }

        protected void PopupDirectoryTreeViewContextMenu(System.Object sender, System.EventArgs e) {
            XmlNode node = directoryTreeView.SelectedNode.Tag as XmlNode;
            if (node == null) {
                return;
            }

            directoryTreeViewContextMenu.MenuItems.Clear();

            XmlNodeList xmlSubElements = wixFiles.XsdDocument.SelectNodes(String.Format("/xs:schema/xs:element[@name='{0}']/xs:complexType/xs:choice/xs:element", node.Name), wixFiles.XsdNsmgr);

            foreach (XmlNode xmlSubElement in xmlSubElements) {
                XmlAttribute refAtt = xmlSubElement.Attributes["ref"];
                if (refAtt != null) {
                    if (refAtt.Value != null && refAtt.Value.Length > 0) {
                        directoryTreeViewContextMenu.MenuItems.Add(refAtt.Value, new EventHandler(NewElement_Click));       
                    }
                }
            }
        }

        private void NewElement_Click(object sender, System.EventArgs e) {
            MenuItem menuItem = sender as MenuItem;
            if (menuItem != null) {
                CreateNewSubElement(menuItem.Text);
            }
        }

        private void CreateNewSubElement(string typeName) {
            XmlNode node = directoryTreeView.SelectedNode.Tag as XmlNode;
            if (node == null) {
                return;
            }

            // Get new name, and add Text element
            // EnterStringForm frm = new EnterStringForm();
            // frm.Text = "Enter new Text value";
            XmlElement newElement = node.OwnerDocument.CreateElement(typeName, "http://schemas.microsoft.com/wix/2003/01/wi");
            TreeNode control = new TreeNode(typeName);
            control.Tag = newElement;
            
            int imageIndex = types.IndexOf(typeName);
            if (imageIndex >= 0) {
                control.ImageIndex = imageIndex;
                control.SelectedImageIndex = imageIndex;
            }

            directoryTreeView.SelectedNode.Nodes.Add(control);
            directoryTreeView.SelectedNode = control;

            ShowProperties(newElement);       
        }

        private ImageList GetDirectoryTreeViewImageList() {
            ImageList images = new ImageList(); 

            Bitmap bmp = new Bitmap(WixFiles.GetResourceStream("WixEdit.empty.bmp"));
            bmp.MakeTransparent();
            images.Images.Add(bmp);

            bmp = new Bitmap(WixFiles.GetResourceStream("WixEdit.dialog.bmp"));
            bmp.MakeTransparent();
            images.Images.Add(bmp);

            bmp = new Bitmap(WixFiles.GetResourceStream("WixEdit.control.bmp"));
            bmp.MakeTransparent();
            images.Images.Add(bmp);

            bmp = new Bitmap(WixFiles.GetResourceStream("WixEdit.text.bmp"));
            bmp.MakeTransparent();
            images.Images.Add(bmp);

            bmp = new Bitmap(WixFiles.GetResourceStream("WixEdit.condition.bmp"));
            bmp.MakeTransparent();
            images.Images.Add(bmp);

            bmp = new Bitmap(WixFiles.GetResourceStream("WixEdit.subscribe.bmp"));
            bmp.MakeTransparent();
            images.Images.Add(bmp);

            bmp = new Bitmap(WixFiles.GetResourceStream("WixEdit.publish.bmp"));
            bmp.MakeTransparent();
            images.Images.Add(bmp);

            return images;
        }
    }
}
