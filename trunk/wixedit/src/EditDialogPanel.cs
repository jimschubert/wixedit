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
using System.Text;
using System.IO;
using System.Resources;
using System.Reflection;

using WixEdit.PropertyGridExtensions;
 
namespace WixEdit {
    /// <summary>
    /// Editing of dialogs.
    /// </summary>
    public class EditDialogPanel : BasePanel {
        #region Controls
        
        private DesignerForm currentDialog;
        private TreeView dialogTreeView;
        private PropertyGrid propertyGrid;
        private ContextMenu wxsDialogsContextMenu;
        private ContextMenu propertyGridContextMenu;
        private ContextMenu dialogTreeViewContextMenu;
        private ListView wxsDialogs;
        private Splitter splitter1;
        private Splitter splitter2;
        private Panel panel1;
        private IconMenuItem viewMenu;
        private IconMenuItem Opacity100;
        private IconMenuItem Opacity75;
        private IconMenuItem Opacity50;
        private IconMenuItem Opacity25;
        private IconMenuItem Separator;
        private IconMenuItem AlwaysOnTop;

        private IconMenuItem newControlElementMenu;
        private IconMenuItem newControlSubElementsMenu;
        private IconMenuItem newTextElementMenu;
        private IconMenuItem newPublishElementMenu;
        private IconMenuItem newConditionElementMenu;
        private IconMenuItem newSubscribeElementMenu;
        private IconMenuItem deleteCurrentElementMenu;

        private IconMenuItem infoAboutCurrentElementMenu;
        #endregion

        public EditDialogPanel(WixFiles wixFiles) : base(wixFiles) {
            InitializeComponent();
        }

        private void OnResizeWxsDialogs(object sender, System.EventArgs e) {
            if (this.wxsDialogs.Columns[0] != null) {
                this.wxsDialogs.Columns[0].Width = this.wxsDialogs.ClientSize.Width - 4;
            }
        }

        public override MenuItem Menu {
            get {
                return viewMenu;
            }
        }

