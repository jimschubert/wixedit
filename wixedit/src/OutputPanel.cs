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
using System.Xml.Xsl;

namespace WixEdit {
    /// <summary>
    /// Summary description for OutputPanel.
    /// </summary>
    public class OutputPanel : Panel {
        protected OutputTextbox outputTextBox;
        protected Process activeProcess;
        Button closeButton;
        Label outputLabel;

        private System.Windows.Forms.Timer doubleClickTimer = new System.Windows.Forms.Timer();
        private bool isFirstClick = true;
        private int milliseconds = 0;

        XmlDisplayForm xmlDisplayForm = new XmlDisplayForm();

        public OutputPanel() {
            InitializeComponent();
        }

        public RichTextBox RichTextBox {
            get { return outputTextBox; }
        }

        public event EventHandler CloseClicked;

        private void InitializeComponent() {
            TabStop = true;
            int buttonWidth = 11;
            int buttonHeigth = 11;
            int paddingX = 2;
            int paddingY = 2;

            closeButton = new Button();
            outputTextBox = new OutputTextbox();
            outputLabel = new Label();

            closeButton.Size = new Size(buttonWidth, buttonHeigth);
            closeButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            closeButton.Location = new Point(ClientSize.Width - buttonWidth - 2*paddingX, paddingY);
            closeButton.BackColor = Color.Transparent;
            closeButton.Click += new EventHandler(OnCloseClick);


            outputLabel.Text = "WIX Compiler output";
            outputLabel.Font = new Font("Tahoma", 8.25F, FontStyle.Bold, GraphicsUnit.Point, ((System.Byte)(0)));;
            outputLabel.BorderStyle = BorderStyle.FixedSingle;

            Bitmap bmp = new Bitmap(WixFiles.GetResourceStream("close_8x8.bmp"));
            bmp.MakeTransparent();
            closeButton.Image = bmp;
            closeButton.FlatStyle = FlatStyle.Flat;

            outputLabel.Size = new Size(ClientSize.Width - 2*paddingX, buttonHeigth+ (2*paddingY));
            outputLabel.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            outputLabel.Location = new Point(paddingX, 0);

            outputLabel.BackColor = Color.Gray;
            outputLabel.ForeColor = Color.LightGray;


            outputTextBox.Dock = DockStyle.Bottom;
            outputTextBox.Location = new Point(0, buttonHeigth + 3*paddingY);
            outputTextBox.Size = new Size(200, ClientSize.Height - outputTextBox.Location.Y);
            outputTextBox.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            outputTextBox.ScrollBars = RichTextBoxScrollBars.Both;
            outputTextBox.WordWrap = false;
            outputTextBox.AllowDrop = false;

            Controls.Add(closeButton);
            Controls.Add(outputLabel);
            Controls.Add(outputTextBox);


            closeButton.TabStop = true;
            outputLabel.TabStop = true;
            outputTextBox.TabStop = true;

            closeButton.LostFocus += new EventHandler(HasFocus);
            outputLabel.LostFocus += new EventHandler(HasFocus);
            outputTextBox.LostFocus += new EventHandler(HasFocus);

            closeButton.GotFocus += new EventHandler(HasFocus);
            outputLabel.GotFocus += new EventHandler(HasFocus);
            outputTextBox.GotFocus += new EventHandler(HasFocus);

            closeButton.Enter += new EventHandler(HasFocus);
            outputLabel.Enter += new EventHandler(HasFocus);
            outputTextBox.Enter += new EventHandler(HasFocus);

            closeButton.Click += new EventHandler(HasFocus);
            outputLabel.Click += new EventHandler(HasFocus);

            outputTextBox.MouseUp += new MouseEventHandler(outputTextBox_MouseDown);


            doubleClickTimer.Interval = 100;
            doubleClickTimer.Tick += new EventHandler(doubleClickTimer_Tick);
        }

        protected void OnCloseClick(Object sender, EventArgs e) {
            if (CloseClicked != null) {
                CloseClicked(sender, e);
            }
        }

