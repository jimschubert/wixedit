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
        
        public XmlDisplayForm() {

            InitializeComponent();
		}

        public string Url {
            get { return url; }
        }

        private void InitializeComponent() {
            this.Icon = new Icon(WixFiles.GetResourceStream("dialog.source.ico"));
            this.ClientSize = new System.Drawing.Size(800, 600);

            webBrowser = new AxWebBrowser();

            webBrowser.BeginInit();

            webBrowser.TabIndex = 1;
            //AxWebBrowser.Anchor = AnchorStyles.All;
            webBrowser.Dock = DockStyle.Fill;
            
            this.Controls.Add(webBrowser);
            webBrowser.EndInit();
      
            webBrowser.RegisterAsBrowser = true;
            webBrowser.RegisterAsDropTarget = true;
            webBrowser.Silent = false;
        }
            
        protected override void OnClosed(EventArgs e) {
            if (webBrowser != null) {
                this.Controls.Remove(webBrowser);
                webBrowser.Dispose();
                webBrowser = null;
            }
            base.OnClosed(e);
        }

        public void ShowFile(string url) {
            this.url = url;
            object o = null;
            webBrowser.Navigate(url, ref o, ref o, ref o, ref o);
        }
    }
}