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
using WixEdit.Settings;
 
namespace WixEdit {
    /// <summary>
    /// Editing of dialogs.
    /// </summary>
    public class EditDialogPanel : DisplayBasePanel {
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
        private IconMenuItem SnapToGrid;
        private IconMenuItem DialogScale;

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
            if (wxsDialogs.Columns.Count > 0 && wxsDialogs.Columns[0] != null) {
                wxsDialogs.Columns[0].Width = wxsDialogs.ClientSize.Width - 4;
            }
        }

        public override MenuItem Menu {
            get {
                return viewMenu;
            }
        }

        #region Initialize Controls
        private void InitializeComponent() {
            viewMenu = new IconMenuItem();
            Opacity100 = new IconMenuItem();
            Opacity75 = new IconMenuItem();
            Opacity50 = new IconMenuItem();
            Opacity25 = new IconMenuItem();
            Separator = new IconMenuItem("-");
            AlwaysOnTop = new IconMenuItem();
            SnapToGrid = new IconMenuItem();
            DialogScale = new IconMenuItem();
            dialogTreeView = new TreeView();
            propertyGrid = new CustomPropertyGrid();
            propertyGridContextMenu = new ContextMenu();
            wxsDialogs = new ListView();
            wxsDialogsContextMenu = new ContextMenu();
            splitter1 = new Splitter();
            splitter2 = new Splitter();
            panel1 = new Panel();
            panel1.SuspendLayout();
            SuspendLayout();
            
            viewMenu.MenuItems.AddRange(new MenuItem[] {
                                                              Opacity100,
                                                              Opacity75,
                                                              Opacity50,
                                                              Opacity25,
                                                              Separator,
                                                              AlwaysOnTop,
                                                              SnapToGrid,
                                                              DialogScale});
            viewMenu.Text = "&Dialogs";
            // 
            // Opacity100
            // 
            Opacity100.Index = 0;
            Opacity100.Text = "Set Opacity 100%";
            Opacity100.Click += new System.EventHandler(Opacity_Click);
            // 
            // Opacity75
            // 
            Opacity75.Index = 1;
            Opacity75.Text = "Set Opacity 75%";
            Opacity75.Click += new System.EventHandler(Opacity_Click);
            // 
            // Opacity50
            // 
            Opacity50.Index = 2;
            Opacity50.Text = "Set Opacity 50%";
            Opacity50.Click += new System.EventHandler(Opacity_Click);
            // 
            // Opacity25
            // 
            Opacity25.Index = 3;
            Opacity25.Text = "Set Opacity 25%";
            Opacity25.Click += new System.EventHandler(Opacity_Click);
            //
            // Separator
            //
            Separator.Index = 4;
            // 
            // AlwaysOnTop
            // 
            AlwaysOnTop.Index = 5;
            AlwaysOnTop.Text = "Always on top";
            AlwaysOnTop.Click += new System.EventHandler(AlwaysOnTop_Click);
            AlwaysOnTop.Checked = WixEditSettings.Instance.AlwaysOnTop;
            // 
            // SnapToGrid
            // 
            SnapToGrid.Index = 6;
            SnapToGrid.Text = "Snap to grid";
            SnapToGrid.Click += new System.EventHandler(SnapToGrid_Click);
            // 
            // Scale
            // 
            DialogScale.Index = 6;
            DialogScale.Text = "Scale Dialog";
            DialogScale.Click += new System.EventHandler(DialogScale_Click);
            // 
            // dialogTreeView
            // 
            dialogTreeView.Dock = DockStyle.Left;
            dialogTreeView.ImageIndex = -1;
            dialogTreeView.Location = new Point(0, 0);
            dialogTreeView.Name = "dialogTreeView";
            dialogTreeView.SelectedImageIndex = -1;
            dialogTreeView.Size = new Size(170, 266);
            dialogTreeView.TabIndex = 6;
            dialogTreeView.AfterSelect += new TreeViewEventHandler(OnAfterSelect);
            dialogTreeViewContextMenu = new ContextMenu();
            dialogTreeViewContextMenu.Popup += new EventHandler(PopupDialogTreeViewContextMenu);
            dialogTreeView.MouseDown += new MouseEventHandler(TreeViewMouseDown);

            dialogTreeView.ImageList = GetDialogTreeViewImageList();

            newControlElementMenu = new IconMenuItem("New Control", new Bitmap(WixFiles.GetResourceStream("elements.control.bmp")));
            newControlElementMenu.Click += new System.EventHandler(NewControlElement_Click);


            newControlSubElementsMenu = new IconMenuItem("New", new Bitmap(WixFiles.GetResourceStream("bmp.new.bmp")));

            newTextElementMenu = new IconMenuItem("Text", new Bitmap(WixFiles.GetResourceStream("elements.text.bmp")));
            newTextElementMenu.Click += new System.EventHandler(NewTextElement_Click);

            newPublishElementMenu = new IconMenuItem("Publish", new Bitmap(WixFiles.GetResourceStream("elements.publish.bmp")));
            newPublishElementMenu.Click += new System.EventHandler(NewPublishElement_Click);

            newConditionElementMenu = new IconMenuItem("Condition", new Bitmap(WixFiles.GetResourceStream("elements.condition.bmp")));
            newConditionElementMenu.Click += new System.EventHandler(NewConditionElement_Click);

            newSubscribeElementMenu = new IconMenuItem("Subsribe", new Bitmap(WixFiles.GetResourceStream("elements.subscribe.bmp")));
            newSubscribeElementMenu.Click += new System.EventHandler(NewSubscribeElement_Click);

            deleteCurrentElementMenu = new IconMenuItem("&Delete", new Bitmap(WixFiles.GetResourceStream("bmp.delete.bmp")));
            deleteCurrentElementMenu.Click += new System.EventHandler(DeleteElement_Click);

            infoAboutCurrentElementMenu = new IconMenuItem("&Info", new Bitmap(WixFiles.GetResourceStream("bmp.info.bmp")));
            infoAboutCurrentElementMenu.Click += new System.EventHandler(InfoAboutCurrentElement_Click);

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
            // wxsDialogs
            // 
            wxsDialogs.Dock = DockStyle.Left;
            wxsDialogs.Font = new Font("Tahoma", 8.25F, FontStyle.Regular, GraphicsUnit.Point, ((System.Byte)(0)));
            wxsDialogs.Location = new Point(0, 0);
            wxsDialogs.Name = "wxsDialogs";
            wxsDialogs.Size = new Size(140, 264);
            wxsDialogs.TabIndex = 0;
            wxsDialogs.View = View.Details;
            wxsDialogs.MultiSelect = false;
            wxsDialogs.HideSelection = false;
            wxsDialogs.FullRowSelect = true;
            wxsDialogs.GridLines = false;
            wxsDialogs.SelectedIndexChanged += new System.EventHandler(OnSelectedDialogChanged);
            wxsDialogs.ContextMenu = wxsDialogsContextMenu;

            wxsDialogsContextMenu.Popup += new EventHandler(OnWxsDialogsPopupContextMenu);
            // 
            // splitter1
            // 
            splitter1.Location = new Point(140, 0);
            splitter1.Name = "splitter1";
            splitter1.Size = new Size(2, 266);
            splitter1.TabIndex = 7;
            splitter1.TabStop = false;
            // 
            // panel1
            // 
            panel1.Controls.Add(splitter2);
            panel1.Controls.Add(propertyGrid);
            panel1.Controls.Add(dialogTreeView);
            panel1.Dock = DockStyle.Fill;
            panel1.Location = new Point(142, 0);
            panel1.Name = "panel1";
            panel1.Size = new Size(409, 266);
            panel1.TabIndex = 9;
            // 
            // splitter2
            // 
            splitter2.Location = new Point(140, 0);
            splitter2.Name = "splitter2";
            splitter2.Size = new Size(2, 266);
            splitter2.TabIndex = 7;
            splitter2.TabStop = false;
            // 
            // EditorForm
            // 
            //AutoScaleBaseSize = new Size(5, 14);
            ClientSize = new Size(553, 266);
            Controls.Add(panel1);
            Controls.Add(splitter1);
            Controls.Add(wxsDialogs);
            Font = new Font("Tahoma", 8.25F, FontStyle.Regular, GraphicsUnit.Point, ((System.Byte)(0)));
            
            Name = "EditorForm";
            Text = "Wix Dialog Editor";
            panel1.ResumeLayout(false);
            ResumeLayout(false);
           
            double opacity = WixEditSettings.Instance.Opacity;
            if (opacity == 1.00) {
                Opacity100.Checked = true;
            } else if (opacity == 0.75) {
                Opacity75.Checked = true;
            } else if (opacity == 0.50) {
                Opacity50.Checked = true;
            } else if (opacity == 0.25) {
                Opacity25.Checked = true;
            } else {
                Opacity100.Checked = true;
            }

            wxsDialogs.Items.Clear();

            XmlNodeList dialogs = wixFiles.WxsDocument.SelectNodes("/wix:Wix/*/wix:UI/wix:Dialog", wixFiles.WxsNsmgr);
            foreach (XmlNode dialog in dialogs) {
                XmlAttribute attr = dialog.Attributes["Id"];
                if (attr != null) {
                    ListViewItem toAdd = new ListViewItem(attr.Value);
                    toAdd.Tag = dialog;

                    wxsDialogs.Items.Add(toAdd);
                }
            }

            wxsDialogs.Columns.Add("Item Column", -2, HorizontalAlignment.Left);
            wxsDialogs.HeaderStyle = ColumnHeaderStyle.None;

            wxsDialogs.Resize += new EventHandler(OnResizeWxsDialogs);
        }