        private void outputTextBox_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e) {
            HasFocus(sender, e);
            
            // This is the first mouse click.
            if (isFirstClick) {
                isFirstClick = false;

                // Start the double click timer.
                doubleClickTimer.Start();
            } else { // This is the second mouse click.
                // Verify that the mouse click is within the double click
                // rectangle and is within the system-defined double 
                // click period.
                if (milliseconds < SystemInformation.DoubleClickTime) {
                    OpenLine(e.X, e.Y);
                }
            }
        }


        void doubleClickTimer_Tick(object sender, EventArgs e) {
            milliseconds += 100;

            // The timer has reached the double click time limit.
            if (milliseconds >= SystemInformation.DoubleClickTime) {
                doubleClickTimer.Stop();

                // Allow the MouseDown event handler to process clicks again.
                isFirstClick = true;
                milliseconds = 0;
            }
        }

        private void OpenLine(int x, int y) {
            // Obtain the character index at which the mouse cursor was clicked at.
            int currentIndex = outputTextBox.GetCharIndexFromPosition(new Point(x, y));
            int currentLine = outputTextBox.GetLineFromCharIndex(currentIndex);

            int lineCount = outputTextBox.Lines.Length;

            int beginLineIndex = currentIndex;
            if (currentLine == 0) {
                beginLineIndex = 0;
            } else {
                while (currentLine == outputTextBox.GetLineFromCharIndex(beginLineIndex - 1) && 
                       currentLine != 0) {
                    beginLineIndex--;
                }
            }

            outputTextBox.SuspendLayout();
            outputTextBox.HideSelection = true;

            outputTextBox.Select(0, outputTextBox.TextLength);       
            outputTextBox.SelectionBackColor = Color.White;
            outputTextBox.SelectionColor = Color.Black;

            outputTextBox.Select(beginLineIndex, outputTextBox.Lines[currentLine].Length + 1);
            outputTextBox.SelectionBackColor = Color.DarkBlue; //SystemColors.Highlight; // HighLight colors seem not to be working.
            outputTextBox.SelectionColor = Color.White; //SystemColors.HighlightText;
            
            string text = outputTextBox.SelectedText;

            outputTextBox.Select(beginLineIndex, 0);


            int bracketStart = text.IndexOf("(");
            int bracketEnd = text.IndexOf(")");

            if (bracketStart == -1 || bracketEnd == -1) {
                outputTextBox.Select(0, outputTextBox.TextLength);       
                outputTextBox.SelectionBackColor = Color.White;
                outputTextBox.SelectionColor = Color.Black;
                outputTextBox.Select(beginLineIndex, 0);
                outputTextBox.ResumeLayout();

                return;
            }

            string fileName = text.Substring(0, bracketStart);

            int lineNumber = Int32.Parse(text.Substring(bracketStart + 1, bracketEnd - bracketStart - 1));

            string message = text.Substring(bracketEnd+1);

            if (File.Exists(fileName) == false) {
                outputTextBox.Select(0, outputTextBox.TextLength);       
                outputTextBox.SelectionBackColor = Color.White;
                outputTextBox.SelectionColor = Color.Black;
                outputTextBox.Select(beginLineIndex, 0);
                outputTextBox.ResumeLayout();

                return;
            }

            outputTextBox.ResumeLayout();

            int anchorCount = 0;
            using (StreamReader sr = new StreamReader(fileName)) {
                XmlTextReader reader = new XmlTextReader(sr);
                int reads = 0;
                int readElement = 0;
                int readText = 0;
                int readEndElement = 0;
                // Parse the XML and display each node.
                while (reader.Read()){
                   reads++;
                   switch (reader.NodeType){
                     case XmlNodeType.Element:
                        readElement++;
                        break;
                     case XmlNodeType.Text:
                        readText++;
                        break;
                     case XmlNodeType.EndElement:
                        readEndElement++;
                        break;
                   }
                    if (reader.LineNumber == lineNumber) {
                        anchorCount = readElement;
                        break;
                    }
                }   
            }

            LaunchFile(fileName, anchorCount ,lineNumber, message.Trim());
        }

