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
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;
using System.Xml;
using System.IO;
using System.Resources;
using System.Reflection;

namespace WixEdit
{
	/// <summary>
	/// Summary description for Form1.
	/// </summary>
	public class EditorForm : System.Windows.Forms.Form
	{

        private System.Windows.Forms.OpenFileDialog openWxsFileDialog;
        private XmlDocument wxsDocument;
        private XmlNamespaceManager wxsNsmgr;
        private XmlDocument wxsXsd;
        private FileInfo wxsFile;
        private Form currentDialog;
        private System.Windows.Forms.MainMenu mainMenu1;
        private System.Windows.Forms.MenuItem menuItem1;
        private System.Windows.Forms.MenuItem menuItem2;
        private System.Windows.Forms.TreeView dialogTreeView;
        private System.Windows.Forms.ListBox listBox1;
        private System.Windows.Forms.ListView listView1;
        private System.Windows.Forms.ListView propertyListView;
        private System.Windows.Forms.ListBox wxsDialogs;
        private System.Windows.Forms.Splitter splitter1;
        private System.Windows.Forms.Splitter splitter2;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Splitter splitter3;
        private System.Windows.Forms.MenuItem menuItem3;
        private System.Windows.Forms.MenuItem Opacity100;
        private System.Windows.Forms.MenuItem Opacity75;
        private System.Windows.Forms.MenuItem Opacity50;
        private System.Windows.Forms.MenuItem Opacity25;

		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public EditorForm()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

            wxsDocument = new XmlDocument();
            wxsXsd = new XmlDocument();

            Assembly assembly = Assembly.GetExecutingAssembly();
            string resourceName = "WixEdit.wix.xsd";
            if (assembly.GetManifestResourceInfo(resourceName) == null) {
                throw new Exception("Could not find resource: " + resourceName);
            }

            Stream resourceStream = assembly.GetManifestResourceStream(resourceName);
            if (resourceStream == null) {
                throw new Exception("Could not load resource: " +  resourceName);
            }

            wxsXsd.Load(resourceStream);

            ContextMenu contextMenu = new ContextMenu();
            contextMenu.MenuItems.Add(0, new MenuItem("New Attribute", new EventHandler(NewAttribute)));

            propertyListView.ContextMenu = contextMenu;
           
