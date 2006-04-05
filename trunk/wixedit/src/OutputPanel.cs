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

using WixEdit.Controls;

namespace WixEdit {
    /// <summary>
    /// Summary description for OutputPanel.
    /// </summary>
    public class OutputPanel : Panel {
        protected OutputTextbox outputTextBox;
        protected Process activeProcess;

        private System.Windows.Forms.Timer doubleClickTimer = new System.Windows.Forms.Timer();
        private bool isFirstClick = true;
        private int milliseconds = 0;
        
        private int currentSelectionStart = 0;
        private int currentSelectionLength = 0;

        XmlDisplayForm xmlDisplayForm = new XmlDisplayForm();

        public OutputPanel() {
            InitializeComponent();
        }

        public RichTextBox RichTextBox {
            get { return outputTextBox; }
        }

        private void InitializeComponent() {
            TabStop = true;

            outputTextBox = new OutputTextbox();

            outputTextBox.Dock = DockStyle.Fill;
            outputTextBox.ScrollBars = RichTextBoxScrollBars.Both;
            outputTextBox.WordWrap = false;
            outputTextBox.AllowDrop = false;

            Controls.Add(outputTextBox);

            outputTextBox.TabStop = true;
            outputTextBox.HideSelection = false;

            outputTextBox.MouseUp += new MouseEventHandler(outputTextBox_MouseDown);

            doubleClickTimer.Interval = 100;
            doubleClickTimer.Tick += new EventHandler(doubleClickTimer_Tick);
        }

        private void outputTextBox_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e) {
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
            int start = outputTextBox.SelectionStart;
            int length = outputTextBox.SelectionLength;

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
            
            if (currentSelectionStart + currentSelectionLength > 0) {
                outputTextBox.Select(currentSelectionStart, currentSelectionLength);
                outputTextBox.SelectionBackColor = Color.White;
                outputTextBox.SelectionColor = Color.Black;
            }

            currentSelectionStart = beginLineIndex;
            currentSelectionLength = outputTextBox.Lines[currentLine].Length + 1;
            
            string text = outputTextBox.Lines[currentLine];

            int bracketStart = text.IndexOf("(");
            int bracketEnd = text.IndexOf(")");

            if (bracketStart == -1 || bracketEnd == -1) {
                // outputTextBox.Select(start, length);
                // outputTextBox.Select(beginLineIndex, 0);
                outputTextBox.ResumeLayout();

                currentSelectionStart = 0; 
                currentSelectionLength = 0;

                return;
            }

            string fileName = text.Substring(0, bracketStart);

            int lineNumber = 0;
            try {
                lineNumber = Int32.Parse(text.Substring(bracketStart + 1, bracketEnd - bracketStart - 1));
            } catch (Exception) {
                outputTextBox.ResumeLayout();

                currentSelectionStart = 0; 
                currentSelectionLength = 0;

                return;
            }

            string message = text.Substring(bracketEnd+1);

            if (File.Exists(fileName) == false) {
                // outputTextBox.Select(start, length);
                // outputTextBox.Select(beginLineIndex, 0);
                outputTextBox.ResumeLayout();

                currentSelectionStart = 0; 
                currentSelectionLength = 0;

                return;
            }

            outputTextBox.Select(currentSelectionStart, currentSelectionLength);
            outputTextBox.SelectionBackColor = Color.DarkBlue; //SystemColors.Highlight; // HighLight colors seem not to be working.
            outputTextBox.SelectionColor = Color.White; //SystemColors.HighlightText;
            outputTextBox.Select(beginLineIndex, 0);

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

        public int RunWithLogFile(ProcessStartInfo processStartInfo, string logFile) {
            DateTime start = DateTime.Now;

            OutputStart(processStartInfo, start);
            
            activeProcess = Process.Start(processStartInfo);
            
            while(activeProcess.WaitForExit(100) == false) {
                if (File.Exists(logFile)) {
                    ReadLogFile(logFile);
                    break;
                }
                Application.DoEvents();
            }

            OutputDone(activeProcess, start);

            return activeProcess.ExitCode;
        }

        private void ReadLogFile(string logFile) {
            FileInfo log = new FileInfo(logFile);

            if (log.Exists) {
                using(FileStream fs = log.Open(FileMode.Open, FileAccess.Read, FileShare.ReadWrite)) {
                    StreamReader sr = new StreamReader(fs);
/*
                    string line = sr.ReadLine();
                    while (line != null) {
                        Output(line, false);
                        
                        line = sr.ReadLine();
                    }
*/
                    string line = null;
                    while (sr.Peek() >= 0 || activeProcess.WaitForExit(200) == false) {
                        if (sr.Peek() >= 0) {
                            try {
                                line = sr.ReadLine();
                            } catch {}
    
                            if (line != null) {
                                Output(line, false);
                            }
                        }

                        Application.DoEvents();
                    }

                    sr.Close();
                }
            }
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