        protected void LaunchFile(string filename, int anchorNumber, int lineNumber, string message) {
            XslTransform transform = new XslTransform();
            using (Stream strm = WixFiles.GetResourceStream("viewWixXml.xsl")) {
                 XmlTextReader xr = new XmlTextReader(strm);
                transform.Load(xr, null, null);
            }

            string outputFile = Path.Combine(Path.GetTempPath(), Path.GetFileName(filename)) + ".html";
            if (
                ( File.Exists(outputFile) &&
                ( File.GetLastWriteTimeUtc(outputFile).CompareTo(File.GetLastWriteTimeUtc(filename)) > 0 ) &&
                ( File.GetLastWriteTimeUtc(outputFile).CompareTo(File.GetLastWriteTimeUtc(Assembly.GetExecutingAssembly().Location)) > 0 ) ) == false
               ) {
                File.Delete(outputFile);
                transform.Transform(filename, outputFile, null);
            }


            if (xmlDisplayForm.Visible == false) {
                xmlDisplayForm = new XmlDisplayForm();
            }

            xmlDisplayForm.Text = String.Format("{0}({1}) {2}", Path.GetFileName(filename), lineNumber, message);
            xmlDisplayForm.ShowFile(String.Format("{0}#a{1}", outputFile, anchorNumber));
            xmlDisplayForm.Show();
        }

        protected void HasFocus(Object sender, EventArgs e) {
            if (closeButton.Focused ||
                outputLabel.Focused ||
                outputTextBox.Focused || outputTextBox.Capture) {
                outputLabel.BackColor = Color.DimGray;
                outputLabel.ForeColor = Color.White;
            } else {
                outputLabel.BackColor = Color.Gray;
                outputLabel.ForeColor = Color.LightGray;
            }
        }

        public int Run(ProcessStartInfo[] processStartInfos) {
            outputTextBox.Rtf = "";

            DateTime start = DateTime.Now;

            foreach (ProcessStartInfo processStartInfo in processStartInfos) {
                DateTime subStart = DateTime.Now;
                OutputStart(processStartInfo, subStart);
                
                activeProcess = Process.Start(processStartInfo);
                
                ReadStandardOut();
    
                activeProcess.WaitForExit();

                if (activeProcess.ExitCode != 0) {
                    break;
                }

                OutputDone(activeProcess, subStart);
            }

            Output("", true);
            Output("----- Finished", true);
            Output("", false);

            if (activeProcess.ExitCode != 0) {
                Output("Error in " + Path.GetFileNameWithoutExtension(activeProcess.StartInfo.FileName), true);
            } else {
                Output(String.Format("Finished in: {0} seconds", activeProcess.ExitTime.Subtract(start).Seconds.ToString()), true);
            }

            return activeProcess.ExitCode;
        }
        
        public int Run(ProcessStartInfo processStartInfo) {
            DateTime start = DateTime.Now;

            OutputStart(processStartInfo, start);
            
            activeProcess = Process.Start(processStartInfo);
            
            ReadStandardOut();

            activeProcess.WaitForExit();

            OutputDone(activeProcess, start);

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
                    output = String.Format("\\b {0}\\b0\\par\r\n", escaped);
                }
            }

            outputTextBox.Select(outputTextBox.Text.Length, 0);
            outputTextBox.SelectedRtf = String.Format(@"{{\rtf1\ansi\ansicpg1252\deff0\deflang1033{{\fonttbl{{\f0\fmodern\fprq1\fcharset0 Courier New;}}}}" +
                                            @"\viewkind4\uc1\pard\f0\fs16 {0}}}", output);

            outputTextBox.Select(outputTextBox.Text.Length, 0);
            outputTextBox.Focus();
            outputTextBox.ScrollToCaret();
        }

        private void OutputStart(ProcessStartInfo processStartInfo, DateTime start) {
            Output(String.Format("----- Starting {0} {1} at {2}", processStartInfo.FileName, processStartInfo.Arguments, start), true);
            Output("", true);
        }

        private void OutputDone(Process process, DateTime start) {
            Output("", true);
            Output(String.Format("Done in: {0} ms", process.ExitTime.Subtract(start).Milliseconds), true);
            Output("", true);
        }

        public void Clear() {
            outputTextBox.Text = "";
        }
    }
}
