using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Windows.Forms;
using System.Drawing;
using System.Collections;

namespace WixEdit.Wizard
{
    class FinishSheet : BaseSheet
    {
        Label titleLabel;
        Label descriptionLabel;
        PictureBox picture;

        public FinishSheet(WizardForm creator)
            : base(creator)
        {
            string title = "Finished Wizard";
            string description = "The WixEdit wizard finished creating the source for the MSI file. WixEdit allows you to customize the MSI.\r\n\r\nClick \"Finish\" to finish the WixEdit wizard and start customizing the MSI.";

            Initialize(title, description);
        }

        private void Initialize(string title, string description)
        {
            this.BackColor = Color.White;

            picture = new PictureBox();
            picture.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Bottom;
            picture.Height = this.Height;
            picture.Left = 0;
            picture.Width = 164;
            picture.Image = new Bitmap(WixFiles.GetResourceStream("dlgbmp.bmp"));
            picture.SizeMode = PictureBoxSizeMode.StretchImage;

            this.Controls.Add(picture);

            titleLabel = new Label();
            titleLabel.Text = title;
            titleLabel.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            titleLabel.Height = 50;
            titleLabel.Width = this.Width - picture.Width;
            titleLabel.Top = 0;
            titleLabel.Left = picture.Width;
            titleLabel.Padding = new Padding(7, 12, 0, 0);
            titleLabel.Font = new Font("Verdana",
                                        13,
                                        FontStyle.Bold,
                                        GraphicsUnit.Point
                                    );
            titleLabel.BackColor = Color.White;
            this.Controls.Add(titleLabel);

            descriptionLabel = new Label();
            descriptionLabel.Text = description;

            descriptionLabel.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            descriptionLabel.Width = this.Width - picture.Width;
            descriptionLabel.Left = picture.Width;
            descriptionLabel.Height = this.Height - titleLabel.Height;
            descriptionLabel.Top = titleLabel.Height;
            descriptionLabel.Padding = new Padding(7, 15, 5, 5);
            this.Controls.Add(descriptionLabel);
        }

        public override void OnShow()
        {
            Wizard.WixFiles.UndoManager.BeginNewCommandRange();

            XmlDocument wxsDoc = Wizard.WixFiles.WxsDocument;
            XmlNamespaceManager wxsNsmgr = Wizard.WixFiles.WxsNsmgr;

            ArrayList orphanedComponents = new ArrayList();

            XmlNodeList componentNodes = wxsDoc.SelectNodes("//wix:Component", wxsNsmgr);
            foreach (XmlElement componentNode in componentNodes)
            {
                XmlNodeList componentRefNodes = wxsDoc.SelectNodes(String.Format("//wix:ComponentRef[@Id='{0}']", componentNode.GetAttribute("Id")), wxsNsmgr);
                if (componentRefNodes.Count == 0)
                {
                    orphanedComponents.Add(componentNode);
                }
            }

            XmlNodeList featureNodes = wxsDoc.SelectNodes("//wix:Feature", wxsNsmgr);

            if (orphanedComponents.Count > 0)
            {
                XmlElement defaultFeature = null;

                // Zoek naar precies 1 feature of maak er 1
                if (featureNodes.Count == 1)
                {
                    defaultFeature = (XmlElement)featureNodes[0];
                }
                else if (featureNodes.Count == 0)
                {
                    // Add default feature
                    defaultFeature = Wizard.WixFiles.WxsDocument.CreateElement("Feature", WixFiles.WixNamespaceUri);
                    defaultFeature.SetAttribute("Id", "DefaultFeature");
                    defaultFeature.SetAttribute("Title", "Default Feature");
                    defaultFeature.SetAttribute("Level", "1");

                    XmlNodeList targetDir = Wizard.WixFiles.WxsDocument.SelectNodes("//wix:Directory[@Id='TARGETDIR']", Wizard.WixFiles.WxsNsmgr);
                    if (targetDir.Count > 0)
                    {
                        defaultFeature.SetAttribute("ConfigurableDirectory", "TARGETDIR");
                    }

                    XmlNode parentNode = Wizard.WixFiles.WxsDocument.SelectSingleNode("/wix:Wix/*", Wizard.WixFiles.WxsNsmgr);
                    parentNode.AppendChild(defaultFeature);
                }

                if (defaultFeature != null)
                {
                    // Precies 1 feature of 1 gemaakt
                    foreach (XmlElement component in orphanedComponents)
                    {
                        XmlElement componentRef = Wizard.WixFiles.WxsDocument.CreateElement("ComponentRef", WixFiles.WixNamespaceUri);
                        componentRef.SetAttribute("Id", component.GetAttribute("Id"));
                        defaultFeature.AppendChild(componentRef);
                    }
                }
                else
                {
                    // Te veel features gevonden
                    descriptionLabel.Text = "Please note:\r\nThere are more than one Feature elements to add the orphaned Components to. Please make sure all components are added to one or more feature.\r\n\r\n"
                        + descriptionLabel.Text;
                }
            }
        }

        public override bool OnBack()
        {
            Wizard.WixFiles.UndoManager.Undo();

            return true;
        }
    }
}
