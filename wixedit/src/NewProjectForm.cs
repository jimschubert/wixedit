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

using WixEdit.PropertyGridExtensions;
using WixEdit.Settings;

namespace WixEdit {
	/// <summary>
	/// NewProjectForm create a new WiX toolset file from a template.
	/// </summary>
	public class NewProjectForm : Form {
        protected Button buttonOk;
        protected Button buttonCancel;

        protected Label templateListLabel;
        protected ListBox templateList;

        protected Label wixFileNameLabel;
        protected TextBox wixFileName;

        protected Label directoryNameLabel;
        protected TextBox directoryName;
        protected Button directoryBrowseButton;

        protected Label wixDestinationLabel;
        protected Label wixDestinationTextLabel;

        protected Label installerTypeLabel;
        protected RadioButton productRadioButton;
        protected RadioButton moduleRadioButton;

        protected string newFileName;

		public NewProjectForm() {
			InitializeComponent();
		}

        private void InitializeComponent() {
            int padding = 2;

            Text = "Create new WiX file";
            ShowInTaskbar = false;

            ClientSize = new Size(480, 256);
            SizeGripStyle = SizeGripStyle.Hide;


            buttonOk = new Button();
            buttonOk.Text = "Ok";
            buttonOk.Anchor = AnchorStyles.Right | AnchorStyles.Bottom;
            buttonOk.FlatStyle = FlatStyle.System;
            buttonOk.Click += new EventHandler(OnOk);
            Controls.Add(buttonOk);

            buttonCancel = new Button();
            buttonCancel.Text = "Cancel";
            buttonCancel.Anchor = AnchorStyles.Right | AnchorStyles.Bottom;
            buttonCancel.FlatStyle = FlatStyle.System;
            Controls.Add(buttonCancel);

            templateListLabel = new Label();
            templateListLabel.Text = "Select template:";
            templateListLabel.Top = 0;
            templateListLabel.Left = padding;
            templateListLabel.Height = 14;
            templateListLabel.Width = ClientSize.Width - 2*padding;
            templateListLabel.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Top;
            Controls.Add(templateListLabel);

            templateList = new ListBox();
            templateList.Font = new Font("Tahoma", 8.25F, FontStyle.Regular, GraphicsUnit.Point, ((System.Byte)(0)));
            templateList.Name = "templateList";
            templateList.TabIndex = 1;
            templateList.Top = templateListLabel.Bottom;
            templateList.Left = padding;
            templateList.Height = ClientSize.Height - 193;
            templateList.Width = ClientSize.Width - 2*padding;
            templateList.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Bottom | AnchorStyles.Top;
            Controls.Add(templateList);

            templateList.Items.Add("<none>");
            templateList.SelectedIndex = 0;
            if (WixEditSettings.Instance.TemplateDirectory != null && Directory.Exists(WixEditSettings.Instance.TemplateDirectory)) {
                foreach (string dir in Directory.GetDirectories(WixEditSettings.Instance.TemplateDirectory)) {
                    DirectoryInfo dirInfo = new DirectoryInfo(dir);
                    templateList.Items.Add(dirInfo.Name);
                }
            }

            wixFileNameLabel = new Label();
            wixFileNameLabel.Text = "Enter WiX Project name:";
            wixFileNameLabel.Top = templateList.Bottom + 2;
            wixFileNameLabel.Left = padding;
            wixFileNameLabel.Height = 14;
            wixFileNameLabel.Width = ClientSize.Width - 2*padding;
            wixFileNameLabel.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Bottom;
            Controls.Add(wixFileNameLabel);
            
            wixFileName = new TextBox();
            wixFileName.Top = wixFileNameLabel.Bottom;
            wixFileName.Left = padding;
            wixFileName.Width = ClientSize.Width - 2*padding;
            wixFileName.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Bottom;
            wixFileName.Text = "NewProjectName";
            wixFileName.TextChanged += new EventHandler(OnCheckEnableOkButton);
            Controls.Add(wixFileName);

            directoryNameLabel = new Label();
            directoryNameLabel.Text = "Select directory:";
            directoryNameLabel.Top = wixFileName.Bottom + 2;
            directoryNameLabel.Left = padding;
            directoryNameLabel.Height = 14;
            directoryNameLabel.Width = ClientSize.Width - 2*padding;
            directoryNameLabel.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Bottom;
            Controls.Add(directoryNameLabel);

            directoryName = new TextBox();
            directoryName.Top = directoryNameLabel.Bottom;
            directoryName.Left = padding;
            directoryName.Width = ClientSize.Width - buttonOk.Width - 3*padding;
            directoryName.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Bottom;
            directoryName.TextChanged += new EventHandler(OnCheckEnableOkButton);
            directoryName.Text = WixEditSettings.Instance.DefaultProjectDirectory;
            Controls.Add(directoryName);

            directoryBrowseButton = new Button();
            directoryBrowseButton.Text = "Browse";
            directoryBrowseButton.FlatStyle = FlatStyle.System;
            directoryBrowseButton.Top = directoryNameLabel.Bottom;
            directoryBrowseButton.Height = wixFileName.Height;
            directoryBrowseButton.Left = ClientSize.Width - directoryBrowseButton.Width - padding;
            directoryBrowseButton.Width = buttonOk.Width;
            directoryBrowseButton.Anchor = AnchorStyles.Right | AnchorStyles.Bottom;
            directoryBrowseButton.Click += new EventHandler(OnDirectoryBrowse);
            Controls.Add(directoryBrowseButton);

            wixDestinationTextLabel = new Label();
            wixDestinationTextLabel.Text = "Project will be created at:";
            wixDestinationTextLabel.Top = directoryName.Bottom + 2;
            wixDestinationTextLabel.Left = padding;
            wixDestinationTextLabel.Height = 14;
            wixDestinationTextLabel.Width = ClientSize.Width - 2*padding;
            wixDestinationTextLabel.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Bottom;
            Controls.Add(wixDestinationTextLabel);

            wixDestinationLabel = new Label();
            wixDestinationLabel.Text = "";
            wixDestinationLabel.Top = wixDestinationTextLabel.Bottom + 2;
            wixDestinationLabel.Left = padding;
            wixDestinationLabel.Height = 14;
            wixDestinationLabel.Width = ClientSize.Width - 2*padding;
            wixDestinationLabel.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Bottom;
            Controls.Add(wixDestinationLabel);

            installerTypeLabel = new Label();
            installerTypeLabel.Text = "Choose Installer Type:";
            installerTypeLabel.Top = wixDestinationLabel.Bottom + 8;
            installerTypeLabel.Left = padding;
            installerTypeLabel.Height = 14;
            installerTypeLabel.Width = ClientSize.Width - 2*padding;
            installerTypeLabel.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Bottom;
            Controls.Add(installerTypeLabel);

            productRadioButton = new RadioButton();
            productRadioButton.Text = "Product";
            productRadioButton.Tag = "Product";
            productRadioButton.Top = installerTypeLabel.Bottom;
            productRadioButton.Left = 20;
            productRadioButton.Width = ClientSize.Width - 20 - padding;
            productRadioButton.Height = 14;
            productRadioButton.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Bottom;
            productRadioButton.Checked = true;
            Controls.Add(productRadioButton);

            moduleRadioButton = new RadioButton();
            moduleRadioButton.Text = "Module";
            moduleRadioButton.Tag = "Module";
            moduleRadioButton.Top = productRadioButton.Bottom;
            moduleRadioButton.Left = 20;
            moduleRadioButton.Width = ClientSize.Width - 20 - padding;
            moduleRadioButton.Height = 14;
            moduleRadioButton.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Bottom;
            Controls.Add(moduleRadioButton);


            buttonCancel.Left = ClientSize.Width - buttonCancel.Width - padding;
            buttonOk.Left = buttonCancel.Left - buttonOk.Width - 2*padding;

            buttonCancel.Top = moduleRadioButton.Bottom;
            buttonOk.Top = moduleRadioButton.Bottom;

            FormBorderStyle = FormBorderStyle.SizableToolWindow;

            AcceptButton = buttonOk;
            CancelButton = buttonCancel;

            MaximizeBox = false;
            MinimizeBox = false;
            ControlBox = true; 

            StartPosition = FormStartPosition.CenterParent;

            Activated += new EventHandler(IsActivated);

            wixFileName.SelectAll();

            CheckEnableOkButton();

            wixFileName.Select();
        }