        #endregion

        #region DisplayBasePanel overrides and helpers
        public override bool IsOwnerOfNode(XmlNode node) {
            XmlNodeList dialogs = wixFiles.WxsDocument.SelectNodes("/wix:Wix/*/wix:UI/wix:Dialog", wixFiles.WxsNsmgr);
            return FindNode(GetShowableNode(node), dialogs);
        }

        private XmlNode GetShowableNode(XmlNode node) {
            XmlNode showableNode = node;
            while (showableNode.NodeType != XmlNodeType.Element) {
                if (showableNode.NodeType == XmlNodeType.Attribute) {
                    showableNode = ((XmlAttribute) showableNode).OwnerElement;
                } else {
                    showableNode = showableNode.ParentNode;
                }
            }

            return showableNode;
        }

        private bool FindNode(XmlNode nodeToFind, IEnumerable xmlNodes) {
            foreach (XmlNode node in xmlNodes) {
                if (node == nodeToFind) {
                    return true;
                }

                if (FindNode(nodeToFind, node.ChildNodes)) {
                    return true;
                }
            }

            return false;
        }

        public override void ShowNode(XmlNode node) {
            XmlNode showable = GetShowableNode(node);

            XmlNode dialog = showable;
            while (dialog.Name != "Dialog") {
                dialog = dialog.ParentNode;
            }

            foreach (ListViewItem item in wxsDialogs.Items) {
                if (dialog == item.Tag) {
                    item.Selected = true;

                    ShowWixDialog(null);  
                    ShowWixDialogTree(null);
                    ShowWixProperties(null);
          
                    ShowWixDialog(dialog);
                    ShowWixDialogTree(dialog);
                    ShowWixProperties(dialog);


                    break;
                }
            }

            TreeNode treeNode = FindTreeNode(showable, dialogTreeView.Nodes);
            if (treeNode != null) {
                dialogTreeView.SelectedNode = null;
                dialogTreeView.SelectedNode = treeNode;
            }
        }

