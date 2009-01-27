using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Windows.Forms;
using System.Drawing;

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
            string description = "The WixEdit wizard finished creating the source for the MSI file. WixEdit allows you to customize the MSI.\r\n\r\n\r\nClick \"Finish\" to finish the WixEdit wizard and start customizing the MSI.";

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
    }
}