        private void IsActivated(object sender, EventArgs e) {

        }

        private void OnOk(object sender, EventArgs e) {
            string wixFile = Path.Combine(directoryName.Text, wixFileName.Text + "\\" + wixFileName.Text + ".wxs");
            if (File.Exists(wixFile)) {
                MessageBox.Show(wixFile + " Does already exist.");

                return;
            }

            if(Directory.Exists(Path.Combine(directoryName.Text, wixFileName.Text)) == false) {
                Directory.CreateDirectory(Path.Combine(directoryName.Text, wixFileName.Text));
            }

            DialogResult = DialogResult.OK;

            XmlDocument wixXmlDoc = new XmlDocument();
            wixXmlDoc.AppendChild(wixXmlDoc.CreateXmlDeclaration("1.0", "utf-8", null));
            XmlNamespaceManager wxsNsmgr = new XmlNamespaceManager(wixXmlDoc.NameTable);
            wxsNsmgr.AddNamespace("wix", "http://schemas.microsoft.com/wix/2003/01/wi");

            string installerType;
            if (moduleRadioButton.Checked) {
                installerType = moduleRadioButton.Tag as string;
            } else {
                installerType = productRadioButton.Tag as string;
            }

            XmlNode rootNode = wixXmlDoc.CreateElement("Wix", "http://schemas.microsoft.com/wix/2003/01/wi");
            wixXmlDoc.AppendChild(rootNode);
            XmlNode installationTypeNode = wixXmlDoc.CreateElement(installerType, "http://schemas.microsoft.com/wix/2003/01/wi");
            rootNode.AppendChild(installationTypeNode);

            if (templateList.SelectedItem.ToString() != "<none>") {
                XmlDocument includeXmlDoc = new XmlDocument();
                string currentTemplateDir = Path.Combine(WixEditSettings.Instance.TemplateDirectory, templateList.SelectedItem.ToString());
                string includeFile = Path.Combine(currentTemplateDir, templateList.SelectedItem.ToString() + ".wxi");
    
                includeXmlDoc.Load(includeFile);
    
                XmlNamespaceManager wxsIncludeNsmgr = new XmlNamespaceManager(includeXmlDoc.NameTable);
                wxsIncludeNsmgr.AddNamespace("wix", "http://schemas.microsoft.com/wix/2003/01/wi");
    
                XmlNodeList nodesToImport = includeXmlDoc.SelectNodes("/wix:Include/*", wxsIncludeNsmgr);
    
                foreach (XmlNode nodeToImport in nodesToImport) {
                    XmlNode importedNode = wixXmlDoc.ImportNode(nodeToImport, true);
                    installationTypeNode.AppendChild(importedNode);
                }
    
                foreach (string file in Directory.GetFiles(currentTemplateDir)) {
                    if (file == includeFile) {
                        continue;
                    }
    
                    FileInfo info = new FileInfo(file);
                    info.CopyTo(Path.Combine(directoryName.Text, info.Name), true);
                }
    
                foreach (string subDir in Directory.GetDirectories(currentTemplateDir)) {
                    DirectoryInfo info = new DirectoryInfo(subDir);
                    CopyDirectory(subDir, Path.Combine(directoryName.Text, info.Name));
                }
            }

            wixXmlDoc.Save(wixFile);

            newFileName = wixFile;
        }

        
        public void CopyDirectory(string sourceDirectory, string destinationDirectory) {
            if (Directory.Exists(destinationDirectory) == false) {
                Directory.CreateDirectory(destinationDirectory);
            }
            foreach(string fileName in Directory.GetFiles(sourceDirectory)) {
                FileInfo info = new FileInfo(fileName);
                File.Copy(fileName, Path.Combine(destinationDirectory, info.Name), true);
            }

            foreach(string directoryName in Directory.GetDirectories(sourceDirectory)) {
                DirectoryInfo info = new DirectoryInfo(directoryName);
                CopyDirectory(Path.Combine(sourceDirectory, info.Name), directoryName);
            }
        }


