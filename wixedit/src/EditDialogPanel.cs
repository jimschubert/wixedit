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
    /// Summary description for EditDialogPanel.
    /// </summary>
    public class EditDialogPanel : Panel {
        #region Controls
        
        private Form currentDialog;
        private TreeView dialogTreeView;
        private PropertyGrid propertyGrid;
        private ContextMenu propertyGridContextMenu;
//        private ListBox wxsDialogs;
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

        #endregion

        private WixFiles wixFiles;

        public EditDialogPanel(WixFiles wixFiles) {
            this.wixFiles = wixFiles;

            InitializeComponent();
           
            Opacity100.Checked = true;

            this.wxsDialogs.Items.Clear();

            XmlNodeList dialogs = wixFiles.WxsDocument.SelectNodes("/wix:Wix/wix:Product/wix:UI/wix:Dialog", wixFiles.WxsNsmgr);
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

        private void OnResizeWxsDialogs(object sender, System.EventArgs e) {
            if (this.wxsDialogs.Columns[0] != null) {
                this.wxsDialogs.Columns[0].Width = this.wxsDialogs.ClientSize.Width - 4;
            }
        }

        public MenuItem Menu {
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
            this.propertyGrid = new PropertyGrid();
            this.propertyGridContextMenu = new ContextMenu();
            this.wxsDialogs = new ListView();
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
            this.viewMenu.Text = "Tools";
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
        }

        #endregion

        public void OnPropertyValueChanged(object s, PropertyValueChangedEventArgs e) {
            // TODO: Good place to keep track of command, for undo functionality

            string currentDialogId = wxsDialogs.SelectedItems[0].Text;
            XmlNode dialog = wixFiles.WxsDocument.SelectSingleNode(String.Format("/wix:Wix/wix:Product/wix:UI/wix:Dialog[@Id='{0}']", currentDialogId), wixFiles.WxsNsmgr);
            
            ShowWixDialog(dialog);
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
            XmlAttributeAdapter attAdapter = propertyGrid.SelectedObject as XmlAttributeAdapter;

            ArrayList attributes = new ArrayList();

            XmlNodeList xmlAttributes = attAdapter.XmlNodeDefinition.SelectNodes("xs:attribute", wixFiles.XsdNsmgr);
            foreach (XmlNode at in xmlAttributes) {
                string attName = at.Attributes["name"].Value;
                if (attAdapter.XmlNode.Attributes[attName] == null) {
                    attributes.Add(attName);
                }
            }

            SelectStringForm frm = new SelectStringForm();
            frm.PossibleStrings = attributes.ToArray(typeof(String)) as String[];
            if (DialogResult.OK != frm.ShowDialog()) {
                return;
            }

            // Show dialog to choose from available items.
            string newAttributeName = frm.SelectedString;

            // Get the XmlAttribute from the PropertyDescriptor
            XmlAttributePropertyDescriptor desc = propertyGrid.SelectedGridItem.PropertyDescriptor as XmlAttributePropertyDescriptor;
            XmlAttribute att = wixFiles.WxsDocument.CreateAttribute(newAttributeName);

            // resetting the propertyGrid.
            propertyGrid.SelectedObject = null;

            // Add the attribute
            attAdapter.XmlNode.Attributes.Append(att);

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

        private void OnSelectedDialogChanged(object sender, System.EventArgs e) {
            if (wxsDialogs.SelectedItems.Count > 0 && wxsDialogs.SelectedItems[0] != null) {
                string currentDialogId = wxsDialogs.SelectedItems[0].Text;
                XmlNode dialog = wixFiles.WxsDocument.SelectSingleNode(String.Format("/wix:Wix/wix:Product/wix:UI/wix:Dialog[@Id='{0}']", currentDialogId), wixFiles.WxsNsmgr);
                
                ShowWixDialog(dialog);
                ShowWixDialogTree(dialog);
                ShowWixProperties(dialog);
            }
        }

        private void ShowWixDialog(XmlNode dialog) {
            Form prevDialog = null;
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

            DialogGenerator generator = new DialogGenerator(wixFiles);
            currentDialog = generator.GenerateDialog(dialog, this);

            currentDialog.Left = prevLeft;
            currentDialog.Top = prevTop;

            currentDialog.Opacity = GetOpacity();
            currentDialog.TopMost = this.AlwaysOnTop.Checked;

            currentDialog.Show();
            if (prevDialog != null) {
                prevDialog.Hide();
                prevDialog.Dispose();
            }

            this.Focus();
        }

        private void ShowWixDialogTree(XmlNode dialog) {
            dialogTreeView.Nodes.Clear();

            TreeNode rootNode = new TreeNode("Dialog");
            rootNode.Tag = dialog;
            dialogTreeView.Nodes.Add(rootNode);

            foreach (XmlNode control in dialog.ChildNodes) {
                AddSubTreeItems(rootNode, control);
            }

            dialogTreeView.ExpandAll();
            dialogTreeView.SelectedNode = rootNode;
        }

        private void AddSubTreeItems(TreeNode parent, XmlNode xmlNodeToAdd) {
            string treeNodeName = xmlNodeToAdd.Name;
            if (xmlNodeToAdd.Attributes != null && xmlNodeToAdd.Attributes["Id"] != null) {
                treeNodeName = xmlNodeToAdd.Attributes["Id"].Value;
            }

            TreeNode child = new TreeNode(treeNodeName);
            child.Tag = xmlNodeToAdd;
            parent.Nodes.Add(child);

            foreach (XmlNode xmlChildNode in xmlNodeToAdd.ChildNodes) {
                AddSubTreeItems(child, xmlChildNode);
            }
        }

        private void ShowWixProperties(XmlNode xmlNode) {
            XmlAttributeAdapter attAdapter = new XmlAttributeAdapter(xmlNode, wixFiles);

            propertyGrid.SelectedObject = attAdapter;
            propertyGrid.Update();

            return;
        }

        private void OnAfterSelect(object sender, TreeViewEventArgs e) {
            XmlNode node = e.Node.Tag as XmlNode;
            if (node != null) {
                ShowWixProperties(node);
            }
        }

        private void OnPropertyDoubleClick(object sender, System.EventArgs e) {
            // Edit here?
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
