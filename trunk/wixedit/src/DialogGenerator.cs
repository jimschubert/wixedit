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
using System.Xml;
using System.IO;
using System.Resources;
using System.Reflection;

namespace WixEdit {
    public class DialogGenerator {
        private Hashtable _definedFonts;
        private WixFiles _wixFiles;
        private Image _bgImage;

        public DialogGenerator(WixFiles wixFiles) {
            _definedFonts = new Hashtable();
            _wixFiles = wixFiles;

            ReadFonts();
        }

        private void ReadFonts() {
            XmlNodeList fontElements = _wixFiles.WxsDocument.SelectNodes("//wix:UI/wix:TextStyle", _wixFiles.WxsNsmgr);
            foreach (XmlNode fontElement in fontElements) {

                FontStyle style = FontStyle.Regular;
                if (fontElement.Attributes["Bold"] != null && fontElement.Attributes["Bold"].Value.ToLower() == "yes") {
                    style = style | FontStyle.Bold;
                }
                if (fontElement.Attributes["Italic"] != null && fontElement.Attributes["Italic"].Value.ToLower() == "yes") {
                    style = style | FontStyle.Italic;
                }
                if (fontElement.Attributes["Strike"] != null && fontElement.Attributes["Strike"].Value.ToLower() == "yes") {
                    style = style | FontStyle.Strikeout;
                }
                if (fontElement.Attributes["Underline"] != null && fontElement.Attributes["Underline"].Value.ToLower() == "yes") {
                    style = style | FontStyle.Underline;
                }

                Font font = new Font(
                        fontElement.Attributes["FaceName"].Value,
                        XmlConvert.ToInt32(fontElement.Attributes["Size"].Value),
                        style,
                        GraphicsUnit.Point
                    );

                _definedFonts.Add(fontElement.Attributes["Id"].Value, font);
            }

//
//Name Type Description Usage 
//Id xs:string   required 
//FaceName xs:string   required 
//
//Blue xs:integer 0 to 255   
//Green xs:integer 0 to 255  
//Red xs:integer 0 to 255   
//
//Size xs:integer   required 
//
//Bold YesNoType     
//Italic YesNoType     
//Strike YesNoType     
//Underline YesNoType   


            //
        }

