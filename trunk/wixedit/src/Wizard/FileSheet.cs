using System;
using System.Collections.Generic;
using System.Text;
using WixEdit.Wizard;
using System.Windows.Forms;
using System.Drawing;
using System.Xml;

namespace WixEdit.Wizard
{
    class FileSheet : BaseSheet
    {
        Label titleLabel;
        Label descriptionLabel;
        Label lineLabel;
        TreeView tree;

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

            this.Controls.Add(tree);

            XmlDocument wxsDoc = Wizard.WixFiles.WxsDocument;
            XmlNamespaceManager wxsNsmgr = Wizard.WixFiles.WxsNsmgr;

            XmlNodeList dirNodes = wxsDoc.SelectNodes("/wix:Wix/*/wix:Directory", wxsNsmgr);
            TreeNodeCollection treeNodes = tree.Nodes;
            tree.CheckBoxes = true;

            AddDirectoryTreeNodes(dirNodes, treeNodes);
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
    }
}
