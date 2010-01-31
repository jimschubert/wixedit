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
using WixEdit.Settings;

namespace WixEdit.Wizard
{
    class SelectTemplatesSheet : BaseSheet
    {
        Label titleLabel;
        Label descriptionLabel;
        Label lineLabel;
        ListView listView;

        public SelectTemplatesSheet(WizardForm creator)
            : base(creator)
        {
            this.AutoScroll = true;

            titleLabel = new Label();
            titleLabel.Text = "Select featues to add";
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
            descriptionLabel.Text = "Select functionality you want to add to the installer";
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

            listView = new ListView();
            listView.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            listView.Location = new Point(4, titleLabel.Height + descriptionLabel.Height + lineLabel.Height + 4);
            listView.Width = this.Width - 8;
            listView.Height = this.Height - listView.Top - 4;
            listView.CheckBoxes = true;
            listView.FullRowSelect = true;
            listView.View = View.List;
            listView.FullRowSelect = true;

            this.Controls.Add(listView);

            DirectoryInfo oldTemplateDir = null;
            DirectoryInfo templateDir = null;

            if (!String.IsNullOrEmpty(WixEditSettings.Instance.TemplateDirectory))
            {
                oldTemplateDir = new DirectoryInfo(WixEditSettings.Instance.TemplateDirectory);
                templateDir = new DirectoryInfo(Path.Combine(oldTemplateDir.Parent.FullName, "wizard"));
            }

            if (templateDir != null && 
                templateDir.Exists)
            {
                FileInfo[] files = templateDir.GetFiles("template.xml", SearchOption.AllDirectories);

                foreach (FileInfo file in files)
                {
                    if (file.Directory.Parent.FullName == templateDir.FullName)
                    {
                        string title = file.Directory.Name;

                        XmlDocument doc = new XmlDocument();
                        XmlNamespaceManager nsmgr = new XmlNamespaceManager(doc.NameTable);

                        doc.Load(file.FullName);
                        XmlElement template = (XmlElement)doc.SelectSingleNode("/Template");
                        string tempTitle = template.GetAttribute("Title");

                        if (!String.IsNullOrEmpty(tempTitle))
                        {
                            title = tempTitle;
                        }

                        ListViewItem item = new ListViewItem(title);
                        item.Tag = file.FullName;

                        listView.Items.Add(item);
                    }
                }
            }
            else
            {
                StringBuilder message = new StringBuilder("Directory containing the templates could not be found. Please reinstall WixEdit.");
                if (templateDir != null)
                {
                    message.AppendFormat("\r\n\r\nDirectory:\r\n{0}", templateDir.FullName);
                }

                MessageBox.Show(message.ToString(), "Templates not found");
            }
        }

        public override bool OnNext()
        {
            foreach (ListViewItem item in listView.CheckedItems)
            {
                Wizard.AddTemplate((String)item.Tag);
            }

            return base.OnNext();
        }

        public override bool UndoNext()
        {
            int numberOfTemplates = listView.CheckedItems.Count;

            for (int i = 0; i < numberOfTemplates; i++)
            {
                Wizard.RemoveLastAddedTemplate();
            }

            return base.UndoNext();
        }
    }
}