        private TreeNode FindTreeNode(XmlNode node, TreeNodeCollection treeNodes) {
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
        #endregion

        private ImageList GetDialogTreeViewImageList() {
            ImageList images = new ImageList(); 

            Bitmap bmp = new Bitmap(WixFiles.GetResourceStream("elements.empty.bmp"));
            bmp.MakeTransparent();
            images.Images.Add(bmp);

            bmp = new Bitmap(WixFiles.GetResourceStream("elements.dialog.bmp"));
            bmp.MakeTransparent();
            images.Images.Add(bmp);

            bmp = new Bitmap(WixFiles.GetResourceStream("elements.control.bmp"));
            bmp.MakeTransparent();
            images.Images.Add(bmp);

            bmp = new Bitmap(WixFiles.GetResourceStream("elements.text.bmp"));
            bmp.MakeTransparent();
            images.Images.Add(bmp);

            bmp = new Bitmap(WixFiles.GetResourceStream("elements.condition.bmp"));
            bmp.MakeTransparent();
            images.Images.Add(bmp);

            bmp = new Bitmap(WixFiles.GetResourceStream("elements.subscribe.bmp"));
            bmp.MakeTransparent();
            images.Images.Add(bmp);

            bmp = new Bitmap(WixFiles.GetResourceStream("elements.publish.bmp"));
            bmp.MakeTransparent();
            images.Images.Add(bmp);

            return images;
        }

        public void OnPropertyValueChanged(object s, PropertyValueChangedEventArgs e) {
            string currentDialogId = wxsDialogs.SelectedItems[0].Text;
//            XmlNode dialog = wixFiles.WxsDocument.SelectSingleNode(String.Format("/wix:Wix/*/wix:UI/wix:Dialog[@Id='{0}']", currentDialogId), wixFiles.WxsNsmgr);
            XmlNode dialog = (XmlNode) wxsDialogs.SelectedItems[0].Tag;

            ShowWixDialog(dialog);
        }

        public void OnWxsDialogsPopupContextMenu(object sender, EventArgs e) {
            MenuItem menuItem1 = new IconMenuItem("&New Dialog", new Bitmap(WixFiles.GetResourceStream("bmp.new.bmp")));
            MenuItem menuItem2 = new IconMenuItem("&Delete", new Bitmap(WixFiles.GetResourceStream("bmp.delete.bmp")));
            
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
            MenuItem menuItem1 = new IconMenuItem("&New", new Bitmap(WixFiles.GetResourceStream("bmp.new.bmp")));
            MenuItem menuItem2 = new IconMenuItem("&Delete", new Bitmap(WixFiles.GetResourceStream("bmp.delete.bmp")));
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
                if (ui == null) {
                    ui = wixFiles.WxsDocument.CreateElement("UI", "http://schemas.microsoft.com/wix/2003/01/wi");

                    XmlNode parent = wixFiles.WxsDocument.SelectSingleNode("/wix:Wix/*", wixFiles.WxsNsmgr);
                    if (parent == null) {
                        MessageBox.Show("No module or product found!");
                        return;
                    }

                    // if (parent.Name != "Module" && parent.Name != "Product") {
                    //      Whoops!
                    // }

                    parent.AppendChild(ui);
                }
                ui.AppendChild(dialog);

                ListViewItem item = new ListViewItem(frm.SelectedString);
                wxsDialogs.Items.Add(item);

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

        private void OnDialogItemChanged(XmlNode changedItem) {
            TreeNode node = FindTreeNode(changedItem, dialogTreeView.Nodes);
            if (node != null) {
                dialogTreeView.SelectedNode = node;
            }

            ShowWixProperties(changedItem);
        }

        private void OnDialogSelectionChanged(XmlNode selectedItem) {
            TreeNode node = FindTreeNode(selectedItem, dialogTreeView.Nodes);
            if (node != null) {
                dialogTreeView.SelectedNode = node;
            }

            ShowWixProperties(selectedItem);
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
                prevTop = TopLevelControl.Top;
                prevLeft = TopLevelControl.Right;
            }

            if (dialog != null) {
                DialogGenerator generator = new DialogGenerator(wixFiles, TopLevelControl);
                currentDialog = generator.GenerateDialog(dialog, this);
    
                if (currentDialog != null) {
                    currentDialog.ItemChanged += new DesignerFormItemHandler(OnDialogItemChanged);
                    currentDialog.SelectionChanged += new DesignerFormItemHandler(OnDialogSelectionChanged);

                    currentDialog.Left = prevLeft;
                    currentDialog.Top = prevTop;
        
                    currentDialog.Opacity = GetOpacity();
                    currentDialog.TopMost = AlwaysOnTop.Checked;
        
                    currentDialog.Show();
                }
            }
            if (prevDialog != null) {
                prevDialog.Hide();
                prevDialog.Dispose();
            }

            Focus();
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

            propertyGrid.SelectedObject = null;
            propertyGrid.SelectedObject = attAdapter;
            propertyGrid.Update();

            return;
        }


        private void OnAfterSelect(object sender, TreeViewEventArgs e) {
            XmlNode node = e.Node.Tag as XmlNode;
            if (node != null) {
                ShowWixProperties(node);
                if (currentDialog != null && currentDialog.Visible){
                    currentDialog.SelectedNode = node;
                }
            }
        }

        private void TreeViewMouseDown(object sender, System.Windows.Forms.MouseEventArgs e) {
            if (e.Button == MouseButtons.Right) {
                TreeNode node = dialogTreeView.GetNodeAt(e.X, e.Y);
                if (node == null) {
                    return;
                }
                dialogTreeView.SelectedNode = node;

                Point spot = PointToClient(dialogTreeView.PointToScreen(new Point(e.X,e.Y)));
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
                    dialogTreeViewContextMenu.MenuItems.Add(newControlElementMenu);
                    break;
                case "Control":
                    newControlSubElementsMenu.MenuItems.Clear();
                    dialogTreeViewContextMenu.MenuItems.Add(newControlSubElementsMenu);
                    newControlSubElementsMenu.MenuItems.Add(newTextElementMenu);
                    newControlSubElementsMenu.MenuItems.Add(newPublishElementMenu);
                    newControlSubElementsMenu.MenuItems.Add(newConditionElementMenu);
                    newControlSubElementsMenu.MenuItems.Add(newSubscribeElementMenu);
                    break;
                default:
                    break;
            }

            dialogTreeViewContextMenu.MenuItems.Add(deleteCurrentElementMenu);


            XmlAttributeAdapter attAdapter = (XmlAttributeAdapter) propertyGrid.SelectedObject;

            XmlDocumentationManager docManager = new XmlDocumentationManager(wixFiles);
            if (docManager.HasDocumentation(attAdapter.XmlNodeDefinition)) {
                dialogTreeViewContextMenu.MenuItems.Add(new IconMenuItem("-"));
                dialogTreeViewContextMenu.MenuItems.Add(infoAboutCurrentElementMenu);
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

            if (node.Name == "Control") {
                XmlElement newText = node.OwnerDocument.CreateElement(typeName, "http://schemas.microsoft.com/wix/2003/01/wi");

                TreeNode control = new TreeNode(typeName);
                control.Tag = newText;
                control.ImageIndex = imageIndex;
                control.SelectedImageIndex = imageIndex;

                dialogTreeView.SelectedNode.Nodes.Add(control);
                dialogTreeView.SelectedNode = control;

                ShowWixProperties(newText);
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

            WixEditSettings.Instance.Opacity = GetOpacity();
            WixEditSettings.Instance.SaveChanges();

            if (currentDialog != null) {
                currentDialog.Opacity = GetOpacity();
            }
        }

        private void AlwaysOnTop_Click(object sender, System.EventArgs e) {
            AlwaysOnTop.Checked = !AlwaysOnTop.Checked;

            WixEditSettings.Instance.AlwaysOnTop = AlwaysOnTop.Checked;
            WixEditSettings.Instance.SaveChanges();

            if (currentDialog != null) {
                currentDialog.TopMost = AlwaysOnTop.Checked;
            }
        }

        private void SnapToGrid_Click(object sender, System.EventArgs e) {
            EnterIntegerForm form = new EnterIntegerForm();
            form.Text = "Enter number of pixels to snap to:";
            form.SelectedInteger = WixEditSettings.Instance.SnapToGrid;

            if (form.ShowDialog() == DialogResult.OK) {
                SelectionOverlay.SnapToGrid = form.SelectedInteger;
                WixEditSettings.Instance.SnapToGrid = form.SelectedInteger;
                WixEditSettings.Instance.SaveChanges();
            }
        }

        private void DialogScale_Click(object sender, System.EventArgs e) {
            EnterIntegerForm form = new EnterIntegerForm();
            form.Text = "Enter percentage to scale to:";
            form.SelectedInteger = (int) (WixEditSettings.Instance.Scale*100);

            if (form.ShowDialog() == DialogResult.OK) {
                DialogGenerator.Scale = ((double)form.SelectedInteger)/100.00;
                WixEditSettings.Instance.Scale = ((double)form.SelectedInteger)/100.00;
                WixEditSettings.Instance.SaveChanges();

                XmlNode dialog = (XmlNode) wxsDialogs.SelectedItems[0].Tag;
                
                ShowWixDialog(dialog);
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
                    currentDialog = null;
                }
            }

            base.Dispose( disposing );
        }
    }
}
