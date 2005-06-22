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
    public class OutputPanel : BasePanel{
        protected OutputTextbox outputBox;
        protected Process activeProcess;
        Button closeButton;
        Label outputLabel;

        private System.Windows.Forms.Timer doubleClickTimer = new System.Windows.Forms.Timer();
        private bool isFirstClick = true;
        private int milliseconds = 0;

        public OutputPanel(WixFiles wixFiles) : base(wixFiles) {
            InitializeComponent();
        }

        public RichTextBox RichTextBox {
            get { return outputBox; }
        }

        public event EventHandler CloseClicked;

        private void InitializeComponent() {
            this.TabStop = true;
            int buttonWidth = 11;
            int buttonHeigth = 11;
            int paddingX = 2;
            int paddingY = 2;

            closeButton = new Button();
            outputBox = new OutputTextbox();
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


            outputBox.Dock = DockStyle.Bottom;
            outputBox.Location = new Point(0, buttonHeigth + 3*paddingY);
            outputBox.Size = new Size(200, this.ClientSize.Height - outputBox.Location.Y);
            outputBox.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            outputBox.ScrollBars = RichTextBoxScrollBars.Both;
            outputBox.WordWrap = false;
            outputBox.AllowDrop = false;

            this.Controls.Add(closeButton);
            this.Controls.Add(outputLabel);
            this.Controls.Add(outputBox);

            closeButton.TabStop = true;
            outputLabel.TabStop = true;
            outputBox.TabStop = true;

            closeButton.LostFocus += new EventHandler(HasFocus);
            outputLabel.LostFocus += new EventHandler(HasFocus);
            outputBox.LostFocus += new EventHandler(HasFocus);

            closeButton.GotFocus += new EventHandler(HasFocus);
            outputLabel.GotFocus += new EventHandler(HasFocus);
            outputBox.GotFocus += new EventHandler(HasFocus);

            closeButton.Enter += new EventHandler(HasFocus);
            outputLabel.Enter += new EventHandler(HasFocus);
            outputBox.Enter += new EventHandler(HasFocus);

            closeButton.Click += new EventHandler(HasFocus);
            outputLabel.Click += new EventHandler(HasFocus);

            outputBox.MouseUp += new MouseEventHandler(outputBox_MouseDown);


            doubleClickTimer.Interval = 100;
            doubleClickTimer.Tick += new EventHandler(doubleClickTimer_Tick);
        }

        protected void OnCloseClick(Object sender, EventArgs e) {
            if (CloseClicked != null) {
                CloseClicked(sender, e);
            }
        }


        private void outputBox_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e) {
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
            int currentIndex = outputBox.GetCharIndexFromPosition(new Point(x, y));
            int currentLine = outputBox.GetLineFromCharIndex(currentIndex);

            int lineCount = outputBox.Lines.Length;

            int beginLineIndex = currentIndex;
            if (currentLine == 0) {
                beginLineIndex = 0;
            } else {
                while (currentLine == outputBox.GetLineFromCharIndex(beginLineIndex - 1) && 
                       currentLine != 0) {
                    beginLineIndex--;
                }
            }

            outputBox.Select(0, outputBox.TextLength);
            outputBox.SelectionBackColor = Color.White;
            outputBox.SelectionColor = Color.Black;
            outputBox.HideSelection = true;

            outputBox.Select(beginLineIndex, outputBox.Lines[currentLine].Length + 1);
            outputBox.SelectionBackColor = SystemColors.Highlight;
            outputBox.SelectionColor = SystemColors.HighlightText;

            string text = outputBox.SelectedText;

            outputBox.Select(beginLineIndex, 0);
            outputBox.HideSelection = true;
            

            int bracketStart = text.IndexOf("(");
            int bracketEnd = text.IndexOf(")");

            if (bracketStart == -1 || bracketEnd == -1) {
                return;
            }

            string fileName = text.Substring(0, bracketStart);

            int lineNumber = Int32.Parse(text.Substring(bracketStart + 1, bracketEnd - bracketStart - 1));

            string message = text.Substring(bracketEnd+1);

            if (File.Exists(fileName) == false) {
                return;
            }

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
            using (Stream strm = WixFiles.GetResourceStream("WixEdit.viewWixXml.xsl")) {
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


            XmlDisplayForm form = new XmlDisplayForm(String.Format("{0}#a{1}", outputFile, anchorNumber));
            form.Text = String.Format("{0}({1}) {2}", Path.GetFileName(filename), lineNumber, message);
            form.Show();
        }

        protected void HasFocus(Object sender, EventArgs e) {
            if (closeButton.Focused ||
                outputLabel.Focused ||
                outputBox.Focused || outputBox.Capture) {
                outputLabel.BackColor = Color.DimGray;
                outputLabel.ForeColor = Color.White;
            } else {
                outputLabel.BackColor = Color.Gray;
                outputLabel.ForeColor = Color.LightGray;
            }
        }

        public int Run(ProcessStartInfo[] processStartInfos) {
            outputBox.Rtf = "";

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

            if (activeProcess.ExitCode != 0) {
                Output("", false);
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

            outputBox.Select(outputBox.Text.Length, 0);
            outputBox.SelectedRtf = String.Format(@"{{\rtf1\ansi\ansicpg1252\deff0\deflang1033{{\fonttbl{{\f0\fmodern\fprq1\fcharset0 Courier New;}}}}" +
                                            @"\viewkind4\uc1\pard\f0\fs16 {0}}}", output);

            outputBox.Select(outputBox.Text.Length, 0);
            outputBox.Focus();
            outputBox.ScrollToCaret();
        }

        private void OutputStart(ProcessStartInfo processStartInfo, DateTime start) {
            Output(String.Format("Starting {0} {1} at {2}", processStartInfo.FileName, processStartInfo.Arguments, start), true);
            Output("", true);
        }

        private void OutputDone(Process process, DateTime start) {
            Output(String.Format("Done in: {0} ms", process.ExitTime.Subtract(start).Milliseconds), true);
            Output("", true);
        }

        public void Clear() {
            outputBox.Text = "";
        }
    }
}