        int _parentHwnd;
        public Form GenerateDialog(XmlNode dialog, Control parent) {
            Form newDialog = new Form();

            _parentHwnd = (int)parent.Handle;

            newDialog.Font = new Font("Tahoma", 8.00F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
            newDialog.ShowInTaskbar = false;
            newDialog.TopLevel = true;
            // newDialog.TopMost = true;
            // newDialog.Opacity = 0.75;

            newDialog.Icon = new Icon(WixFiles.GetResourceStream("WixEdit.msi.ico"));
            
            newDialog.StartPosition = FormStartPosition.Manual;

            newDialog.MinimizeBox = false;
            newDialog.MaximizeBox = false;
            newDialog.FormBorderStyle = FormBorderStyle.FixedDialog;

            // newDialog.Width = dialogUnitToPixelsWidth(XmlConvert.ToInt32(dialog.Attributes["Width"].Value));
            // newDialog.Height = dialogUnitToPixelsHeight(XmlConvert.ToInt32(dialog.Attributes["Height"].Value));
            newDialog.ClientSize = new Size(dialogUnitToPixelsWidth(XmlConvert.ToInt32(dialog.Attributes["Width"].Value)), dialogUnitToPixelsHeight(XmlConvert.ToInt32(dialog.Attributes["Height"].Value)));

            // Background Images should be added first, these controls should be used as parent 
            // to get correct transparancy. For now only 1 bitmap is supported per Dialog.
            // - Is this the correct way to handle the transparancy?
            // - How does MSI handle transparant labels when having 2 bitmaps as background?

            XmlNodeList buttons = dialog.SelectNodes("wix:Control[@Type='PushButton']", _wixFiles.WxsNsmgr);
            AddButtons(newDialog, buttons);

            XmlNodeList edits = dialog.SelectNodes("wix:Control[@Type='Edit']", _wixFiles.WxsNsmgr);
            AddEditBoxes(newDialog, edits);

            XmlNodeList pathEdits = dialog.SelectNodes("wix:Control[@Type='PathEdit']", _wixFiles.WxsNsmgr);
            AddPathEditBoxes(newDialog, pathEdits);

            XmlNodeList lines = dialog.SelectNodes("wix:Control[@Type='Line']", _wixFiles.WxsNsmgr);
            AddLines(newDialog, lines);

            XmlNodeList texts = dialog.SelectNodes("wix:Control[@Type='Text']", _wixFiles.WxsNsmgr);
            AddTexts(newDialog, texts);

            XmlNodeList bitmaps = dialog.SelectNodes("wix:Control[@Type='Bitmap']", _wixFiles.WxsNsmgr);
            AddBackgroundBitmaps(newDialog, bitmaps);

            if (dialog.Attributes["Title"] != null) {
                newDialog.Text = ExpandWixProperties(dialog.Attributes["Title"].Value);
            }

            if (dialog.Attributes["NoMinimize"] == null) {
                newDialog.MinimizeBox = true;
            } else {
                newDialog.MinimizeBox = (dialog.Attributes["NoMinimize"].Value.ToLower() != "yes");
            }

            return newDialog;
        }

        /// <summary>
        /// The function returns the dialog base units. The low-order word of the return value 
        /// contains the horizontal dialog box base unit, and the high-order word contains the 
        /// vertical dialog box base unit. 
        /// 
        /// One horizontal dialog unit is equal to one-fourth of the average character width for the current system font.
        /// One vertical dialog unit is equal to one-eighth of an average character height for the current system font.
        /// </summary>
        /// <param name="hwnd"></param>
        /// <returns></returns>
        [DllImport("user32")]
        public static extern int GetDialogBaseUnits(int hwnd);

        private int dialogUnitToPixelsWidth(int dlus) {
            long  DLUs = GetDialogBaseUnits(_parentHwnd);
            int HorDLUs = (int) DLUs & 0x0000FFFF;
            
            return (int)Math.Round(((double)dlus*HorDLUs) / 6);
            //return (int)Math.Round(((double)dlus)*10.63/8);
        }
        private int dialogUnitToPixelsHeight(int dlus) {
            long  DLUs = GetDialogBaseUnits(_parentHwnd);
            int VerDLUs = (int) (DLUs >> 16) & 0xFFFF;

            return (int)Math.Round(((double)dlus*VerDLUs) / 12);
            //return (int)Math.Round(((double)dlus)*10.66/8);
        }

        private string ExpandWixProperties(string value) {
            int posStart = value.IndexOf("[", 0);
            int posEnd = 0;
            while (posStart > -1) {
                posEnd = value.IndexOf("]", posStart);

                string propName = value.Substring(posStart+1, posEnd-posStart-1);
                
                XmlNode propertyNode = _wixFiles.WxsDocument.SelectSingleNode(String.Format("//wix:Property[@Id='{0}']", propName), _wixFiles.WxsNsmgr);
                if (propertyNode != null) {
                    value = value.Replace(String.Format("[{0}]", propName), propertyNode.InnerText);
                } else {
                    value = value.Replace(String.Format("[{0}]", propName), getSpecialWixProperty(propName));
                }

                posStart = value.IndexOf("[", posStart);               
            }

            return value;
        }

        private string getSpecialWixProperty(string propname) {
            switch (propname) {
                case "ProductName":
                    return GetProductName();
                default:
                    return String.Empty;
            }
        }

        private string GetProductName() {
            string returnValue = String.Empty;

            XmlNode productyNode = _wixFiles.WxsDocument.SelectSingleNode("/wix:Wix/wix:Product", _wixFiles.WxsNsmgr);
            XmlAttribute nameAttribute = productyNode.Attributes["Name"];
            if (nameAttribute != null) {
                returnValue = nameAttribute.Value;
            }

            return returnValue;
        }

        private void AddButtons(Form newDialog, XmlNodeList buttons) {
            foreach (XmlNode button in buttons) {
                Button newButton = new Button();
                SetControlSizes(newButton, button);

//              <Control Id="Up" Type="PushButton" X="298" Y="55" Width="19" Height="19" 
//               Icon="yes" FixedSize="yes" IconSize="16" Text="Up">

                if (button.Attributes["Icon"] != null &&
                    button.Attributes["Icon"].Value.ToLower() == "yes") {
                    string binaryId = GetTextFromXmlElement(button);
                    try {
                        Stream imageStream = GetBinaryStream(binaryId);
                        newButton.Image = new Bitmap(imageStream);
                    } catch {
                        SetText(newButton, button);
                    }
                } else {
                    newButton.FlatStyle = FlatStyle.System;
                    SetText(newButton, button);
                }
                
                newDialog.Controls.Add(newButton);
            }
        }

        private void AddEditBoxes(Form newDialog, XmlNodeList editboxes) {
            foreach (XmlNode edit in editboxes) {
                TextBox newEdit = new TextBox();
                SetControlSizes(newEdit, edit);
                SetText(newEdit, edit);

                newEdit.BorderStyle = BorderStyle.Fixed3D;

                newDialog.Controls.Add(newEdit);
            }
        }

        private void AddPathEditBoxes(Form newDialog, XmlNodeList patheditboxes) {
            foreach (XmlNode pathEdit in patheditboxes) {
                TextBox newPathEdit = new TextBox();
                SetControlSizes(newPathEdit, pathEdit);
                SetText(newPathEdit, pathEdit);

                newDialog.Controls.Add(newPathEdit);
            }
        }

        private void AddLines(Form newDialog, XmlNodeList lines) {
            foreach (XmlNode line in lines) {
                Label label = new Label();
                SetControlSizes(label, line);

                label.Height = 2;
                label.BorderStyle = BorderStyle.Fixed3D;

                newDialog.Controls.Add(label);
            }
        }

        private void AddTexts(Form newDialog, XmlNodeList texts) {
            foreach (XmlNode text in texts) {
                Label label = new Label();
                SetControlSizes(label, text);
                label.ClientSize = new Size(dialogUnitToPixelsWidth(XmlConvert.ToInt32(text.Attributes["Width"].Value)),
                                            dialogUnitToPixelsHeight(XmlConvert.ToInt32(text.Attributes["Height"].Value)));
                SetText(label, text);

                label.BackColor = Color.Transparent;

                newDialog.Controls.Add(label);
            }
        }
        
        private void AddBackgroundBitmaps(Form newDialog, XmlNodeList bitmaps) {
            foreach (XmlNode bitmap in bitmaps) {
                if (_bgImage == null) {
                    _bgImage = new Bitmap(newDialog.Width, newDialog.Height);
                    newDialog.BackgroundImage = _bgImage;
                }

                Graphics graphic = Graphics.FromImage(_bgImage);
                graphic.Clear(SystemColors.Control);

                string binaryId = GetTextFromXmlElement(bitmap);
                try {
                    using (Stream imageStream = GetBinaryStream(binaryId)) {
                        graphic.DrawImage(new Bitmap(imageStream),
                            dialogUnitToPixelsWidth(XmlConvert.ToInt32(bitmap.Attributes["X"].Value)),
                            dialogUnitToPixelsHeight(XmlConvert.ToInt32(bitmap.Attributes["Y"].Value)),
                            dialogUnitToPixelsWidth(XmlConvert.ToInt32(bitmap.Attributes["Width"].Value)),
                            dialogUnitToPixelsHeight(XmlConvert.ToInt32(bitmap.Attributes["Height"].Value)));
                    }
                } catch {
                    // Oke, just make the area nice and picture like :)
                    Brush brush = new HatchBrush(HatchStyle.OutlinedDiamond, Color.LightGray, Color.GhostWhite);
                    graphic.FillRectangle(brush,
                            dialogUnitToPixelsWidth(XmlConvert.ToInt32(bitmap.Attributes["X"].Value)),
                            dialogUnitToPixelsHeight(XmlConvert.ToInt32(bitmap.Attributes["Y"].Value)),
                            dialogUnitToPixelsWidth(XmlConvert.ToInt32(bitmap.Attributes["Width"].Value)),
                            dialogUnitToPixelsHeight(XmlConvert.ToInt32(bitmap.Attributes["Height"].Value)));
                }
            }
        }

        private void SetControlSizes(Control control, XmlNode controlElement) {
            control.Left = dialogUnitToPixelsWidth(XmlConvert.ToInt32(controlElement.Attributes["X"].Value));
            control.Top = dialogUnitToPixelsHeight(XmlConvert.ToInt32(controlElement.Attributes["Y"].Value));
            control.Width = dialogUnitToPixelsWidth(XmlConvert.ToInt32(controlElement.Attributes["Width"].Value));
            control.Height = dialogUnitToPixelsHeight(XmlConvert.ToInt32(controlElement.Attributes["Height"].Value));

            //control.ClientSize = new Size(dialogUnitToPixels(XmlConvert.ToInt32(controlElement.Attributes["Width"].Value)), dialogUnitToPixels(XmlConvert.ToInt32(button.Attributes["Height"].Value)));
        }

        private void SetText(Control textControl, XmlNode textElement) {
            string textValue = GetTextFromXmlElement(textElement);

            int startFont = textValue.IndexOf("{\\");
            if (startFont < 0) {
                startFont = textValue.IndexOf("{&");
            }
            if (startFont >= 0) {
                int endFont = textValue.IndexOf("}", startFont);

                Font font = _definedFonts[textValue.Substring(startFont+2, endFont-startFont-2)] as Font;
                if (font != null) {
                    textControl.Font = font;
                }

                textValue = textValue.Remove(startFont, endFont-startFont+1);
            }

            textControl.Text = textValue;
        }

        private string GetTextFromXmlElement(XmlNode textElement) {
            string textValue = String.Empty;

            if (textElement.Attributes["Text"] != null) {
                textValue = ExpandWixProperties(textElement.Attributes["Text"].Value);
            } else {
                XmlNode text = textElement.SelectSingleNode("wix:Text", _wixFiles.WxsNsmgr);
                if (text != null) {
                    textValue = ExpandWixProperties(text.InnerText);
                }
            }

            return textValue;
        }

        private Stream GetBinaryStream(string binaryId) {
            XmlNode binaryNode = _wixFiles.WxsDocument.SelectSingleNode(String.Format("//wix:Binary[@Id='{0}']", binaryId), _wixFiles.WxsNsmgr);
            if (binaryNode == null) {
                throw new Exception(String.Format("Binary with id \"{0}\" not found", binaryId));
            }
            
            XmlAttribute srcAttrib = binaryNode.Attributes["src"];
            if (srcAttrib == null) {
                throw new Exception(String.Format("src Attribute of binary with id \"{0}\" not found", binaryId));
            }

            string src = srcAttrib.Value;
            if (src == null || src.Length == 0) {
                throw new Exception(String.Format("src Attribute of binary with id \"{0}\" is invalid.", binaryId));
            }

            if (Path.IsPathRooted(src)) {
                if (File.Exists(src) == false) {
                    throw new FileNotFoundException(String.Format("File of binary with id \"{0}\" is not found.", binaryId), src);
                }

                return File.Open(src, FileMode.Open);
            } else {
                if (File.Exists(src)) {
                    return File.Open(src, FileMode.Open);
                } else {
                    FileInfo[] files = _wixFiles.WxsDirectory.GetFiles(src);
                    if (files.Length != 1) {
                        throw new FileNotFoundException(String.Format("File of binary with id \"{0}\" is not found.", binaryId), src);
                    }

                    return files[0].OpenRead();
                }
            }
        }
    }
}
