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
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Collections;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Data;
using System.Diagnostics;
using System.Xml;
using System.IO;
using System.Resources;
using System.Reflection;
using System.Text;
using System.Threading;

using WixEdit.PropertyGridExtensions;

namespace WixEdit {
    /// <summary>
    /// Summary description for OutputPanel.
    /// </summary>
    public class OutputPanel : BasePanel{
        protected RichTextBox richTextBox;
        protected Process activeProcess;
        Button closeButton;
        Label outputLabel;

        public OutputPanel(WixFiles wixFiles) : base(wixFiles) {
            InitializeComponent();
        }

        public RichTextBox RichTextBox {
            get { return richTextBox; }
        }

        public event EventHandler CloseClicked;

        private void InitializeComponent() {
            this.TabStop = true;
            int buttonWidth = 11;
            int buttonHeigth = 11;
            int paddingX = 2;
            int paddingY = 2;

            closeButton = new Button();
            richTextBox = new RichTextBox();
            outputLabel = new Label();

            closeButton.Size = new Size(buttonWidth, buttonHeigth);
            closeButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            closeButton.Location = new Point(this.ClientSize.Width - buttonWidth - 2*paddingX, paddingY);
            closeButton.BackColor = Color.Transparent;
            closeButton.Click += new EventHandler(OnCloseClick);


            outputLabel.Text = "WIX Compiler output";
            outputLabel.Font = new Font("Tahoma", 8.25F, FontStyle.Bold, GraphicsUnit.Point, ((System.Byte)(0)));;
            outputLabel.BorderStyle = BorderStyle.FixedSingle;

            Bitmap bmp = new Bitmap(WixFiles.GetResourceStream("WixEdit.close_8x8.bmp"));
            bmp.MakeTransparent();
            closeButton.Image = bmp;
            closeButton.FlatStyle = FlatStyle.Flat;

            outputLabel.Size = new Size(ClientSize.Width - 2*paddingX, buttonHeigth+ (2*paddingY));
            outputLabel.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            outputLabel.Location = new Point(paddingX, 0);

            outputLabel.BackColor = Color.Gray;
            outputLabel.ForeColor = Color.LightGray;


            richTextBox.Dock = DockStyle.Bottom;
            richTextBox.Location = new Point(0, buttonHeigth + 3*paddingY);
            richTextBox.Size = new Size(200, this.ClientSize.Height - richTextBox.Location.Y);
            richTextBox.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            richTextBox.ScrollBars = RichTextBoxScrollBars.Both;
            richTextBox.WordWrap = false;

            this.Controls.Add(closeButton);
            this.Controls.Add(outputLabel);
            this.Controls.Add(richTextBox);

            closeButton.TabStop = true;
            outputLabel.TabStop = true;
            richTextBox.TabStop = true;

            closeButton.LostFocus += new EventHandler(HasFocus);
            outputLabel.LostFocus += new EventHandler(HasFocus);
            richTextBox.LostFocus += new EventHandler(HasFocus);

            closeButton.LostFocus += new EventHandler(HasFocus);
            outputLabel.LostFocus += new EventHandler(HasFocus);
            richTextBox.LostFocus += new EventHandler(HasFocus);

            closeButton.Enter += new EventHandler(HasFocus);
            outputLabel.Enter += new EventHandler(HasFocus);
            richTextBox.Enter += new EventHandler(HasFocus);

            closeButton.Click += new EventHandler(HasFocus);
            outputLabel.Click += new EventHandler(HasFocus);
            richTextBox.Click += new EventHandler(HasFocus);
        }

        protected void OnCloseClick(Object sender, EventArgs e) {
            if (CloseClicked != null) {
                CloseClicked(sender, e);
            }
        }

        protected void HasFocus(Object sender, EventArgs e) {
            if (closeButton.Focused ||
                outputLabel.Focused ||
                richTextBox.Focused || richTextBox.Capture) {
                outputLabel.BackColor = Color.DimGray;
                outputLabel.ForeColor = Color.White;
            } else {
                outputLabel.BackColor = Color.Gray;
                outputLabel.ForeColor = Color.LightGray;
            }
        }

        public int Run(ProcessStartInfo[] processStartInfos) {
            richTextBox.Rtf = "";

            DateTime start = DateTime.Now;

            foreach (ProcessStartInfo processStartInfo in processStartInfos) {
                DateTime subStart = DateTime.Now;
                Output(String.Format("Starting {0} {1} at {2}", processStartInfo.FileName, processStartInfo.Arguments, subStart), true);
                Output("", true);
                
                activeProcess = Process.Start(processStartInfo);
                
                ReadStandardOut();
    
                activeProcess.WaitForExit();

                if (activeProcess.ExitCode != 0) {
                    break;
                }

                Output(String.Format("Done in: {0} seconds", activeProcess.ExitTime.Subtract(subStart).Seconds), true);
                Output("", true);
            }

            if (activeProcess.ExitCode != 0) {
                Output("Error in " + Path.GetFileNameWithoutExtension(activeProcess.StartInfo.FileName), true);
            } else {
                Output("Done in: " + activeProcess.ExitTime.Subtract(start).Seconds.ToString(), true);
            }

            return activeProcess.ExitCode;
        }
        
        public int Run(ProcessStartInfo processStartInfo) {
            DateTime start = DateTime.Now;

            Output(String.Format("Starting: {0} {1}", processStartInfo.FileName, processStartInfo.Arguments), true);
            Output("", true);
            
            activeProcess = Process.Start(processStartInfo);
            
            ReadStandardOut();

            activeProcess.WaitForExit();

            Output("Done in: " + activeProcess.ExitTime.Subtract(start).Seconds.ToString(), true);
            Output("", true);

            return activeProcess.ExitCode;
        }
        
       
        private void ReadStandardOut() {
            if ( activeProcess != null ) {
                string line;
                using (StreamReader sr = activeProcess.StandardOutput) {
                    while ((line = sr.ReadLine()) != null) {
                        Output(line, false);
                    }
                }
            }
        }

        private void Output(string message, bool bold) {
            string output;
            if (message == null || message.Length == 0) {
                output = "\\par\r\n";
            } else {
                string escaped = message.Replace("\\", "\\\\");
                if (bold == false) {
                    output = String.Format("{0}\\par\r\n", escaped);
                } else {
                    output = String.Format("\\cf1\\b {0}\\b0\\cf0\\par\r\n", escaped);
                }
            }

            richTextBox.Select(richTextBox.Text.Length, 0);
            richTextBox.SelectedRtf = String.Format(@"{{\rtf1\ansi\ansicpg1252\deff0\deflang1033{{\fonttbl{{\f0\fmodern\fprq1\fcharset0 Courier New;}}}}" +
                                            @"{{\colortbl ;\red255\green0\blue0;}}" +
                                            @"\viewkind4\uc1\pard\f0\fs16 {0}}}", output);

            richTextBox.Select(richTextBox.Text.Length, 0);
            richTextBox.Focus();
            richTextBox.ScrollToCaret();
        }

        public void Clear() {
            richTextBox.Text = "";
        }
    }
}
