// Copyright (c) 2005 J.Keuper (j.keuper@gmail.com)
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to 
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
using System.IO;
using System.Windows.Forms;
using System.Drawing;
using AxSHDocVw;

namespace WixEdit {
	/// <summary>
	/// A Html browser dialog for showing xml
	/// </summary>
	public class XmlDisplayForm : Form 	{
        protected AxWebBrowser webBrowser;
        protected string url;

        protected static bool hasAxshdocvwLoadFailure = false;
        
        public XmlDisplayForm() {
            VisibleChanged += new EventHandler(VisibleChangedHandler);

            try {
                InitializeComponent();
            } catch (FileNotFoundException ex) {
                if (ex.FileName.ToLower().IndexOf("axshdocvw") >= 0) {
                    if (hasAxshdocvwLoadFailure == false) {
                        hasAxshdocvwLoadFailure = true;
    
                        ShowAxshdocvwLoadFailureMessage();
                    }
                } else {
                    throw;
                }
            }
		}

        public bool HasAxshdocvwLoadFailure {
            get {
                return hasAxshdocvwLoadFailure;
            }
        }

        protected void VisibleChangedHandler(object sender, EventArgs e) {
            if (hasAxshdocvwLoadFailure && this.Visible) {
                ShowAxshdocvwLoadFailureMessage();
                this.Visible = false;
            }
        }

        public void ShowAxshdocvwLoadFailureMessage() {
            MessageBox.Show("Unable to load assembly \"AxSHDocVw.dll\"! Please make sure it is present next to WixEdit executable. It is not possible to view the source of any compile errors or warnings in the WixEdit source viewer.", "Failed to load: AxSHDocVw", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        public string Url {
            get { return url; }
        }

        private void InitializeComponent() {
            Icon = new Icon(WixFiles.GetResourceStream("dialog.source.ico"));
            ClientSize = new System.Drawing.Size(800, 600);

            webBrowser = new AxWebBrowser();

            webBrowser.BeginInit();

            webBrowser.TabIndex = 1;
            //AxWebBrowser.Anchor = AnchorStyles.All;
            webBrowser.Dock = DockStyle.Fill;
            
            Controls.Add(webBrowser);
            webBrowser.EndInit();
      
            webBrowser.RegisterAsBrowser = true;
            webBrowser.RegisterAsDropTarget = true;
            webBrowser.Silent = false;
        }
            
        protected override void OnClosed(EventArgs e) {
            if (webBrowser != null) {
                Controls.Remove(webBrowser);
                webBrowser.Dispose();
                webBrowser = null;
            }
            base.OnClosed(e);
        }

        public void ShowFile(string url) {
            this.url = url;
            object o = null;

            if (hasAxshdocvwLoadFailure == false) {
                webBrowser.Navigate(url, ref o, ref o, ref o, ref o);
            }
        }
    }
}