        #region Initialize Controls
        private void InitializeComponent() {
            this.viewMenu = new IconMenuItem();
            this.Opacity100 = new IconMenuItem();
            this.Opacity75 = new IconMenuItem();
            this.Opacity50 = new IconMenuItem();
            this.Opacity25 = new IconMenuItem();
            this.Separator = new IconMenuItem("-");
            this.AlwaysOnTop = new IconMenuItem();
            this.dialogTreeView = new TreeView();
            this.propertyGrid = new CustomPropertyGrid();
            this.propertyGridContextMenu = new ContextMenu();
            this.wxsDialogs = new ListView();
            this.wxsDialogsContextMenu = new ContextMenu();
            this.splitter1 = new Splitter();
            this.splitter2 = new Splitter();
            this.panel1 = new Panel();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            
            this.viewMenu.MenuItems.AddRange(new MenuItem[] {
                                                              this.Opacity100,
                                                              this.Opacity75,
                                                              this.Opacity50,
                                                              this.Opacity25,
                                                              this.Separator,
                                                              this.AlwaysOnTop});
            this.viewMenu.Text = "&Dialogs";
            // 
            // Opacity100
            // 
            this.Opacity100.Index = 0;
            this.Opacity100.Text = "Set Opacity 100%";
            this.Opacity100.Click += new System.EventHandler(this.Opacity_Click);
            // 
            // Opacity75
            // 
            this.Opacity75.Index = 1;
            this.Opacity75.Text = "Set Opacity 75%";
            this.Opacity75.Click += new System.EventHandler(this.Opacity_Click);
            // 
            // Opacity50
            // 
            this.Opacity50.Index = 2;
            this.Opacity50.Text = "Set Opacity 50%";
            this.Opacity50.Click += new System.EventHandler(this.Opacity_Click);
            // 
            // Opacity25
            // 
            this.Opacity25.Index = 3;
            this.Opacity25.Text = "Set Opacity 25%";
            this.Opacity25.Click += new System.EventHandler(this.Opacity_Click);
            //
            // Separator
            //
            this.Separator.Index = 4;
            // 
            // AlwaysOnTop
            // 
            this.AlwaysOnTop.Index = 5;
            this.AlwaysOnTop.Text = "Always on top";
            this.AlwaysOnTop.Click += new System.EventHandler(this.AlwaysOnTop_Click);
            // 
            // dialogTreeView
            // 
            this.dialogTreeView.Dock = DockStyle.Left;
            this.dialogTreeView.ImageIndex = -1;
            this.dialogTreeView.Location = new Point(0, 0);
            this.dialogTreeView.Name = "dialogTreeView";
            this.dialogTreeView.SelectedImageIndex = -1;
            this.dialogTreeView.Size = new Size(170, 266);
            this.dialogTreeView.TabIndex = 6;
            this.dialogTreeView.AfterSelect += new TreeViewEventHandler(this.OnAfterSelect);
            this.dialogTreeViewContextMenu = new ContextMenu();
            this.dialogTreeViewContextMenu.Popup += new EventHandler(PopupDialogTreeViewContextMenu);
            this.dialogTreeView.MouseDown += new MouseEventHandler(TreeViewMouseDown);

            this.dialogTreeView.ImageList = GetDialogTreeViewImageList();

            this.newControlElementMenu = new IconMenuItem("New Control", new Bitmap(WixFiles.GetResourceStream("WixEdit.control.bmp")));
            this.newControlElementMenu.Click += new System.EventHandler(this.NewControlElement_Click);


            this.newControlSubElementsMenu = new IconMenuItem("New", new Bitmap(WixFiles.GetResourceStream("WixEdit.new.bmp")));

            this.newTextElementMenu = new IconMenuItem("Text", new Bitmap(WixFiles.GetResourceStream("WixEdit.text.bmp")));
            this.newTextElementMenu.Click += new System.EventHandler(this.NewTextElement_Click);

            this.newPublishElementMenu = new IconMenuItem("Publish", new Bitmap(WixFiles.GetResourceStream("WixEdit.publish.bmp")));
            this.newPublishElementMenu.Click += new System.EventHandler(this.NewPublishElement_Click);

            this.newConditionElementMenu = new IconMenuItem("Condition", new Bitmap(WixFiles.GetResourceStream("WixEdit.condition.bmp")));
            this.newConditionElementMenu.Click += new System.EventHandler(this.NewConditionElement_Click);

            this.newSubscribeElementMenu = new IconMenuItem("Subsribe", new Bitmap(WixFiles.GetResourceStream("WixEdit.subscribe.bmp")));
            this.newSubscribeElementMenu.Click += new System.EventHandler(this.NewSubscribeElement_Click);

            this.deleteCurrentElementMenu = new IconMenuItem("&Delete", new Bitmap(WixFiles.GetResourceStream("WixEdit.delete.bmp")));
            this.deleteCurrentElementMenu.Click += new System.EventHandler(this.DeleteElement_Click);

            this.infoAboutCurrentElementMenu = new IconMenuItem("&Info", new Bitmap(WixFiles.GetResourceStream("WixEdit.info.bmp")));
            this.infoAboutCurrentElementMenu.Click += new System.EventHandler(this.InfoAboutCurrentElement_Click);

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
            this.propertyGrid.PropertyValueChanged += new PropertyValueChangedEventHandler(OnPropertyValueChanged);
            this.propertyGrid.ContextMenu = this.propertyGridContextMenu;

            // 
            // wxsDialogs
            // 
            this.wxsDialogs.Dock = DockStyle.Left;
            this.wxsDialogs.Font = new Font("Tahoma", 8.25F, FontStyle.Regular, GraphicsUnit.Point, ((System.Byte)(0)));
            this.wxsDialogs.Location = new Point(0, 0);
            this.wxsDialogs.Name = "wxsDialogs";
            this.wxsDialogs.Size = new Size(140, 264);
            this.wxsDialogs.TabIndex = 0;
            this.wxsDialogs.View = View.Details;
            this.wxsDialogs.MultiSelect = false;
            this.wxsDialogs.HideSelection = false;
            this.wxsDialogs.FullRowSelect = true;
            this.wxsDialogs.GridLines = false;
            this.wxsDialogs.SelectedIndexChanged += new System.EventHandler(this.OnSelectedDialogChanged);
            this.wxsDialogs.ContextMenu = this.wxsDialogsContextMenu;

            this.wxsDialogsContextMenu.Popup += new EventHandler(OnWxsDialogsPopupContextMenu);
            // 
            // splitter1
            // 
            this.splitter1.Location = new Point(140, 0);
            this.splitter1.Name = "splitter1";
            this.splitter1.Size = new Size(2, 266);
            this.splitter1.TabIndex = 7;
            this.splitter1.TabStop = false;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.splitter2);
            this.panel1.Controls.Add(this.propertyGrid);
            this.panel1.Controls.Add(this.dialogTreeView);
            this.panel1.Dock = DockStyle.Fill;
            this.panel1.Location = new Point(142, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new Size(409, 266);
            this.panel1.TabIndex = 9;
            // 
            // splitter2
            // 
            this.splitter2.Location = new Point(140, 0);
            this.splitter2.Name = "splitter2";
            this.splitter2.Size = new Size(2, 266);
            this.splitter2.TabIndex = 7;
            this.splitter2.TabStop = false;
            // 
            // EditorForm
            // 
            //this.AutoScaleBaseSize = new Size(5, 14);
            this.ClientSize = new Size(553, 266);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.splitter1);
            this.Controls.Add(this.wxsDialogs);
            this.Font = new Font("Tahoma", 8.25F, FontStyle.Regular, GraphicsUnit.Point, ((System.Byte)(0)));
            
