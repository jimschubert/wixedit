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

namespace WixEdit {
	/// <summary>
	/// Summary description for Form1.
	/// </summary>
	public class SelectStringForm : Form {
        protected Button ButtonOk;
        protected Button ButtonCancel;
        protected ListBox StringList;

        protected string selectedString;
        protected string[] possibleStrings;

		public SelectStringForm() {
			InitializeComponent();
		}

        private void InitializeComponent() {
            this.Text = "New Attribute Name";
            this.ShowInTaskbar = false;

            this.ButtonOk = new Button();
            this.ButtonOk.Text = "Ok";
            this.ButtonOk.Dock = DockStyle.Left;
            this.ButtonOk.FlatStyle = FlatStyle.System;
            this.ButtonOk.Click += new EventHandler(OnOk);
            this.ButtonOk.Enabled = false;
            this.Controls.Add(this.ButtonOk);

            this.ButtonCancel = new Button();
            this.ButtonCancel.Text = "Cancel";
            this.ButtonCancel.Dock = DockStyle.Right;
            this.ButtonCancel.FlatStyle = FlatStyle.System;
            this.Controls.Add(this.ButtonCancel);

            this.StringList = new ListBox();
            this.StringList.Dock = DockStyle.Top;
            this.StringList.SelectionMode = SelectionMode.One;
            this.StringList.DoubleClick += new EventHandler(OnDoubleClickList);
            this.StringList.SelectedValueChanged += new EventHandler(OnSelectionChanged);
            this.Controls.Add(this.StringList);

            this.StringList.Size = new Size(this.ButtonCancel.Width+2+this.ButtonOk.Width, 237);

            this.ClientSize = new Size(this.ButtonCancel.Width+2+this.ButtonOk.Width, 250);
            this.FormBorderStyle = FormBorderStyle.FixedToolWindow;

            this.AcceptButton = ButtonOk;
            this.CancelButton = ButtonCancel;

            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.ControlBox = false; 

            this.StartPosition = FormStartPosition.CenterParent;

            this.Activated += new EventHandler(OnActivate);
        }

        private void OnActivate(object sender, EventArgs e) {   
            this.StringList.Items.Clear();
            foreach (string it in possibleStrings) {
                this.StringList.Items.Add(it);
            }

            UpdateOkButton();
        }

        private void OnSelectionChanged(object sender, EventArgs e) {
            UpdateOkButton();
        }

        private void UpdateOkButton() {
            if (this.StringList.SelectedItem == null) {
                this.ButtonOk.Enabled = false;
            } else {
                this.ButtonOk.Enabled = true;
            }
        }        

        public string SelectedString {
            get {
                return this.selectedString;
            }
            set {
                this.selectedString = value;
            }
        }

        public string[] PossibleStrings {
            get {
                return this.possibleStrings;
            }
            set {
                this.possibleStrings = value;
            }
        }

        private void OnOk(object sender, EventArgs e) {   
            this.selectedString = this.StringList.SelectedItem.ToString();
            this.DialogResult = DialogResult.OK;
        }

        private void OnDoubleClickList(object sender, EventArgs e) {   
            this.selectedString = this.StringList.SelectedItem.ToString();
            this.DialogResult = DialogResult.OK;
        }
    }
}