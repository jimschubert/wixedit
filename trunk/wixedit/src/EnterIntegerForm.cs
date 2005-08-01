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
using System.Windows.Forms;

namespace WixEdit {
	/// <summary>
	/// Form to enter integers.
	/// </summary>
	public class EnterIntegerForm : EnterStringForm {
        public int SelectedInteger {
            get {
                return Int32.Parse(SelectedString);
            }
            set {
                SelectedString = value.ToString();
            }
        }

        protected override void OnOk(object sender, EventArgs e) {
            base.OnOk(sender, e);

            bool isOk = true;
            int selectedInteger = 0;
            try {
                selectedInteger = Int32.Parse(selectedString);
            } catch {
                isOk = false;
                MessageBox.Show("Invalid number.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            if (isOk) {
                if (selectedInteger <= 0) {
                    isOk = false;
                    MessageBox.Show("Number should be larger then 0.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

            if (isOk == false) {
                DialogResult = DialogResult.None;
            }
        }
    }
}