            this.Name = "EditorForm";
            this.Text = "Wix Dialog Editor";
            this.panel1.ResumeLayout(false);
            this.ResumeLayout(false);
           
            Opacity100.Checked = true;

            this.wxsDialogs.Items.Clear();

            XmlNodeList dialogs = wixFiles.WxsDocument.SelectNodes("/wix:Wix/*/wix:UI/wix:Dialog", wixFiles.WxsNsmgr);
            foreach (XmlNode dialog in dialogs) {
                XmlAttribute attr = dialog.Attributes["Id"];
                if (attr != null) {
                    this.wxsDialogs.Items.Add(new ListViewItem(attr.Value));
                }
            }

            this.wxsDialogs.Columns.Add("Item Column", -2, HorizontalAlignment.Left);
            this.wxsDialogs.HeaderStyle = ColumnHeaderStyle.None;

            this.wxsDialogs.Resize += new EventHandler(OnResizeWxsDialogs);
        }

        #endregion

        private ImageList GetDialogTreeViewImageList() {
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

        public void OnPropertyValueChanged(object s, PropertyValueChangedEventArgs e) {
            // TODO: Good place to keep track of command, for undo functionality

            string currentDialogId = wxsDialogs.SelectedItems[0].Text;
            XmlNode dialog = wixFiles.WxsDocument.SelectSingleNode(String.Format("/wix:Wix/*/wix:UI/wix:Dialog[@Id='{0}']", currentDialogId), wixFiles.WxsNsmgr);
            
            ShowWixDialog(dialog);
        }

        public void OnWxsDialogsPopupContextMenu(object sender, EventArgs e) {
            MenuItem menuItem1 = new IconMenuItem("&New Dialog", new Bitmap(WixFiles.GetResourceStream("WixEdit.new.bmp")));
            MenuItem menuItem2 = new IconMenuItem("&Delete", new Bitmap(WixFiles.GetResourceStream("WixEdit.delete.bmp")));
            
            menuItem1.Click += new EventHandler(OnNewWxsDialogsItem);
            menuItem2.Click += new EventHandler(OnDeleteWxsDialogsItem);

            wxsDialogsContextMenu.MenuItems.Clear();

            wxsDialogsContextMenu.MenuItems.Add(menuItem1);

            if (wxsDialogs.SelectedItems.Count > 0 && wxsDialogs.SelectedItems[0] != null) {
                wxsDialogsContextMenu.MenuItems.Add(menuItem2);
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

        public void OnNewWxsDialogsItem(object sender, EventArgs e) {
            EnterStringForm frm = new EnterStringForm();
            frm.Text = "Enter new Dialog name";
            if (DialogResult.OK == frm.ShowDialog()) {
                XmlNode dialog = wixFiles.WxsDocument.CreateElement("Dialog", "http://schemas.microsoft.com/wix/2003/01/wi");
                XmlAttribute att = wixFiles.WxsDocument.CreateAttribute("Id");
                att.Value = frm.SelectedString;
                dialog.Attributes.Append(att);
                
                XmlNode ui = wixFiles.WxsDocument.SelectSingleNode("/wix:Wix/*/wix:UI", wixFiles.WxsNsmgr);
                ui.AppendChild(dialog);

                ListViewItem item = new ListViewItem(frm.SelectedString);
                this.wxsDialogs.Items.Add(item);

                item.Selected = true;
                item.Focused = true;
                item.EnsureVisible();

                wxsDialogs.Focus();
            }
        }

        public void OnDeleteWxsDialogsItem(object sender, EventArgs e) {
            if (wxsDialogs.SelectedItems.Count > 0 && wxsDialogs.SelectedItems[0] != null) {
                string currentDialogId = wxsDialogs.SelectedItems[0].Text;
                XmlNode dialog = wixFiles.WxsDocument.SelectSingleNode(String.Format("/wix:Wix/*/wix:UI/wix:Dialog[@Id='{0}']", currentDialogId), wixFiles.WxsNsmgr);

                dialog.ParentNode.RemoveChild(dialog);

                int currentIndex = wxsDialogs.SelectedItems[0].Index;
                wxsDialogs.Items.Remove(wxsDialogs.SelectedItems[0]);

                if (currentIndex >= wxsDialogs.Items.Count) {
                    currentIndex--;
                }

                if (currentIndex < 0) {               
                    ShowWixDialog(null);
                    ShowWixDialogTree(null);
                    ShowWixProperties(null);
                } else {
                    wxsDialogs.Items[currentIndex].Selected = true;
                    wxsDialogs.Focus();
                }
            }
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

        public void OnToggleDescriptionPropertyGrid(object sender, EventArgs e) {
            propertyGrid.HelpVisible = !propertyGrid.HelpVisible;
        }

        public void OnDeletePropertyGridItem(object sender, EventArgs e) {
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

        private void OnSelectedDialogChanged(object sender, System.EventArgs e) {
            if (wxsDialogs.SelectedItems.Count > 0 && wxsDialogs.SelectedItems[0] != null) {
                string currentDialogId = wxsDialogs.SelectedItems[0].Text;
                XmlNode dialog = wixFiles.WxsDocument.SelectSingleNode(String.Format("/wix:Wix/*/wix:UI/wix:Dialog[@Id='{0}']", currentDialogId), wixFiles.WxsNsmgr);
                
                ShowWixDialog(dialog);
                ShowWixDialogTree(dialog);
                ShowWixProperties(dialog);
            }
        }

        private void ShowWixDialog(XmlNode dialog) {
            DesignerForm prevDialog = null;
            int prevTop = 0;
            int prevLeft = 0;

            if (currentDialog != null) {
                prevTop = currentDialog.Top;
                prevLeft = currentDialog.Left;
                prevDialog = currentDialog;
            } else {
                prevTop = this.TopLevelControl.Top;
                prevLeft = this.TopLevelControl.Right;
            }

            if (dialog != null) {
                DialogGenerator generator = new DialogGenerator(wixFiles);
                currentDialog = generator.GenerateDialog(dialog, this);
    
                if (currentDialog != null) {
                    currentDialog.Left = prevLeft;
                    currentDialog.Top = prevTop;
        
                    currentDialog.Opacity = GetOpacity();
                    currentDialog.TopMost = this.AlwaysOnTop.Checked;
        
                    currentDialog.Show();
                }
            }
            if (prevDialog != null) {
                prevDialog.Hide();
                prevDialog.Dispose();
            }

            this.Focus();
        }

        private void ShowWixDialogTree(XmlNode dialog) {
            dialogTreeView.Nodes.Clear();

            if (dialog != null) {
                TreeNode rootNode = new TreeNode("Dialog");
                rootNode.Tag = dialog;
                rootNode.ImageIndex = 1;
                rootNode.SelectedImageIndex = 1;
    
                dialogTreeView.Nodes.Add(rootNode);
    
                foreach (XmlNode control in dialog.ChildNodes) {
                    AddControlTreeItems(rootNode, control);
                }
    
                dialogTreeView.ExpandAll();
                dialogTreeView.SelectedNode = rootNode;
            }
        }

        private void AddControlTreeItems(TreeNode parent, XmlNode xmlNodeToAdd) {
            string treeNodeName = xmlNodeToAdd.Name;
            if (xmlNodeToAdd.Attributes != null && xmlNodeToAdd.Attributes["Id"] != null) {
                treeNodeName = xmlNodeToAdd.Attributes["Id"].Value;
            }

            TreeNode control = new TreeNode(treeNodeName);
            control.Tag = xmlNodeToAdd;
            control.ImageIndex = 2;
            control.SelectedImageIndex = 2;
            parent.Nodes.Add(control);

            foreach (XmlNode xmlChildNode in xmlNodeToAdd.ChildNodes) {
                AddControlSubTreeItems(control, xmlChildNode);
            }
        }

        private void AddControlSubTreeItems(TreeNode parent, XmlNode xmlNodeToAdd) {
            string treeNodeName = xmlNodeToAdd.Name;
            if (xmlNodeToAdd.Attributes != null && xmlNodeToAdd.Attributes["Id"] != null) {
                treeNodeName = xmlNodeToAdd.Attributes["Id"].Value;
            }

            TreeNode child = new TreeNode(treeNodeName);
            switch (treeNodeName) {
                case "Text":
                    child.ImageIndex = 3;
                    child.SelectedImageIndex = 3;
                    break;
                case "Condition":
                    child.ImageIndex = 4;
                    child.SelectedImageIndex = 4;
                    break;
                case "Subscribe":
                    child.ImageIndex = 5;
                    child.SelectedImageIndex = 5;
                    break;
                case "Publish":
                    child.ImageIndex = 6;
                    child.SelectedImageIndex = 6;
                    break;
                default:
                    child.ImageIndex = 0;
                    child.SelectedImageIndex = 0;
                    break;
            }

            child.Tag = xmlNodeToAdd;
            parent.Nodes.Add(child);
        }

        private void ShowWixProperties(XmlNode xmlNode) {
            XmlAttributeAdapter attAdapter = null;
            if (xmlNode != null) {
                attAdapter = new XmlAttributeAdapter(xmlNode, wixFiles);
            }

            propertyGrid.SelectedObject = attAdapter;
            propertyGrid.Update();

            return;
        }


        private void OnAfterSelect(object sender, TreeViewEventArgs e) {
            XmlNode node = e.Node.Tag as XmlNode;
            if (node != null) {
                ShowWixProperties(node);
                currentDialog.SelectedNode = node;
            }
        }

        private void TreeViewMouseDown(object sender, System.Windows.Forms.MouseEventArgs e) {
            if (e.Button == MouseButtons.Right) {
                TreeNode node = dialogTreeView.GetNodeAt(e.X, e.Y);
                if (node == null) {
                    return;
                }
                dialogTreeView.SelectedNode = node;

                Point spot = this.PointToClient(dialogTreeView.PointToScreen(new Point(e.X,e.Y)));
                dialogTreeViewContextMenu.Show(this, spot);
            }
        }

        protected void PopupDialogTreeViewContextMenu(System.Object sender, System.EventArgs e) {
            XmlNode node = dialogTreeView.SelectedNode.Tag as XmlNode;
            if (node == null) {
                return;
            }

            dialogTreeViewContextMenu.MenuItems.Clear();

            switch (node.Name) {
                case "Dialog":
                    dialogTreeViewContextMenu.MenuItems.Add(this.newControlElementMenu);
                    break;
                case "Control":
                    this.newControlSubElementsMenu.MenuItems.Clear();
                    dialogTreeViewContextMenu.MenuItems.Add(this.newControlSubElementsMenu);
                    this.newControlSubElementsMenu.MenuItems.Add(this.newTextElementMenu);
                    this.newControlSubElementsMenu.MenuItems.Add(this.newPublishElementMenu);
                    this.newControlSubElementsMenu.MenuItems.Add(this.newConditionElementMenu);
                    this.newControlSubElementsMenu.MenuItems.Add(this.newSubscribeElementMenu);
                    break;
                default:
                    break;
            }

            dialogTreeViewContextMenu.MenuItems.Add(this.deleteCurrentElementMenu);


            XmlAttributeAdapter attAdapter = (XmlAttributeAdapter) propertyGrid.SelectedObject;

            XmlDocumentationManager docManager = new XmlDocumentationManager(wixFiles);
            if (docManager.HasDocumentation(attAdapter.XmlNodeDefinition)) {
                dialogTreeViewContextMenu.MenuItems.Add(new IconMenuItem("-"));
                dialogTreeViewContextMenu.MenuItems.Add(this.infoAboutCurrentElementMenu);
            }
        }

        private void OnPropertyDoubleClick(object sender, System.EventArgs e) {
            // Edit here?
        }

        private void NewControlElement_Click(object sender, System.EventArgs e) {
            XmlNode node = dialogTreeView.SelectedNode.Tag as XmlNode;
            if (node == null) {
                return;
            }

            // Name should be dialog...
            if (node.Name == "Dialog") {
                // Get new name, and add control
                EnterStringForm frm = new EnterStringForm();
                frm.Text = "Enter new Control name";
                if (DialogResult.OK == frm.ShowDialog()) {
                    XmlElement newControl = node.OwnerDocument.CreateElement("Control", "http://schemas.microsoft.com/wix/2003/01/wi");
    
                    XmlAttribute newAttr = node.OwnerDocument.CreateAttribute("Id");
                    newAttr.Value = frm.SelectedString;

                    newControl.Attributes.Append(newAttr);

                    node.AppendChild(newControl);

                    TreeNode control = new TreeNode(frm.SelectedString);
                    control.Tag = newControl;
                    control.ImageIndex = 2;
                    control.SelectedImageIndex = 2;

                    dialogTreeView.TopNode.Nodes.Add(control);
                    dialogTreeView.SelectedNode = control;

                    ShowWixProperties(newControl);
                }
            }
        }

        private void CreateNewControlSubElement(string typeName, int imageIndex) {
            XmlNode node = dialogTreeView.SelectedNode.Tag as XmlNode;
            if (node == null) {
                return;
            }

            // Name should be dialog...
            if (node.Name == "Control") {
                // Get new name, and add Text element
                // EnterStringForm frm = new EnterStringForm();
                // frm.Text = "Enter new Text value";
                // if (DialogResult.OK == frm.ShowDialog()) {
                    XmlElement newText = node.OwnerDocument.CreateElement(typeName, "http://schemas.microsoft.com/wix/2003/01/wi");
    
                    // XmlText newValue = node.OwnerDocument.CreateTextNode(frm.SelectedString);

                    // newText.AppendChild(newValue);

                    // node.AppendChild(newText);

                    TreeNode control = new TreeNode(typeName);
                    control.Tag = newText;
                    control.ImageIndex = imageIndex;
                    control.SelectedImageIndex = imageIndex;

                    dialogTreeView.SelectedNode.Nodes.Add(control);
                    dialogTreeView.SelectedNode = control;

                    ShowWixProperties(newText);
                //}
            }
        }

        private void NewTextElement_Click(object sender, System.EventArgs e) {
            CreateNewControlSubElement("Text", 3);
        }

        private void NewConditionElement_Click(object sender, System.EventArgs e) {
            CreateNewControlSubElement("Condition", 4);
        }

        private void NewSubscribeElement_Click(object sender, System.EventArgs e) {
            CreateNewControlSubElement("Subscribe", 5);
        }

        private void NewPublishElement_Click(object sender, System.EventArgs e) {
            CreateNewControlSubElement("Publish", 6);
        }

        private void DeleteElement_Click(object sender, System.EventArgs e) {
            XmlNode node = dialogTreeView.SelectedNode.Tag as XmlNode;
            if (node == null) {
                return;
            }

            node.ParentNode.RemoveChild(node);

            dialogTreeView.Nodes.Remove(dialogTreeView.SelectedNode);

            ShowWixProperties(dialogTreeView.SelectedNode.Tag as XmlNode);
        }

        private void InfoAboutCurrentElement_Click(object sender, System.EventArgs e) {
            XmlNode xmlNode = (XmlNode) dialogTreeView.SelectedNode.Tag;

            XmlDocumentationManager docManager = new XmlDocumentationManager(wixFiles);
            XmlAttributeAdapter attAdapter = (XmlAttributeAdapter) propertyGrid.SelectedObject;

            string title = String.Format("Info about '{0}' element", xmlNode.Name);            
            string message = docManager.GetDocumentation(attAdapter.XmlNodeDefinition);

            MessageBox.Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void Opacity_Click(object sender, System.EventArgs e) {
            UncheckOpacityMenu();

            MenuItem item = sender as MenuItem;
            if (item != null) {
                item.Checked = true;
            }

            if (currentDialog != null) {
                currentDialog.Opacity = GetOpacity();
            }
        }

        private void AlwaysOnTop_Click(object sender, System.EventArgs e) {
            this.AlwaysOnTop.Checked = !this.AlwaysOnTop.Checked;

            if (currentDialog != null) {
                currentDialog.TopMost = this.AlwaysOnTop.Checked;
            }
        }

        private void UncheckOpacityMenu() {
            Opacity100.Checked = false;
            Opacity75.Checked = false;
            Opacity50.Checked = false;
            Opacity25.Checked = false;
        }

        private double GetOpacity() {
            if (Opacity100.Checked) {
                return 1.00;
            } else if (Opacity75.Checked) {
                return 0.75;
            } else if (Opacity50.Checked) {
                return 0.50;
            } else if (Opacity25.Checked) {
                return 0.25;
            }

            return 1.00;
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose( bool disposing ) {
            if( disposing ) {
                if (currentDialog != null) {
                    currentDialog.Hide();
                    currentDialog.Dispose();
                }
            }

            base.Dispose( disposing );
        }
    }
}
