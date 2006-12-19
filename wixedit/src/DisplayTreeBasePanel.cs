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
    public abstract class DisplayTreeBasePanel : DisplayBasePanel {
        protected TreeView currTreeView;
        protected ContextMenu currTreeViewContextMenu;
        protected ContextMenu panelContextMenu;
        private Panel panel1;
        private Splitter splitter1;

        public DisplayTreeBasePanel(WixFiles wixFiles, string xpath, string elementName, string keyName) : base(wixFiles, xpath, elementName, keyName) {
            InitializeComponent();
            CreateControl();
        }

        public DisplayTreeBasePanel(WixFiles wixFiles, string xpath, string keyName) : base(wixFiles, xpath, keyName) {
            InitializeComponent();
            CreateControl();
        }

        protected TreeView CurrentTreeView {
            get {
                return currTreeView;
            }
            set {
                currTreeView = value;
            }
        }

        protected ContextMenu CurrentTreeViewContextMenu {
            get {
                return currTreeViewContextMenu;
            }
            set {
                currTreeViewContextMenu = value;
            }
        }

        protected ContextMenu PanelContextMenu {
            get {
                return panelContextMenu;
            }
            set {
                panelContextMenu = value;
            }
        }

        #region Initialize Controls
        private void InitializeComponent() {
            currTreeView = new TreeView();
            splitter1 = new Splitter();
            panel1 = new Panel();

            CustomPropertyGrid currGrid = new CustomPropertyGrid();
            ContextMenu currGridContextMenu = new ContextMenu();

            panelContextMenu = new ContextMenu();
            panelContextMenu.Popup += new EventHandler(PopupPanelContextMenu);

            currTreeView.HideSelection = false;
            currTreeView.Dock = DockStyle.Left;
            currTreeView.ImageIndex = -1;
            currTreeView.Location = new Point(0, 0);
            currTreeView.Name = "currTreeView";
            currTreeView.SelectedImageIndex = -1;
            currTreeView.Size = new Size(256, 266);
            currTreeView.TabIndex = 6;
            currTreeView.ImageList = ImageListFactory.GetImageList();
            currTreeView.AfterSelect += new TreeViewEventHandler(OnAfterSelect);
            currTreeViewContextMenu = new ContextMenu();
            currTreeViewContextMenu.Popup += new EventHandler(PopupTreeViewContextMenu);
            currTreeView.MouseDown += new MouseEventHandler(TreeViewMouseDown);
            currTreeView.KeyDown += new KeyEventHandler(TreeViewKeyDown);

            splitter1.Dock = DockStyle.Left;
            splitter1.Location = new Point(140, 0);
            splitter1.Name = "splitter1";
            splitter1.Size = new Size(2, 266);
            splitter1.TabIndex = 7;
            splitter1.TabStop = false;
 
            currGridContextMenu.Popup += new EventHandler(OnPropertyGridPopupContextMenu);

            currGrid.Dock = DockStyle.Fill;
            currGrid.Font = new Font("Tahoma", 8.25F, FontStyle.Regular, GraphicsUnit.Point, ((System.Byte)(0)));
            currGrid.Location = new Point(140, 0);
            currGrid.Name = "_currGrid";
            currGrid.Size = new Size(250, 266);
            currGrid.TabIndex = 1;
            currGrid.PropertySort = PropertySort.Alphabetical;
            currGrid.ToolbarVisible = false;
            currGrid.PropertyValueChanged += new PropertyValueChangedEventHandler(OnPropertyValueChanged);
            currGrid.ContextMenu = currGridContextMenu;

            panel1.Controls.Add(currGrid);
            panel1.Dock = DockStyle.Fill;
            panel1.Location = new Point(142, 0);
            panel1.Name = "panel1";
            panel1.Size = new Size(409, 266);
            panel1.TabIndex = 9;

            Controls.Add(panel1);
            Controls.Add(splitter1);
            Controls.Add(currTreeView);

            CurrentGrid = currGrid;
            CurrentGridContextMenu = currGridContextMenu;
        }
        #endregion

        public override bool IsOwnerOfNode(XmlNode node) {
            return FindNode(GetShowableNode(node), CurrentList);
        }
       
        public override void ShowNode(XmlNode node) {
            TreeNode treeNode = FindTreeNode(GetShowableNode(node), currTreeView.Nodes);
            if (treeNode == null) {
                InitTreeView();
                treeNode = FindTreeNode(GetShowableNode(node), currTreeView.Nodes);
            }
            if (treeNode != null) {
                currTreeView.SelectedNode = null;
                currTreeView.SelectedNode = treeNode;
                if (node is XmlAttribute) {
                    foreach (GridItem item in CurrentGrid.SelectedGridItem.Parent.GridItems) {
                        if (node.Name == item.Label) {
                            CurrentGrid.SelectedGridItem = item;
                            break;
                        }
                    }
                }
            }
        }

        public override XmlNode GetShowingNode() {
            if (currentGrid.SelectedGridItem != null) {
                CustomXmlPropertyDescriptorBase desc = CurrentGrid.SelectedGridItem.PropertyDescriptor as CustomXmlPropertyDescriptorBase;
                if (desc.XmlElement is XmlAttribute) {
                    return desc.XmlElement;
                } else if (desc.XmlElement.Attributes[currentGrid.SelectedGridItem.Label] != null) {
                    return desc.XmlElement.Attributes[currentGrid.SelectedGridItem.Label];
                } else {
                    return desc.XmlElement;
                }
            } else if (currTreeView.SelectedNode != null) {
                return (XmlNode) currTreeView.SelectedNode.Tag;
            }

            return null;
        }

        public void OnPropertyValueChanged(object s, PropertyValueChangedEventArgs e) {
            if (currTreeView.SelectedNode == null) {
                return;
            }
            XmlNode node = (XmlNode) currTreeView.SelectedNode.Tag;
            string displayName = GetDisplayName(node);
            if (displayName != null && displayName.Length > 0 &&
                currTreeView.SelectedNode.Text != displayName) {
                currTreeView.SelectedNode.Text = displayName;
            }
        }

        public void OnPropertyGridPopupContextMenu(object sender, EventArgs e) {
            CurrentGridContextMenu.MenuItems.Clear();

            if (CurrentGrid.SelectedObject == null) {
                return;
            }

            XmlAttributeAdapter attAdapter = (XmlAttributeAdapter) CurrentGrid.SelectedObject;
            if (attAdapter.XmlNodeDefinition == null) {
                // How can this happen? Just trow an exception so it can be reported to the WixEdit website.
                throw new Exception(String.Format("XmlAttributeAdapter.XmlNodeDefinition is null of \"{0}\" in {1}", attAdapter.GetType(), this.GetType()));
            }
            
            // Need to change "Delete" to "Clear" for required items.
            bool isRequired = false;

            // Get the XmlAttribute from the PropertyDescriptor
            XmlAttributePropertyDescriptor desc = CurrentGrid.SelectedGridItem.PropertyDescriptor as XmlAttributePropertyDescriptor;
            if (desc != null) {
                XmlAttribute att = desc.Attribute;
                XmlNode xmlAttributeDefinition = attAdapter.XmlNodeDefinition.SelectSingleNode(String.Format("xs:attribute[@name='{0}']", att.Name), WixFiles.XsdNsmgr);

                if (xmlAttributeDefinition.Attributes["use"] != null &&
                    xmlAttributeDefinition.Attributes["use"].Value == "required") {
                    isRequired = true;
                }
            }

            MenuItem menuItemSeparator = new IconMenuItem("-");

            // See if new menu item should be shown.
            bool canCreateNew = false;

            XmlNodeList xmlAttributes = attAdapter.XmlNodeDefinition.SelectNodes("xs:attribute", WixFiles.XsdNsmgr);
            foreach (XmlNode at in xmlAttributes) {
                string attName = at.Attributes["name"].Value;
                if (attAdapter.XmlNode.Attributes[attName] == null) {
                    canCreateNew = true;
                }
            }

            if (canCreateNew) {
                // Define the MenuItem objects to display for the TextBox.
                MenuItem menuItem1 = new IconMenuItem("&New", new Bitmap(WixFiles.GetResourceStream("bmp.new.bmp")));
                menuItem1.Click += new EventHandler(OnNewPropertyGridItem);
                CurrentGridContextMenu.MenuItems.Add(menuItem1);
            }

            // Add the clear or delete menu item
            MenuItem menuItem2 = null;
            if (CurrentGrid.SelectedGridItem.PropertyDescriptor != null &&
                !(CurrentGrid.SelectedGridItem.PropertyDescriptor is InnerTextPropertyDescriptor)) {
                if (isRequired) {
                    menuItem2 = new IconMenuItem("&Clear", new Bitmap(WixFiles.GetResourceStream("bmp.clear.bmp")));
                } else {
                    menuItem2 = new IconMenuItem("&Delete", new Bitmap(WixFiles.GetResourceStream("bmp.delete.bmp")));
                }
                menuItem2.Click += new EventHandler(OnDeletePropertyGridItem);
                CurrentGridContextMenu.MenuItems.Add(menuItem2);
            }

            if (CurrentGridContextMenu.MenuItems.Count > 0) {
                CurrentGridContextMenu.MenuItems.Add(menuItemSeparator);
            }

            MenuItem menuItem3 = new IconMenuItem("Description");
            menuItem3.Click += new EventHandler(OnToggleDescriptionPropertyGrid);
            menuItem3.Checked = CurrentGrid.HelpVisible;

            CurrentGridContextMenu.MenuItems.Add(menuItem3);
        }

        public virtual void ImportElement_Click(object sender, EventArgs e) {
            if (this.ImportItems(CurrentXPath)){
                this.LoadData();
            } else if (CurrentElementName != null && this.ImportItems("//wix:" + CurrentElementName)){
                this.LoadData();
            }
        }

        public void OnNewPropertyGridItem(object sender, EventArgs e) {
            WixFiles.UndoManager.BeginNewCommandRange();

            // Temporarily store the XmlAttributeAdapter
            XmlAttributeAdapter attAdapter = (XmlAttributeAdapter) CurrentGrid.SelectedObject;

            ArrayList attributes = new ArrayList();

            XmlNodeList xmlAttributes = attAdapter.XmlNodeDefinition.SelectNodes("xs:attribute", WixFiles.XsdNsmgr);
            foreach (XmlNode at in xmlAttributes) {
                string attName = at.Attributes["name"].Value;
                if (attAdapter.XmlNode.Attributes[attName] == null) {
                    attributes.Add(attName);
                }
            }

            if (attAdapter.XmlNodeDefinition.Name == "xs:extension") {
                bool hasInnerText = false;
                foreach (GridItem it in CurrentGrid.SelectedGridItem.Parent.GridItems) {
                    if (it.Label == "InnerText") {
                        hasInnerText = true;
                        break;
                    }
                }
                if (hasInnerText == false) {
                    attributes.Add("InnerText");
                }
            }

            attributes.Sort();

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
                XmlAttribute att = WixFiles.WxsDocument.CreateAttribute(newAttributeName);
    
                // resetting the CurrentGrid.
                CurrentGrid.SelectedObject = null;
    
                // Add the attribute
                attAdapter.XmlNode.Attributes.Append(att);
            }

            // Update the CurrentGrid.
            CurrentGrid.SelectedObject = attAdapter;
            CurrentGrid.Update();

            foreach (GridItem it in CurrentGrid.SelectedGridItem.Parent.GridItems) {
                if (it.Label == newAttributeName) {
                    CurrentGrid.SelectedGridItem = it;
                    break;
                }
            }
        }

        public override void ReloadData() {
            CurrentGrid.SelectedObject = null;
            currTreeView.Nodes.Clear();
            LoadData();
        }

        protected virtual TreeNode FindTreeNode(XmlNode node, TreeNodeCollection treeNodes) {
            foreach (TreeNode treeNode in treeNodes) {
                if (treeNode.Tag == node) {
                    return treeNode;
                }
                TreeNode foundNode = FindTreeNode(node, treeNode.Nodes);
                if (foundNode != null) {
                    return foundNode;
                }
            }
            return null;
        }

        protected void LoadData() {
            CurrentList = WixFiles.WxsDocument.SelectNodes(CurrentXPath, WixFiles.WxsNsmgr);

            AssignParentNode();

            InitTreeView();
        }

        protected virtual StringCollection SkipElements {
            get {
                return new StringCollection();
            }
        }

        protected XmlNode GetSelectedProperty() {
            // Get the XmlAttribute from the PropertyDescriptor
            XmlNode element = null;
            if (CurrentGrid.SelectedGridItem.PropertyDescriptor is XmlAttributePropertyDescriptor) {
                XmlAttributePropertyDescriptor desc = CurrentGrid.SelectedGridItem.PropertyDescriptor as XmlAttributePropertyDescriptor;
                element = desc.Attribute;
            } else if (CurrentGrid.SelectedGridItem.PropertyDescriptor is CustomXmlPropertyDescriptorBase) {
                CustomXmlPropertyDescriptorBase desc = CurrentGrid.SelectedGridItem.PropertyDescriptor as CustomXmlPropertyDescriptorBase;
                element = desc.XmlElement;
            } else {
                string typeString = "null";
                if (CurrentGrid.SelectedGridItem.PropertyDescriptor != null) {
                    typeString = CurrentGrid.SelectedGridItem.PropertyDescriptor.GetType().ToString();
                }

                throw new Exception(String.Format("Expected XmlAttributePropertyDescriptor, but got {0} in OnDeletePropertyGridItem", typeString));
            }

            return element;
        }

        protected void OnDeletePropertyGridItem(object sender, EventArgs e) {
            XmlNode element = GetSelectedProperty();
            if (element == null) {
                throw new WixEditException("No element found to delete!");
            }

            // Temporarily store the XmlAttributeAdapter, while resetting the CurrentGrid.
            PropertyAdapterBase attAdapter = (PropertyAdapterBase) CurrentGrid.SelectedObject;
            CurrentGrid.SelectedObject = null;

            WixFiles.UndoManager.BeginNewCommandRange();
            attAdapter.RemoveProperty(element);

            // Update the CurrentGrid.
            CurrentGrid.SelectedObject = attAdapter;
            CurrentGrid.Update();
        }

        protected void OnToggleDescriptionPropertyGrid(object sender, EventArgs e) {
            CurrentGrid.HelpVisible = !CurrentGrid.HelpVisible;
        }

        protected void AddTreeNodesRecursive(XmlNode file, TreeNodeCollection nodes) {
            if (file.Name.StartsWith("#") ||
                SkipElements.Contains(file.Name)) {
                return;
            }

            TreeNode node = new TreeNode(GetDisplayName(file));
            node.Tag = file;

            if (file.Name == "File" && file.Attributes["Source"] != null) {
                string filePath = Path.Combine(WixFiles.WxsDirectory.FullName, file.Attributes["Source"].Value);

                if (File.Exists(filePath)) {
                    Icon ico = FileIconFactory.GetFileIcon(filePath);
                    if (ico != null) {
                        try {
                            currTreeView.ImageList.Images.Add(ico);
        
                            node.ImageIndex = currTreeView.ImageList.Images.Count - 1;
                            node.SelectedImageIndex = currTreeView.ImageList.Images.Count - 1;
                        } catch {
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

            if (file.ChildNodes.Count > 10000) {
                TreeNode tooManyNodes = new TreeNode("<< Too many children to display >>");
                node.ImageIndex = ImageListFactory.GetUnsupportedImageIndex();
                node.SelectedImageIndex = ImageListFactory.GetUnsupportedImageIndex();
                node.Nodes.Add(tooManyNodes);

                return;
            }

            foreach (XmlNode child in file.ChildNodes) {
                if (child.NodeType == XmlNodeType.ProcessingInstruction) {
                    continue;
                }

                AddTreeNodesRecursive(child, node.Nodes);
            }
        }

        protected string GetDisplayName(XmlNode element) {
            string displayName = null;
            try {
                switch (element.Name) {
                    case "Directory":
                    case "DirectoryRef":
                    case "File":
                        XmlAttribute nameAtt = element.Attributes["LongName"];
                        if (nameAtt == null) {
                            nameAtt = element.Attributes["Name"];
                        }
                        if (nameAtt == null) {
                            nameAtt = element.Attributes["Id"];
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
                    case "Feature":
                    case "ComponentRef":
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
                    case "Condition":
                        string innerText = element.InnerText;
                        if (innerText != null && innerText.Length > 1) {
                            displayName = String.Format("({0})", innerText);
                        } else {
                            displayName = element.Name;
                        }
                        break;
                    default:
                        displayName = element.Name;
                        break;
                }
            } catch {
                displayName = element.Name;
            }

            return displayName;
        }

        protected void ShowProperties(XmlNode xmlNode) {
            XmlAttributeAdapter attAdapter = null;
            if (xmlNode != null) {
                attAdapter = new XmlAttributeAdapter(xmlNode, WixFiles);
            }

            CurrentGrid.SelectedObject = attAdapter;
            CurrentGrid.Update();

            return;
        }

        protected void PopupTreeViewContextMenu(System.Object sender, System.EventArgs e) {
            XmlNode node = currTreeView.SelectedNode.Tag as XmlNode;
            if (node == null) {
                return;
            }

            IconMenuItem item1 = new IconMenuItem("&New", new Bitmap(WixFiles.GetResourceStream("bmp.new.bmp")));
            IconMenuItem item2 = new IconMenuItem("&Delete", new Bitmap(WixFiles.GetResourceStream("bmp.delete.bmp")));
            item2.Click += new System.EventHandler(DeleteElement_Click);
            IconMenuItem item3 = new IconMenuItem("&Info", new Bitmap(WixFiles.GetResourceStream("bmp.info.bmp")));
            item3.Click += new System.EventHandler(InfoAboutCurrentElement_Click);

            currTreeViewContextMenu.MenuItems.Clear();

            ArrayList newElementStrings = WixFiles.GetXsdSubElements(node.Name);
            newElementStrings.Sort();

            foreach (string newElementString in newElementStrings) {
                IconMenuItem subMenuItem = new IconMenuItem(newElementString);
                subMenuItem.Click += new EventHandler(NewElement_Click);
                item1.MenuItems.Add(subMenuItem);
            }

            if (item1.MenuItems.Count > 0) {
                currTreeViewContextMenu.MenuItems.Add(item1);
            }

            currTreeViewContextMenu.MenuItems.Add(item2);

            XmlAttributeAdapter attAdapter = (XmlAttributeAdapter) CurrentGrid.SelectedObject;
            XmlDocumentationManager docManager = new XmlDocumentationManager(WixFiles);
            if (docManager.HasDocumentation(attAdapter.XmlNodeDefinition)) {
                currTreeViewContextMenu.MenuItems.Add(new IconMenuItem("-"));
                currTreeViewContextMenu.MenuItems.Add(item3);
            }

            AddCustomTreeViewContextMenuItems(node, currTreeViewContextMenu);
        }

        protected virtual void PopupPanelContextMenu(System.Object sender, System.EventArgs e){
            //clear menu
            panelContextMenu.MenuItems.Clear();

            //add import menu
            IconMenuItem itemImport = new IconMenuItem("&Import", new Bitmap(WixFiles.GetResourceStream("bmp.import.bmp")));
            itemImport.Click += new System.EventHandler(ImportElement_Click);

            panelContextMenu.MenuItems.Add(new IconMenuItem("-"));
            panelContextMenu.MenuItems.Add(itemImport);
        }

        protected void OnPanelContextMenu(object sender, System.Windows.Forms.MouseEventArgs e) {
            Point spot = PointToClient(currTreeView.PointToScreen(new Point(e.X,e.Y)));
            panelContextMenu.Show(this, spot);
        }

        protected virtual void AddCustomTreeViewContextMenuItems(XmlNode node, ContextMenu currTreeViewContextMenu) {
        }

        protected TreeNode CreateNewSubElement(string typeName) {
            XmlNode node = currTreeView.SelectedNode.Tag as XmlNode;
            if (node == null) {
                return null;
            }

            WixFiles.UndoManager.BeginNewCommandRange();

            XmlElement newElement = node.OwnerDocument.CreateElement(typeName, WixFiles.GetNamespaceUri(typeName));
            TreeNode control = new TreeNode(typeName);
            control.Tag = newElement;

            int imageIndex = ImageListFactory.GetImageIndex(typeName);
            if (imageIndex >= 0) {
                control.ImageIndex = imageIndex;
                control.SelectedImageIndex = imageIndex;
            }

            InsertNewXmlNode(node, newElement);

            currTreeView.SelectedNode.Nodes.Add(control);

            return control;
        }

        protected virtual void NewCustomElement_Click(object sender, System.EventArgs e) {
            CreateNewCustomElement(CurrentElementName);
        }

        protected void CreateNewCustomElement(string elementName) {
            WixFiles.UndoManager.BeginNewCommandRange();

            if (CurrentParent == null) {
                MessageBox.Show("No location found to add UI element, need element like module or product!");
                return;
            }

            XmlElement newElement = WixFiles.WxsDocument.CreateElement(elementName, WixFiles.WixNamespaceUri);
            TreeNode action = new TreeNode(elementName);
            action.Tag = newElement;

            int imageIndex = ImageListFactory.GetImageIndex(elementName);
            if (imageIndex >= 0) {
                action.ImageIndex = imageIndex;
                action.SelectedImageIndex = imageIndex;
            }

            InsertNewXmlNode(CurrentParent, newElement);

            currTreeView.Nodes.Add(action);
            currTreeView.SelectedNode = action;

            ShowProperties(newElement); 
        }

        /// <summary>
        /// Method inits treeview and do its relayout
        /// </summary>
        private void InitTreeView(){
            this.SuspendLayout();

            currTreeView.Nodes.Clear();

            foreach (XmlNode file in CurrentList) {
                if (file.NodeType == XmlNodeType.ProcessingInstruction) {
                    continue;
                }

                AddTreeNodesRecursive(file, currTreeView.Nodes);
            }

            currTreeView.ExpandAll();

            if (currTreeView.Nodes.Count > 0) {
                currTreeView.SelectedNode = currTreeView.Nodes[0];
            }

            this.ResumeLayout();
        }

        private bool FindNode(XmlNode nodeToFind, IEnumerable xmlNodes) {
            foreach (XmlNode node in xmlNodes) {
                if (SkipElements.Contains(node.Name)) {
                    continue;
                }
                if (node == nodeToFind) {
                    return true;
                }
                if (FindNode(nodeToFind, node.ChildNodes)) {
                    return true;
                }
            }
            return false;
        }

        private void TreeViewMouseDown(object sender, System.Windows.Forms.MouseEventArgs e) {
            if (this.Visible == false) {
                return;
            }
            if (e.Button == MouseButtons.Right) {
                TreeNode node = currTreeView.GetNodeAt(e.X, e.Y);
                if (node == null) {
                    OnPanelContextMenu(sender, e);
                    return;
                }
                currTreeView.SelectedNode = node;

                Point spot = PointToClient(currTreeView.PointToScreen(new Point(e.X,e.Y)));
                currTreeViewContextMenu.Show(this, spot);
            }
        }

        private void TreeViewKeyDown(object sender, System.Windows.Forms.KeyEventArgs e) {
            if(e.KeyCode == Keys.Delete) {
                if (currTreeView.SelectedNode == null) {
                    return;
                }

                XmlNode node = currTreeView.SelectedNode.Tag as XmlNode;
                if (node == null) {
                    return;
                }

                WixFiles.UndoManager.BeginNewCommandRange();
      
                node.ParentNode.RemoveChild(node);
      
                currTreeView.Nodes.Remove(currTreeView.SelectedNode);
      
                if (currTreeView.SelectedNode != null) {
                    ShowProperties(currTreeView.SelectedNode.Tag as XmlNode);
                } else {
                    ShowProperties(null);
                }
            }
        }

        private void OnAfterSelect(object sender, TreeViewEventArgs e) {
            XmlNode node = e.Node.Tag as XmlNode;
            if (node != null) {
                ShowProperties(node);
            }
        }

        private void NewElement_Click(object sender, System.EventArgs e) {
            WixFiles.UndoManager.BeginNewCommandRange();

            MenuItem menuItem = sender as MenuItem;
            if (menuItem != null) {
                TreeNode newNode = CreateNewSubElement(menuItem.Text);
                ShowNode(newNode.Tag as XmlElement);
                ShowProperties(newNode.Tag as XmlElement);
            }
        }

        private void DeleteElement_Click(object sender, System.EventArgs e) {
            if (currTreeView.SelectedNode == null) {
                return;
            }

            XmlNode node = currTreeView.SelectedNode.Tag as XmlNode;
            if (node == null) {
                return;
            }

            WixFiles.UndoManager.BeginNewCommandRange();

            node.ParentNode.RemoveChild(node);
            currTreeView.Nodes.Remove(currTreeView.SelectedNode);
            if (currTreeView.SelectedNode == null) {
                ShowProperties(null);
            } else {
                ShowProperties(currTreeView.SelectedNode.Tag as XmlNode);
            }
        }

        private void InfoAboutCurrentElement_Click(object sender, System.EventArgs e) {
            XmlNode xmlNode = (XmlNode) currTreeView.SelectedNode.Tag;
            XmlDocumentationManager docManager = new XmlDocumentationManager(WixFiles);
            XmlAttributeAdapter attAdapter = (XmlAttributeAdapter) CurrentGrid.SelectedObject;

            string title = String.Format("Info about '{0}' element", xmlNode.Name);
            string message = docManager.GetDocumentation(attAdapter.XmlNodeDefinition);

            MessageBox.Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}