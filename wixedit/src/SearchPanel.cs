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
    /// Summary description for SearchPanel.
    /// </summary>
    public class SearchPanel : Panel {
        protected OutputTextbox outputTextBox;

        private System.Windows.Forms.Timer doubleClickTimer = new System.Windows.Forms.Timer();
        private bool isFirstClick = true;
        private int milliseconds = 0;

        private int currentSelectionStart = 0;
        private int currentSelectionLength = 0;

        XmlNodeList lastNodes;

        EditorForm editorForm;

        XmlDisplayForm xmlDisplayForm = new XmlDisplayForm();

        public SearchPanel(EditorForm editorForm) {
            this.editorForm = editorForm;

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

            outputTextBox.MouseUp += new MouseEventHandler(outputTextBox_MouseDown);

            doubleClickTimer.Interval = 100;
            doubleClickTimer.Tick += new EventHandler(doubleClickTimer_Tick);
        }

        public void Search(WixFiles wixFiles, string search) {
            Clear();

            string searchAttrib = String.Format("//@*[contains(translate(.,'ABCDEFGHIJKLMNOPQRSTUVWXYZ','abcdefghijklmnopqrstuvwxyz'),'{0}')]", search.ToLower());
            string searchElement = String.Format("//*[contains(translate(text(),'ABCDEFGHIJKLMNOPQRSTUVWXYZ','abcdefghijklmnopqrstuvwxyz'),'{0}')]", search.ToLower());
            lastNodes = wixFiles.WxsDocument.SelectNodes(searchAttrib + "|" + searchElement);
            foreach (XmlNode node in lastNodes) {
                OutputResult(search, node, GetFirstElement(node));
            }

            Output("--------------------------------", false);
            Output(String.Format("Found \"{0}\" {1} {2}", search, lastNodes.Count, (lastNodes.Count == 1) ? "time" : "times"), true);
        }

        private XmlElement GetFirstElement(XmlNode node) {
            XmlNode showableNode = node;
            while (showableNode.NodeType != XmlNodeType.Element) {
                if (showableNode.NodeType == XmlNodeType.Attribute) {
                    showableNode = ((XmlAttribute) showableNode).OwnerElement;
                } else {
                    showableNode = showableNode.ParentNode;
                }
            }

            return (XmlElement)showableNode;
        }

        private void OutputResult(string search, XmlNode node, XmlElement element) {
            bool isAttribute = (node != element);

            string strValue = null;
            if (isAttribute) {
                strValue = node.Value;
            } else {
                strValue = node.InnerText;
            }

            int startPos = strValue.ToLower().IndexOf(search.ToLower());

            string firstPart = strValue.Substring(0, startPos).Replace("\\", "\\\\");
            string secondPart = strValue.Substring(startPos, search.Length).Replace("\\", "\\\\");
            string thirdPart = strValue.Substring(startPos + search.Length).Replace("\\", "\\\\");

            strValue = String.Format("{0}\\b {1}\\b0 {2}", firstPart, secondPart, thirdPart);

            if (isAttribute) {
                OutputRaw(String.Format("{0}/@{1} = '{2}'\\par\r\n", element.Name, node.Name, strValue));
            } else {
                if (element.HasAttribute("Id")) {
                    OutputRaw(String.Format("{0}[@Id='{1}'] = '{2}'\\par\r\n", element.Name, element.Attributes["Id"].Value, strValue));
                } else {
                    OutputRaw(String.Format("{0} = '{1}'\\par\r\n", element.Name, strValue));
                }
            }
//            OutputRaw(String.Format("{0}/@{1} = '{2}'\\par\r\n", element.Name, node.Name, node.Value));
//            Output(String.Format("{0} - {1}: {2}", element.Name, node.Name, node.Value), false);
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

            int oldSelectionStart = outputTextBox.SelectionStart;
            int oldSelectionLength = outputTextBox.SelectionLength;
            if (currentSelectionStart + currentSelectionLength > 0) {
                outputTextBox.Select(currentSelectionStart, currentSelectionLength);
                outputTextBox.SelectionBackColor = Color.White;
                outputTextBox.SelectionColor = Color.Black;

                currentSelectionStart = 0;
                currentSelectionLength = 0;
            }

            if (currentLine < lastNodes.Count) {
                currentSelectionStart = beginLineIndex;
                currentSelectionLength = outputTextBox.Lines[currentLine].Length + 1;
    
                outputTextBox.Select(beginLineIndex, outputTextBox.Lines[currentLine].Length + 1);
                outputTextBox.SelectionBackColor = Color.DarkBlue; //SystemColors.Highlight; // HighLight colors seem not to be working.
                outputTextBox.SelectionColor = Color.White; //SystemColors.HighlightText;
                
                outputTextBox.Select(beginLineIndex, 0);    

                editorForm.ShowNode(lastNodes[currentLine]);
            } else {
                outputTextBox.Select(oldSelectionStart, oldSelectionLength);
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

            OutputRaw(output);
        }

        private void OutputRaw(string output) {
            outputTextBox.Select(outputTextBox.Text.Length, 0);
            outputTextBox.SelectedRtf = String.Format(@"{{\rtf1\ansi\ansicpg1252\deff0\deflang1033{{\fonttbl{{\f0\fmodern\fprq1\fcharset0 Courier New;}}}}" +
                                            @"\viewkind4\uc1\pard\f0\fs16 {0}}}", output);

            outputTextBox.Select(outputTextBox.Text.Length, 0);
            outputTextBox.Focus();
            outputTextBox.ScrollToCaret();
        }

        public void Clear() {
            outputTextBox.Text = "";
            lastNodes = null;
        }
    }
}