            Opacity100.Checked = true;
           
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if (components != null) 
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
            this.openWxsFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.mainMenu1 = new System.Windows.Forms.MainMenu();
            this.menuItem1 = new System.Windows.Forms.MenuItem();
            this.menuItem2 = new System.Windows.Forms.MenuItem();
            this.menuItem3 = new System.Windows.Forms.MenuItem();
            this.Opacity100 = new System.Windows.Forms.MenuItem();
            this.Opacity75 = new System.Windows.Forms.MenuItem();
            this.Opacity50 = new System.Windows.Forms.MenuItem();
            this.Opacity25 = new System.Windows.Forms.MenuItem();
            this.dialogTreeView = new System.Windows.Forms.TreeView();
            this.listBox1 = new System.Windows.Forms.ListBox();
            this.listView1 = new System.Windows.Forms.ListView();
            this.propertyListView = new System.Windows.Forms.ListView();
            this.wxsDialogs = new System.Windows.Forms.ListBox();
            this.splitter1 = new System.Windows.Forms.Splitter();
            this.splitter2 = new System.Windows.Forms.Splitter();
            this.panel1 = new System.Windows.Forms.Panel();
            this.splitter3 = new System.Windows.Forms.Splitter();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // mainMenu1
            // 
            this.mainMenu1.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
                                                                                      this.menuItem1,
                                                                                      this.menuItem3});
            // 
            // menuItem1
            // 
            this.menuItem1.Index = 0;
            this.menuItem1.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
                                                                                      this.menuItem2});
            this.menuItem1.Text = "File";
            // 
            // menuItem2
            // 
            this.menuItem2.Index = 0;
            this.menuItem2.Text = "Load";
            this.menuItem2.Click += new System.EventHandler(this.menuItem2_Click);
            // 
            // menuItem3
            // 
            this.menuItem3.Index = 1;
            this.menuItem3.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
                                                                                      this.Opacity100,
                                                                                      this.Opacity75,
                                                                                      this.Opacity50,
                                                                                      this.Opacity25});
            this.menuItem3.Text = "Tools";
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
            // dialogTreeView
            // 
            this.dialogTreeView.Dock = System.Windows.Forms.DockStyle.Left;
            this.dialogTreeView.ImageIndex = -1;
            this.dialogTreeView.Location = new System.Drawing.Point(0, 0);
            this.dialogTreeView.Name = "dialogTreeView";
            this.dialogTreeView.SelectedImageIndex = -1;
            this.dialogTreeView.Size = new System.Drawing.Size(140, 266);
            this.dialogTreeView.TabIndex = 6;
            this.dialogTreeView.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.OnAfterSelect);
            // 
            // listBox1
            // 
            this.listBox1.Location = new System.Drawing.Point(0, 0);
            this.listBox1.Name = "listBox1";
            this.listBox1.Size = new System.Drawing.Size(120, 95);
            this.listBox1.TabIndex = 0;
            // 
            // listView1
            // 
            this.listView1.Location = new System.Drawing.Point(56, 64);
            this.listView1.Name = "listView1";
            this.listView1.TabIndex = 0;
            // 
            // propertyListView
            // 
            this.propertyListView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.propertyListView.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
            this.propertyListView.Location = new System.Drawing.Point(140, 0);
            this.propertyListView.Name = "propertyListView";
            this.propertyListView.Size = new System.Drawing.Size(269, 266);
            this.propertyListView.TabIndex = 1;
            this.propertyListView.DoubleClick += new System.EventHandler(this.OnPropertyDoubleClick);
            this.propertyListView.MouseUp += new System.Windows.Forms.MouseEventHandler(this.propertyListView_MouseUp);
            // 
            // wxsDialogs
            // 
            this.wxsDialogs.Dock = System.Windows.Forms.DockStyle.Left;
            this.wxsDialogs.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
            this.wxsDialogs.Location = new System.Drawing.Point(0, 0);
            this.wxsDialogs.Name = "wxsDialogs";
            this.wxsDialogs.Size = new System.Drawing.Size(140, 264);
            this.wxsDialogs.TabIndex = 0;
            this.wxsDialogs.SelectedIndexChanged += new System.EventHandler(this.OnSelectedDialogChanged);
            // 
            // splitter1
            // 
            this.splitter1.Location = new System.Drawing.Point(140, 0);
            this.splitter1.Name = "splitter1";
            this.splitter1.Size = new System.Drawing.Size(2, 266);
            this.splitter1.TabIndex = 7;
            this.splitter1.TabStop = false;
            // 
            // splitter2
            // 
            this.splitter2.Dock = System.Windows.Forms.DockStyle.Right;
            this.splitter2.Location = new System.Drawing.Point(551, 0);
            this.splitter2.Name = "splitter2";
            this.splitter2.Size = new System.Drawing.Size(2, 266);
            this.splitter2.TabIndex = 8;
            this.splitter2.TabStop = false;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.splitter3);
            this.panel1.Controls.Add(this.propertyListView);
            this.panel1.Controls.Add(this.dialogTreeView);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(142, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(409, 266);
            this.panel1.TabIndex = 9;
            // 
            // splitter3
            // 
            this.splitter3.Location = new System.Drawing.Point(140, 0);
            this.splitter3.Name = "splitter3";
            this.splitter3.Size = new System.Drawing.Size(2, 266);
            this.splitter3.TabIndex = 7;
            this.splitter3.TabStop = false;
            // 
            // EditorForm
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 14);
            this.ClientSize = new System.Drawing.Size(553, 266);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.splitter2);
            this.Controls.Add(this.splitter1);
            this.Controls.Add(this.wxsDialogs);
            this.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
            this.Menu = this.mainMenu1;
            this.Name = "EditorForm";
            this.Text = "Wix Dialog Editor";
            this.panel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }
		#endregion

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		//[STAThread]
		static void Main() 
		{
			Application.Run(new EditorForm());
		}

        private void button1_Click(object sender, System.EventArgs e) {


        }

        private void LoadWxsDocument(Stream wxsStream) {
            this.wxsDocument.Load(wxsStream);
            this.wxsDialogs.Items.Clear();

            //XmlNodeList dialogs = this.wxsDocument.SelectNodes("/wix/product/ui/dialog");
            
            wxsNsmgr = new XmlNamespaceManager(this.wxsDocument.NameTable);
            wxsNsmgr.AddNamespace("wix", this.wxsDocument.DocumentElement.NamespaceURI);

            XmlNodeList dialogs = this.wxsDocument.SelectNodes("/wix:Wix/wix:Product/wix:UI/wix:Dialog", wxsNsmgr);
            foreach (XmlNode dialog in dialogs) {
                XmlAttribute attr = dialog.Attributes["Id"];
                if (attr != null) {
                    this.wxsDialogs.Items.Add(attr.Value);
                }
            }
        }

        private void OnSelectedDialogChanged(object sender, System.EventArgs e) {
            string currentDialogId = wxsDialogs.SelectedItem.ToString();
            XmlNode dialog = this.wxsDocument.SelectSingleNode(String.Format("/wix:Wix/wix:Product/wix:UI/wix:Dialog[@Id='{0}']", currentDialogId), wxsNsmgr);
            
            ShowWixDialog(dialog);
            ShowWixDialogTree(dialog);
            ShowWixProperties(dialog);
//
//            
//            
//
//
//            TreeNode rootNode = new TreeNode("Dialog");
//            dialogTreeView.Nodes.Add(rootNode);
//
//            xmlEditBox.Text = dialog.OuterXml;
            
        }

        private void menuItem2_Click(object sender, System.EventArgs e) {
            this.openWxsFileDialog.Filter = "wxs files (*.wxs)|*.wxs|All files (*.*)|*.*" ;
            this.openWxsFileDialog.RestoreDirectory = true ;

            if(this.openWxsFileDialog.ShowDialog() == DialogResult.OK) {
                wxsFile = new FileInfo(this.openWxsFileDialog.FileName);

                Stream myStream;
                if((myStream = wxsFile.OpenRead())!= null) {
                    LoadWxsDocument(myStream);

                    myStream.Close();
                }
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
                prevTop = this.Top;
                prevLeft = this.Right;
            }

            DialogGenerator generator = new DialogGenerator(wxsDocument, wxsNsmgr, wxsFile.Directory);
            currentDialog = generator.GenerateDialog(dialog, this);

            currentDialog.Left = prevLeft;
            currentDialog.Top = prevTop;

            currentDialog.Opacity = GetOpacity();


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
            propertyListView.Clear();
            ColumnHeader keyCol = new ColumnHeader();
            keyCol.Width = 120;
            keyCol.Text = "";

            ColumnHeader valCol = new ColumnHeader();
            valCol.Width = 200;
            valCol.Text = "";

            propertyListView.Columns.Add(keyCol);
            propertyListView.Columns.Add(valCol);

            propertyListView.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            propertyListView.FullRowSelect = true;
            propertyListView.GridLines = true;
            propertyListView.Name = "lsvData";
            propertyListView.TabIndex = 1;
            propertyListView.View = System.Windows.Forms.View.Details;

            if (xmlNode.Attributes != null) {
                foreach (XmlAttribute at in xmlNode.Attributes) {
                    ListViewItem item = new ListViewItem(at.Name);
                    item.SubItems.Add(at.Value);

                    propertyListView.Items.Add(item);
                }
            } else {
                ListViewItem item = new ListViewItem("cdata");
                item.SubItems.Add(xmlNode.Value);

                propertyListView.Items.Add(item);
            }
        }

        private void OnAfterSelect(object sender, System.Windows.Forms.TreeViewEventArgs e) {
            XmlNode node = e.Node.Tag as XmlNode;
            if (node != null) {
                ShowWixProperties(node);
            }
        }

        private void OnPropertyDoubleClick(object sender, System.EventArgs e) {
            // Edit here?
        }

        private void propertyListView_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e) {
        }

        private ListBox newAttribBox;
        private void NewAttribute(object sender, System.EventArgs e) {
            XmlNamespaceManager nsmgr = new XmlNamespaceManager(wxsXsd.NameTable);
            nsmgr.AddNamespace("xs", "http://www.w3.org/2001/XMLSchema");

            XmlNode xmlNode = dialogTreeView.SelectedNode.Tag as XmlNode;

            XmlNode xsdDef = wxsXsd.SelectSingleNode(String.Format("/xs:schema/xs:element[@name='{0}']", xmlNode.Name), nsmgr);

            XmlNodeList xsdDefNodes = xsdDef.SelectNodes("xs:complexType/xs:attribute", nsmgr);

            // Each Attribute not in ListView
            newAttribBox = new ListBox();
            newAttribBox.BorderStyle = BorderStyle.FixedSingle;

            this.Controls.Add(newAttribBox);
            newAttribBox.Left = propertyListView.Left + propertyListView.Parent.Left;
            newAttribBox.Top = propertyListView.Items[propertyListView.Items.Count-1].Bounds.Bottom;
            newAttribBox.Width = 200;
            newAttribBox.Height = 100;
            newAttribBox.BringToFront();

            foreach (XmlNode at in xsdDefNodes) {
                newAttribBox.Items.Add(at.Attributes["name"].Value);
            }

            newAttribBox.LostFocus += new EventHandler(newAttribBoxLostFocus);
            newAttribBox.Show();
            newAttribBox.Focus();
        }

        private void newAttribBoxLostFocus(object sender, EventArgs e) {
            if (newAttribBox != null) {
                newAttribBox.Hide();
                newAttribBox.Dispose();
                newAttribBox = null;
            }
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
    }
}
