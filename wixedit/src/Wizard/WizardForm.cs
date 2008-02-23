using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Collections;
using System.Xml;

namespace WixEdit.Wizard
{
    public partial class WizardForm : Form
    {
        WixFiles wixFiles;
        int undoCountBefore = 0;
        List<BaseSheet> sheets = new List<BaseSheet>();
        int currentSheetIndex = -1;

        public WizardForm(WixFiles editWixFiles)
        {
            wixFiles = editWixFiles;
            undoCountBefore = wixFiles.UndoManager.UndoCount;

            InitializeComponent();

            IntroductionSheet welcome = new IntroductionSheet("WixEdit wizard", "The WixEdit wizard helps you creating MSI files. The wizard allows you to add functionality to your MSI file.\r\n\r\nFor example:\r\nAdd files, Create shortcuts, Create virual directories, etc.\r\n\r\n\r\nClick \"Next\" to continue or \"Cancel\" to exit the WixEdit wizard.", this);

            AddSheet(welcome);

            FileSheet files = new FileSheet(this);

            AddSheet(files);

            XmlDocument doc = new XmlDocument();
            XmlNamespaceManager nsmgr = new XmlNamespaceManager(doc.NameTable);
            doc.Load(@"E:\WixEdit 0.6.1762-rel\wizard\Create Virtual Directory\template.xml");
            XmlElement template = (XmlElement)doc.SelectSingleNode("/Template");

            // WizardSheet sheet = new WizardSheet();
            IntroductionSheet intro = new IntroductionSheet(template, this);

            AddSheet(intro);

            for (int i = 1; ; i++)
            {
                XmlElement step = (XmlElement)template.SelectSingleNode(String.Format("Step[@Sequence={0}]", i));
                if (step == null)
                {
                    break;
                }

                StepSheet stepSheet = new StepSheet(step, this);

                AddSheet(stepSheet);
            }
        }

        public WixFiles WixFiles
        {
            get { return wixFiles; }
            set { wixFiles = value; }
        }

        private void AddSheet(BaseSheet sheet)
        {
            sheets.Add(sheet);

            sheet.Dock = DockStyle.Fill;
            if (sheets.Count > 1)
            {
                sheet.Visible = false;
            }
            else
            {
                currentSheetIndex = 0;
                backButton.Enabled = false;
            }

            contentPanel.Controls.Add(sheet);
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            // while (undoCountBefore != WixFiles.UndoManager.UndoCount)
            // {
            //    WixFiles.UndoManager.Undo();
            // }

            MessageBox.Show("Cancel does NOT undo changes made by the wizard!");

            this.Close();
        }

        private void nextButton_Click(object sender, EventArgs e)
        {
            if (currentSheetIndex + 1 == sheets.Count)
            {
                if (sheets[currentSheetIndex].OnNext() == false)
                {
                    MessageBox.Show("The values entered are not all correct.");

                    return;
                }
                
                this.Close();
            }
            else
            {
                if (sheets[currentSheetIndex].OnNext() == false)
                {
                    MessageBox.Show("The values entered are not all correct.");

                    return;
                }

                sheets[currentSheetIndex].Visible = false;
                sheets[++currentSheetIndex].Visible = true;
                if (currentSheetIndex + 1 == sheets.Count)
                {
                    nextButton.Text = "Finish";
                }

                backButton.Enabled = true;
            }
        }

        private void backButton_Click(object sender, EventArgs e)
        {
            if (currentSheetIndex <= 0)
            {
                // Hmmm...
            }
            else
            {
                if (sheets[currentSheetIndex].OnBack() == false)
                {
                    MessageBox.Show("Not able to go back.");

                    return;
                }

                sheets[currentSheetIndex].Visible = false;
                sheets[--currentSheetIndex].Visible = true;
                sheets[currentSheetIndex].UndoNext();
                if (currentSheetIndex == 0)
                {
                    backButton.Enabled = false;
                }

                nextButton.Text = "Next";
            }
        }
    }
}