        private void OnDirectoryBrowse(object sender, EventArgs e) {
            FolderBrowserDialog dialog = new FolderBrowserDialog();
            dialog.Description = "Select the directory where you want to create your new Wix project.";

            // Allow the user to create new files via the FolderBrowserDialog.
            dialog.ShowNewFolderButton = true;

            dialog.SelectedPath = directoryName.Text;

            DialogResult result = dialog.ShowDialog();
            if(result == DialogResult.OK) {
                directoryName.Text = dialog.SelectedPath;
                CheckEnableOkButton();
            }
        }

        private void OnCheckEnableOkButton(object sender, EventArgs e) {
            CheckEnableOkButton();
        }

        private void CheckEnableOkButton() {
            if (wixDestinationLabel != null) {
                if (directoryName.Text != null && directoryName.Text.Length > 0 &&
                    wixFileName.Text != null && wixFileName.Text.Length > 0) {
                    wixDestinationLabel.Text = Path.Combine(directoryName.Text, wixFileName.Text + "\\" + wixFileName.Text + ".wxs");
                } else {
                    wixDestinationLabel.Text = "";
                }
            }

            if (directoryName.Text.Length > 0 &&
                Directory.Exists(directoryName.Text) &&
                wixFileName.Text.Length > 0) {
                buttonOk.Enabled = true;
            } else {
                buttonOk.Enabled = false;
            }
        }

        public string NewFileName {
            get {
                return newFileName;
            }
        }
    }
}