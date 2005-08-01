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
	public class EnterStringForm : Form {
        protected Button ButtonOk;
        protected Button ButtonCancel;
        protected TextBox StringEdit;

        protected string selectedString;

		public EnterStringForm() {
			InitializeComponent();
		}

        private void InitializeComponent() {
            Text = "New Property Name";
            ShowInTaskbar = false;

            ButtonOk = new Button();
            ButtonOk.Text = "Ok";
            ButtonOk.Dock = DockStyle.Left;
            ButtonOk.FlatStyle = FlatStyle.System;
            ButtonOk.Click += new EventHandler(OnOk);
            Controls.Add(ButtonOk);

            ButtonCancel = new Button();
            ButtonCancel.Text = "Cancel";
            ButtonCancel.Dock = DockStyle.Right;
            ButtonCancel.FlatStyle = FlatStyle.System;
            Controls.Add(ButtonCancel);

            StringEdit = new TextBox();
            StringEdit.Dock = DockStyle.Top;
            Controls.Add(StringEdit);
            StringEdit.Text = selectedString;
            StringEdit.Size = new Size(ButtonCancel.Width+2+ButtonOk.Width, 23);

            ClientSize = new Size(ButtonCancel.Width+2+ButtonOk.Width, 46);
            FormBorderStyle = FormBorderStyle.FixedToolWindow;

            AcceptButton = ButtonOk;
            CancelButton = ButtonCancel;

            MaximizeBox = false;
            MinimizeBox = false;
            ControlBox = false; 

            StartPosition = FormStartPosition.CenterParent;

            Activated += new EventHandler(IsActivated);
        }

        private void IsActivated(object sender, EventArgs e) {
            StringEdit.Focus();
        }

        public string SelectedString {
            get {
                return selectedString;
            }
            set {
                selectedString = value;
                StringEdit.Text = selectedString;
            }
        }

        protected virtual void OnOk(object sender, EventArgs e) {
            selectedString = StringEdit.Text;
            DialogResult = DialogResult.OK;
        }
    }
}