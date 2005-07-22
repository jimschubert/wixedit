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
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using System.Reflection;

namespace WixEdit.About {
	/// <summary>
	/// Summary description for AboutForm.
	/// </summary>
	public class AboutForm : Form 	{
        Image splashScreen;
        double imageScale = 0.5;

        Label versionLabel;
        Label copyrightLabel;
        LinkLabel urlLabel;

        string versionFormatString = "WiX Edit v{0}";
        string copyright = "Copyright (c) 2005 J.Keuper. All rights reserved";
        string url = "http://wixedit.sourceforge.net/";


        // There are 2 kinds of transparency:
        // 1) The transparency copies the current background (other dialogs/desktop) and shows this so it looks transparent
        //    but isn't. This allows shadow-like images, that is from 0% to 100% transparent. However this way fails to update 
        //    the transparent part properly when other dialogs move over it, or the background changes.
        // 2) "Real" transparency. This way a color is specified and that color will be shown transparent. So shadow-like images
        //    are not possible. The shadow will not be transparent, but will be blended with the transparency color. (glow-like)
        //
        // Set useRealTransparency to true when you want to use the second option.

        bool useRealTransparency = true;
        Color realTransparencyColor = Color.White;

        public AboutForm() {
            Initialize();
        }

        private void Initialize() {
            this.Font = new Font("Tahoma", 8.25F, FontStyle.Regular, GraphicsUnit.Point, ((Byte)(0)));

            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.None;
            this.ShowInTaskbar = false;


            this.Width = 320;
            this.Height = 160;

            splashScreen = Image.FromStream(WixFiles.GetResourceStream("About.png"));

            int labelHeight = 16;

            versionLabel = new Label();
            versionLabel.Text = String.Format(versionFormatString, Assembly.GetExecutingAssembly().GetName().Version.ToString());
            versionLabel.Left = 10;
            versionLabel.Top = 86;
            versionLabel.Width = 300;
            versionLabel.Height = labelHeight;
            versionLabel.BackColor = Color.Transparent;
            versionLabel.Click += new EventHandler(OnClose);

            this.Controls.Add(versionLabel);

            copyrightLabel = new Label();
            copyrightLabel.Text = copyright;
            copyrightLabel.Left = 10;
            copyrightLabel.Top = versionLabel.Top+versionLabel.Height;
            copyrightLabel.Width = 300;
            copyrightLabel.Height = labelHeight;
            copyrightLabel.BackColor = Color.Transparent;
            copyrightLabel.Click += new EventHandler(OnClose);

            this.Controls.Add(copyrightLabel);

            urlLabel = new LinkLabel();
            urlLabel.Text = url;
            urlLabel.Left = 10;
            urlLabel.Top = copyrightLabel.Top+copyrightLabel.Height;
            urlLabel.Width = 160;
            urlLabel.Height = labelHeight;
            urlLabel.BackColor = Color.Transparent;

            urlLabel.Links.Add(0, url.Length, url);
            urlLabel.LinkClicked += new LinkLabelLinkClickedEventHandler(this.OnUrlClicked);

            this.Controls.Add(urlLabel);

            this.Click += new EventHandler(OnClose);

            // Make the background color of form display transparently.
            // The image with the semi transparant stuff, will use this color (glow-like)
            if (useRealTransparency) {
                this.BackColor = realTransparencyColor;
                this.TransparencyKey = BackColor;
            }
        }

        private void OnUrlClicked(object sender, LinkLabelLinkClickedEventArgs e) {
            // Determine which link was clicked within the LinkLabel.
            urlLabel.Links[urlLabel.Links.IndexOf(e.Link)].Visited = true;
    
            // Display the appropriate link based on the value of the 
            // LinkData property of the Link object.
            string target = e.Link.LinkData as string;

            // Navigate to it.
            try {
                Process.Start(target);
            } catch (Win32Exception) {
                // Workaround for:
                // "Win32Exception: The requested lookup key was not found in any active activation context"   
                Process process = new Process();
                process.StartInfo.FileName = "cmd.exe"; // Win2K+
                process.StartInfo.Arguments = "/c start " + target;
                process.StartInfo.CreateNoWindow = true;
                process.StartInfo.UseShellExecute = false;
                process.Start();
            }
        }

        public void OnClose(object sender, EventArgs e) {
            this.Close();
        }

        protected override void OnPaintBackground(PaintEventArgs args) {
            Graphics gfx = args.Graphics;
            if (useRealTransparency) {
                gfx.Clear(realTransparencyColor);
            }
            gfx.DrawImage(splashScreen, new Rectangle(0, 0, 
                                        (int) Math.Round(splashScreen.Width*imageScale), 
                                        (int) Math.Round(splashScreen.Height*imageScale)));
        }
    }
}