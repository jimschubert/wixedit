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

        protected Label installerTypeLabel;
        protected RadioButton productRadioButton;
        protected RadioButton moduleRadioButton;

        protected string newFileName;

		public NewProjectForm() {
			InitializeComponent();
		}

        private void InitializeComponent() {
            int padding = 2;

            this.Text = "Create new WiX file";
            this.ShowInTaskbar = false;

            this.ClientSize = new Size(384, 239);
            this.SizeGripStyle = SizeGripStyle.Hide;


            this.buttonOk = new Button();
            this.buttonOk.Text = "Ok";
            this.buttonOk.Anchor = AnchorStyles.Right | AnchorStyles.Bottom;
            this.buttonOk.FlatStyle = FlatStyle.System;
            this.buttonOk.Click += new EventHandler(OnOk);
            this.Controls.Add(this.buttonOk);

            this.buttonCancel = new Button();
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.Anchor = AnchorStyles.Right | AnchorStyles.Bottom;
            this.buttonCancel.FlatStyle = FlatStyle.System;
            this.Controls.Add(this.buttonCancel);

            this.templateListLabel = new Label();
            this.templateListLabel.Text = "Select template:";
            this.templateListLabel.Top = 0;
            this.templateListLabel.Left = padding;
            this.templateListLabel.Height = 14;
            this.templateListLabel.Width = ClientSize.Width - 2*padding;
            this.templateListLabel.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Top;
            this.Controls.Add(this.templateListLabel);

            this.templateList = new ListBox();
            this.templateList.Font = new Font("Tahoma", 8.25F, FontStyle.Regular, GraphicsUnit.Point, ((System.Byte)(0)));
            this.templateList.Name = "templateList";
            this.templateList.TabIndex = 1;
            this.templateList.Top = templateListLabel.Bottom;
            this.templateList.Left = padding;
            this.templateList.Height = ClientSize.Height - 157;
            this.templateList.Width = ClientSize.Width - 2*padding;
            this.templateList.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Bottom | AnchorStyles.Top;
            this.Controls.Add(this.templateList);

            this.templateList.Items.Add("<none>");
            this.templateList.SelectedIndex = 0;
            if (WixEditSettings.Instance.TemplateDirectory != null && Directory.Exists(WixEditSettings.Instance.TemplateDirectory)) {
                foreach (string dir in Directory.GetDirectories(WixEditSettings.Instance.TemplateDirectory)) {
                    DirectoryInfo dirInfo = new DirectoryInfo(dir);
                    this.templateList.Items.Add(dirInfo.Name);
                }
            }

            this.wixFileNameLabel = new Label();
            this.wixFileNameLabel.Text = "Enter WiX filename:";
            this.wixFileNameLabel.Top = templateList.Bottom + 2;
            this.wixFileNameLabel.Left = padding;
            this.wixFileNameLabel.Height = 14;
            this.wixFileNameLabel.Width = ClientSize.Width - 2*padding;
            this.wixFileNameLabel.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Bottom;
            this.Controls.Add(this.wixFileNameLabel);
            
            this.wixFileName = new TextBox();
            this.wixFileName.Top = wixFileNameLabel.Bottom;
            this.wixFileName.Left = padding;
            this.wixFileName.Width = ClientSize.Width - 2*padding;
            this.wixFileName.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Bottom;
            this.wixFileName.Text = "NewFile.wxs";
            this.wixFileName.TextChanged += new EventHandler(OnCheckEnableOkButton);
            this.Controls.Add(this.wixFileName);

            this.directoryNameLabel = new Label();
            this.directoryNameLabel.Text = "Select directory:";
            this.directoryNameLabel.Top = wixFileName.Bottom + 2;
            this.directoryNameLabel.Left = padding;
            this.directoryNameLabel.Height = 14;
            this.directoryNameLabel.Width = ClientSize.Width - 2*padding;
            this.directoryNameLabel.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Bottom;
            this.Controls.Add(this.directoryNameLabel);

            this.directoryName = new TextBox();
            this.directoryName.Top = directoryNameLabel.Bottom;
            this.directoryName.Left = padding;
            this.directoryName.Width = ClientSize.Width - buttonOk.Width - 3*padding;
            this.directoryName.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Bottom;
            this.directoryName.TextChanged += new EventHandler(OnCheckEnableOkButton);
            this.Controls.Add(this.directoryName);

            this.directoryBrowseButton = new Button();
            this.directoryBrowseButton.Text = "Browse";
            this.directoryBrowseButton.FlatStyle = FlatStyle.System;
            this.directoryBrowseButton.Top = directoryNameLabel.Bottom;
            this.directoryBrowseButton.Height = wixFileName.Height;
            this.directoryBrowseButton.Left = this.ClientSize.Width - this.directoryBrowseButton.Width - padding;
            this.directoryBrowseButton.Width = buttonOk.Width;
            this.directoryBrowseButton.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Bottom;
            this.directoryBrowseButton.Click += new EventHandler(OnDirectoryBrowse);
            this.Controls.Add(this.directoryBrowseButton);

            this.installerTypeLabel = new Label();
            this.installerTypeLabel.Text = "Choose Installer Type:";
            this.installerTypeLabel.Top = directoryName.Bottom + 4;
            this.installerTypeLabel.Left = padding;
            this.installerTypeLabel.Height = 14;
            this.installerTypeLabel.Width = ClientSize.Width - 2*padding;
            this.installerTypeLabel.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Bottom;
            this.Controls.Add(this.installerTypeLabel);

            this.productRadioButton = new RadioButton();
            this.productRadioButton.Text = "Product";
            this.productRadioButton.Tag = "Product";
            this.productRadioButton.Top = this.installerTypeLabel.Bottom;
            this.productRadioButton.Left = 20;
            this.productRadioButton.Width = ClientSize.Width - 20 - padding;
            this.productRadioButton.Height = 14;
            this.productRadioButton.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Bottom;
            this.productRadioButton.Checked = true;
            this.Controls.Add(this.productRadioButton);

            this.moduleRadioButton = new RadioButton();
            this.moduleRadioButton.Text = "Module";
            this.moduleRadioButton.Tag = "Module";
            this.moduleRadioButton.Top = this.productRadioButton.Bottom;
            this.moduleRadioButton.Left = 20;
            this.moduleRadioButton.Width = ClientSize.Width - 20 - padding;
            this.moduleRadioButton.Height = 14;
            this.moduleRadioButton.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Bottom;
            this.Controls.Add(this.moduleRadioButton);


            this.buttonCancel.Left = this.ClientSize.Width - this.buttonCancel.Width - padding;
            this.buttonOk.Left = this.buttonCancel.Left - this.buttonOk.Width - 2*padding;

            this.buttonCancel.Top = moduleRadioButton.Bottom;
            this.buttonOk.Top = moduleRadioButton.Bottom;

            this.FormBorderStyle = FormBorderStyle.SizableToolWindow;

            this.AcceptButton = buttonOk;
            this.CancelButton = buttonCancel;

            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.ControlBox = true; 

            this.StartPosition = FormStartPosition.CenterParent;

            this.Activated += new EventHandler(IsActivated);

            this.wixFileName.SelectAll();

            CheckEnableOkButton();

            this.wixFileName.Select();
        }

        private void IsActivated(object sender, EventArgs e) {

        }

        private void OnOk(object sender, EventArgs e) {
            string wixFile = Path.Combine(directoryName.Text, wixFileName.Text);
            if (Path.GetExtension(wixFile).Length == 0) {
                if (wixFile.EndsWith(".") == false) {
                    wixFile = wixFile + ".";
                }

                wixFile = wixFile + "wxs";
            }

            if (File.Exists(wixFile)) {
                MessageBox.Show(wixFile + " Does already exist.");

                return;
            }

            this.DialogResult = DialogResult.OK;

            XmlDocument wixXmlDoc = new XmlDocument();
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

            // Default to the My Documents folder.
            dialog.RootFolder = Environment.SpecialFolder.MyComputer;

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