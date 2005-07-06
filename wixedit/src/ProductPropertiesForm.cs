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

namespace WixEdit {
	/// <summary>
	/// ProductPropertiesForm edits the Attributes of the Product element.
	/// </summary>
	public class ProductPropertiesForm : Form {
        protected Button buttonOk;
//        protected Button buttonCancel;

        protected PropertyGrid productPropertyGrid;

        protected XmlNode productNode;
        protected WixFiles wixFiles;

		public ProductPropertiesForm(XmlNode productNode, WixFiles wixFiles) {
            this.productNode = productNode;
            this.wixFiles = wixFiles;

			InitializeComponent();
		}

        private void InitializeComponent() {
            this.Text = "Product Properties";
            this.ShowInTaskbar = false;
            this.SizeGripStyle = SizeGripStyle.Hide;

            this.buttonOk = new Button();
            this.buttonOk.Text = "Done";
            this.buttonOk.Anchor = AnchorStyles.Right | AnchorStyles.Bottom;
            this.buttonOk.FlatStyle = FlatStyle.System;
            this.buttonOk.Click += new EventHandler(OnOk);
            this.Controls.Add(this.buttonOk);

//            this.buttonCancel = new Button();
//            this.buttonCancel.Text = "Cancel";
//            this.buttonCancel.Anchor = AnchorStyles.Right | AnchorStyles.Bottom;
//            this.buttonCancel.FlatStyle = FlatStyle.System;
//            this.Controls.Add(this.buttonCancel);

            this.productPropertyGrid = new CustomPropertyGrid();
            this.productPropertyGrid.Font = new Font("Tahoma", 8.25F, FontStyle.Regular, GraphicsUnit.Point, ((System.Byte)(0)));
            this.productPropertyGrid.Name = "propertyGrid";
            this.productPropertyGrid.TabIndex = 1;
            this.productPropertyGrid.PropertySort = PropertySort.Alphabetical;
            this.productPropertyGrid.ToolbarVisible = false;
            this.productPropertyGrid.Dock = DockStyle.Top;
            this.productPropertyGrid.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Bottom | AnchorStyles.Top;
            this.Controls.Add(this.productPropertyGrid);

            this.ClientSize = new Size(384, 256);

            this.productPropertyGrid.Size = new Size(this.ClientSize.Width, this.ClientSize.Height-29);


//            this.buttonCancel.Left = this.ClientSize.Width - this.buttonCancel.Width;
//            this.buttonOk.Left = this.buttonCancel.Left - this.buttonOk.Width - 2;
            this.buttonOk.Left = this.ClientSize.Width - this.buttonOk.Width - 2;

//            this.buttonCancel.Top = this.productPropertyGrid.Top + this.productPropertyGrid.Height + 3;
            this.buttonOk.Top = this.productPropertyGrid.Top + this.productPropertyGrid.Height + 3;

            this.FormBorderStyle = FormBorderStyle.SizableToolWindow;

            this.AcceptButton = buttonOk;
//            this.CancelButton = buttonCancel;

            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.ControlBox = true; 

            this.StartPosition = FormStartPosition.CenterParent;

            this.Activated += new EventHandler(IsActivated);

            this.productPropertyGrid.SelectedObject = new XmlAttributeAdapter(productNode, wixFiles);
        }

        private void IsActivated(object sender, EventArgs e) {
            // this.StringEdit.Focus();
        }

        private void OnOk(object sender, EventArgs e) {
            this.DialogResult = DialogResult.OK;
        }
    }
}