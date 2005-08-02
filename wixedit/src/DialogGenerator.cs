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

using WixEdit.Settings;

namespace WixEdit {
    public class DialogGenerator {
        private Hashtable definedFonts;
        private WixFiles wixFiles;
        private Control parent;

        static double scale;

        static DialogGenerator() {
            scale = WixEditSettings.Instance.Scale;
        }

        public DialogGenerator(WixFiles wixFiles, Control parent) {
            this.definedFonts = new Hashtable();
            this.wixFiles = wixFiles;
            this.parent = parent;

            ReadFonts();
        }

        private void ReadFonts() {
            XmlNodeList fontElements = wixFiles.WxsDocument.SelectNodes("//wix:UI/wix:TextStyle", wixFiles.WxsNsmgr);
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
                        (float)(scale*XmlConvert.ToDouble(fontElement.Attributes["Size"].Value)),
                        style,
                        GraphicsUnit.Point
                    );

                definedFonts.Add(fontElement.Attributes["Id"].Value, font);
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


        public static double Scale {
            get {
                return scale;
            }
            set {
                scale = value;
            }
        }

        int parentHwnd;
        public DesignerForm GenerateDialog(XmlNode dialog, Control parent) {
            DesignerForm newDialog = new DesignerForm();

            parentHwnd = (int)parent.Handle;

            newDialog.Font = new Font("Tahoma", (float)(scale*8.00F), FontStyle.Regular, GraphicsUnit.Point, ((System.Byte)(0)));
            newDialog.ShowInTaskbar = false;
            newDialog.TopLevel = true;
            // newDialog.TopMost = true;
            // newDialog.Opacity = 0.75;

            newDialog.Icon = new Icon(WixFiles.GetResourceStream("dialog.msi.ico"));
            
            newDialog.StartPosition = FormStartPosition.Manual;

            newDialog.MinimizeBox = false;
            newDialog.MaximizeBox = false;
            newDialog.FormBorderStyle = FormBorderStyle.FixedDialog;

            if (dialog.Attributes["Width"] == null ||
                dialog.Attributes["Width"].Value.Trim().Length == 0 ||
                dialog.Attributes["Height"] == null ||
                dialog.Attributes["Height"].Value.Trim().Length == 0) {
                return null;
            }

            newDialog.ClientSize = new Size(DialogUnitsToPixelsWidth(XmlConvert.ToInt32(dialog.Attributes["Width"].Value.Trim())), DialogUnitsToPixelsHeight(XmlConvert.ToInt32(dialog.Attributes["Height"].Value.Trim())));

            // Background Images should be added first, these controls should be used as parent 
            // to get correct transparancy. For now only 1 bitmap is supported per Dialog.
            // - Is this the correct way to handle the transparancy?
            // - How does MSI handle transparant labels when having 2 bitmaps as background?

            XmlNodeList buttons = dialog.SelectNodes("wix:Control[@Type='PushButton']", wixFiles.WxsNsmgr);
            AddButtons(newDialog, buttons);

            XmlNodeList edits = dialog.SelectNodes("wix:Control[@Type='Edit']", wixFiles.WxsNsmgr);
            AddEditBoxes(newDialog, edits);

            XmlNodeList pathEdits = dialog.SelectNodes("wix:Control[@Type='PathEdit']", wixFiles.WxsNsmgr);
            AddPathEditBoxes(newDialog, pathEdits);

            XmlNodeList lines = dialog.SelectNodes("wix:Control[@Type='Line']", wixFiles.WxsNsmgr);
            AddLines(newDialog, lines);

            XmlNodeList texts = dialog.SelectNodes("wix:Control[@Type='Text']", wixFiles.WxsNsmgr);
            AddTexts(newDialog, texts);

            XmlNodeList rtfTexts = dialog.SelectNodes("wix:Control[@Type='ScrollableText']", wixFiles.WxsNsmgr);
            AddRftTextBoxes(newDialog, rtfTexts);

            XmlNodeList groupBoxes = dialog.SelectNodes("wix:Control[@Type='GroupBox']", wixFiles.WxsNsmgr);
            AddGroupBoxes(newDialog, groupBoxes);

            XmlNodeList icons = dialog.SelectNodes("wix:Control[@Type='Icon']", wixFiles.WxsNsmgr);
            AddIcons(newDialog, icons);

            XmlNodeList listBoxes = dialog.SelectNodes("wix:Control[@Type='ListBox']", wixFiles.WxsNsmgr);
            AddListBoxes(newDialog, listBoxes);

            XmlNodeList progressBars = dialog.SelectNodes("wix:Control[@Type='ProgressBar']", wixFiles.WxsNsmgr);
            AddProgressBars(newDialog, progressBars);

            XmlNodeList radioButtonGroups = dialog.SelectNodes("wix:Control[@Type='RadioButtonGroup']", wixFiles.WxsNsmgr);
            AddRadioButtonGroups(newDialog, radioButtonGroups);

            XmlNodeList maskedEdits = dialog.SelectNodes("wix:Control[@Type='MaskedEdit']", wixFiles.WxsNsmgr);
            AddMaskedEdits(newDialog, maskedEdits);

            XmlNodeList volumeCostLists = dialog.SelectNodes("wix:Control[@Type='VolumeCostList']", wixFiles.WxsNsmgr);
            AddVolumeCostLists(newDialog, volumeCostLists);

// Skipping tooltips
/*
            XmlNodeList tooltips = dialog.SelectNodes("wix:Control[@Type='Tooltips']", wixFiles.WxsNsmgr);
            AddTooltips(newDialog, tooltips);
*/
            XmlNodeList directoryCombos = dialog.SelectNodes("wix:Control[@Type='DirectoryCombo']", wixFiles.WxsNsmgr);
            AddDirectoryCombos(newDialog, directoryCombos);

            XmlNodeList directoryLists = dialog.SelectNodes("wix:Control[@Type='DirectoryList']", wixFiles.WxsNsmgr);
            AddDirectoryLists(newDialog, directoryLists);

            XmlNodeList selectionTrees = dialog.SelectNodes("wix:Control[@Type='SelectionTree']", wixFiles.WxsNsmgr);
            AddSelectionTrees(newDialog, selectionTrees);


            XmlNodeList bitmaps = dialog.SelectNodes("wix:Control[@Type='Bitmap']", wixFiles.WxsNsmgr);
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


// http://msdn.microsoft.com/library/en-us/msi/setup/installer_units.asp
// Platform SDK: Windows Installer
// Installer Units

// A Windows Installer user interface unit is equal to one-twelfth (1/12) 
// the height of the 10-point MS Sans Serif font size.

        public static int DialogUnitsToPixelsWidth(int dlus) {
            long  DLUs = GetDialogBaseUnits(0);
            int HorDLUs = (int) DLUs & 0x0000FFFF;
            
            return (int) Math.Round(((double)scale*dlus*HorDLUs) / 6);
        }

        public static int DialogUnitsToPixelsHeight(int dlus) {
            long  DLUs = GetDialogBaseUnits(0);
            int VerDLUs = (int) (DLUs >> 16) & 0xFFFF;

            return (int) Math.Round(((double)scale*dlus*VerDLUs) / 12);
        }

        public static int PixelsToDialogUnitsWidth(int pix) {
            long  DLUs = GetDialogBaseUnits(0);
            int HorDLUs = (int) DLUs & 0x0000FFFF;
            
            return (int) Math.Round(((double)pix*6)/(scale*HorDLUs));
        }

        public static int PixelsToDialogUnitsHeight(int pix) {
            long  DLUs = GetDialogBaseUnits(0);
            int VerDLUs = (int) (DLUs >> 16) & 0xFFFF;

            return (int) Math.Round(((double)pix*12)/(scale*VerDLUs));
        }

        private string ExpandWixProperties(string value) {
            int posStart = value.IndexOf("[", 0);
            int posEnd = 0;
            while (posStart > -1) {
                posEnd = value.IndexOf("]", posStart);

                string propName = value.Substring(posStart+1, posEnd-posStart-1);
                
                XmlNode propertyNode = wixFiles.WxsDocument.SelectSingleNode(String.Format("//wix:Property[@Id='{0}']", propName), wixFiles.WxsNsmgr);
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

            XmlNode productyNode = wixFiles.WxsDocument.SelectSingleNode("/wix:Wix/*", wixFiles.WxsNsmgr);
            XmlAttribute nameAttribute = productyNode.Attributes["Name"];
            if (nameAttribute != null) {
                returnValue = nameAttribute.Value;
            }

            return returnValue;
        }

        private void AddButtons(DesignerForm newDialog, XmlNodeList buttons) {
            foreach (XmlNode button in buttons) {
                Button newButton = new Button();
                SetControlSizes(newButton, button);

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
                
                newDialog.AddControl(button, newButton);
            }
        }

        private void AddEditBoxes(DesignerForm newDialog, XmlNodeList editboxes) {
            foreach (XmlNode edit in editboxes) {
                TextBox newEdit = new TextBox();
                SetControlSizes(newEdit, edit);
                SetText(newEdit, edit);

                newEdit.BorderStyle = BorderStyle.Fixed3D;

                newDialog.AddControl(edit, newEdit);
            }
        }

        private void AddPathEditBoxes(DesignerForm newDialog, XmlNodeList patheditboxes) {
            foreach (XmlNode pathEdit in patheditboxes) {
                TextBox newPathEdit = new TextBox();
                SetControlSizes(newPathEdit, pathEdit);
                SetText(newPathEdit, pathEdit);

                newDialog.AddControl(pathEdit, newPathEdit);
            }
        }

        private void AddLines(DesignerForm newDialog, XmlNodeList lines) {
            foreach (XmlNode line in lines) {
                Label label = new Label();
                SetControlSizes(label, line);

                label.Height = 2;
                label.BorderStyle = BorderStyle.Fixed3D;

                newDialog.AddControl(line, label);
            }
        }

        private void AddTexts(DesignerForm newDialog, XmlNodeList texts) {
            foreach (XmlNode text in texts) {
                Label label = new Label();
                SetControlSizes(label, text);
                SetText(label, text);

                label.BackColor = Color.Transparent;

                newDialog.AddControl(text, label);
            }
        }

        private void AddRftTextBoxes(DesignerForm newDialog, XmlNodeList rtfTexts) {
            foreach (XmlNode text in rtfTexts) {
                RichTextBox rtfCtrl = new RichTextBox();
                SetControlSizes(rtfCtrl, text);
                rtfCtrl.Rtf = GetTextFromXmlElement(text);

                newDialog.AddControl(text, rtfCtrl);
            }
        }

        private void AddGroupBoxes(DesignerForm newDialog, XmlNodeList groupBoxes) {
            foreach (XmlNode group in groupBoxes) {
                GroupBox groupCtrl = new GroupBox();

                // The FlatStyle.System makes the control look weird.
                // groupCtrl.FlatStyle = FlatStyle.System;

                SetControlSizes(groupCtrl, group);
                SetText(groupCtrl, group);

                newDialog.AddControl(group, groupCtrl);
            }
        }

        private void AddIcons(DesignerForm newDialog, XmlNodeList icons) {
            foreach (XmlNode icon in icons) {
                PictureBox picCtrl = new PictureBox();
                SetControlSizes(picCtrl, icon);

                picCtrl.SizeMode = PictureBoxSizeMode.StretchImage;

                string binaryId = GetTextFromXmlElement(icon);
                try {
                    Stream imageStream = GetBinaryStream(binaryId);
                    picCtrl.Image = new Bitmap(imageStream);
                } catch {
                }

                newDialog.AddControl(icon, picCtrl);
            }
        }

        private void AddListBoxes(DesignerForm newDialog, XmlNodeList listBoxes) {
            foreach (XmlNode list in listBoxes) {
                ListBox listCtrl = new ListBox();
                SetControlSizes(listCtrl, list);

                listCtrl.Items.Add(GetFromXmlElement(list, "Property"));

                newDialog.AddControl(list, listCtrl);
            }
        }

        private void AddProgressBars(DesignerForm newDialog, XmlNodeList progressBars) {
            foreach (XmlNode progressbar in progressBars) {
                ProgressBar progressCtrl = new ProgressBar();
                SetControlSizes(progressCtrl, progressbar);

                progressCtrl.Maximum = 100;
                progressCtrl.Value = 33;

                newDialog.AddControl(progressbar, progressCtrl);
            }
        }

        private void AddRadioButtonGroups(DesignerForm newDialog, XmlNodeList radioButtonGroups) {
            foreach (XmlNode radioButtonGroup in radioButtonGroups) {
                string radioGroupName = radioButtonGroup.Attributes["Property"].Value;
                string defaultValue = ExpandWixProperties(String.Format("[{0}]", radioGroupName));

                XmlNode radioGroup = wixFiles.WxsDocument.SelectSingleNode(String.Format("//wix:RadioGroup[@Property='{0}']", radioGroupName), wixFiles.WxsNsmgr);
                if (radioGroup == null) {
                    radioGroup = wixFiles.WxsDocument.SelectSingleNode(String.Format("//wix:RadioButtonGroup[@Property='{0}']", radioGroupName), wixFiles.WxsNsmgr);
                }

                Panel panel = new Panel();
                SetControlSizes(panel, radioButtonGroup);

                foreach (XmlNode radioElement in radioGroup.ChildNodes) {
                    RadioButton radioCtrl = new RadioButton();
                    SetText(radioCtrl, radioElement);
                    SetTag(radioCtrl, radioElement);

                    SetControlSizes(radioCtrl, radioElement);
                    
                    panel.Controls.Add(radioCtrl);

                    if (((string)radioCtrl.Tag).ToLower() == defaultValue.ToLower()) {
                        radioCtrl.Checked = true;
                    }
                }

                newDialog.AddControl(radioButtonGroup, panel);
            }
        }

        private void AddMaskedEdits(DesignerForm newDialog, XmlNodeList maskedEdits) {
            foreach (XmlNode edit in maskedEdits) {
                TextBox newEdit = new TextBox();
                SetControlSizes(newEdit, edit);
                SetText(newEdit, edit);

                newEdit.BorderStyle = BorderStyle.Fixed3D;

                newDialog.AddControl(edit, newEdit);
            }
        }

        private void AddVolumeCostLists(DesignerForm newDialog, XmlNodeList volumeCostLists) {
            foreach (XmlNode volumeCostList in volumeCostLists) {
                ListView listView = new ListView();
                ColumnHeader columnHeader1 = new ColumnHeader();
                ColumnHeader columnHeader2 = new ColumnHeader();
                ColumnHeader columnHeader3 = new ColumnHeader();
                ColumnHeader columnHeader4 = new ColumnHeader();
                ColumnHeader columnHeader5 = new ColumnHeader();
                               
                columnHeader1.Text = "Volume";
                columnHeader2.Text = "Disk Size";
                columnHeader3.Text = "Available";
                columnHeader4.Text = "Required";
                columnHeader5.Text = "Difference";

                columnHeader1.TextAlign = HorizontalAlignment.Left;
                columnHeader2.TextAlign = HorizontalAlignment.Right;
                columnHeader3.TextAlign = HorizontalAlignment.Right;
                columnHeader4.TextAlign = HorizontalAlignment.Right;
                columnHeader5.TextAlign = HorizontalAlignment.Right;

                listView.Columns.AddRange(new ColumnHeader[] { columnHeader1,
                                                               columnHeader2,
                                                               columnHeader3,
                                                               columnHeader4,
                                                               columnHeader5} );

                listView.Items.Add(new ListViewItem(new string[] {"C:", "30GB", "3200MB", "1MB", "3189MB" }));
                listView.View = System.Windows.Forms.View.Details;

                SetControlSizes(listView, volumeCostList);

                newDialog.AddControl(volumeCostList, listView);
            }
        }

        private void AddDirectoryCombos(DesignerForm newDialog, XmlNodeList directoryCombos) {
            foreach (XmlNode directoryCombo in directoryCombos) {
                ComboBox comboCtrl = new ComboBox();
                comboCtrl.Items.Add("Directories");
                comboCtrl.SelectedIndex = 0;

                SetControlSizes(comboCtrl, directoryCombo);

                newDialog.AddControl(directoryCombo, comboCtrl);
            }
        }

        private void AddDirectoryLists(DesignerForm newDialog, XmlNodeList directoryLists) {
            foreach (XmlNode directoryList in directoryLists) {
                ListBox listBox = new ListBox();
                listBox.Items.Add("Director content");
                listBox.SelectedIndex = 0;

                SetControlSizes(listBox, directoryList);

                newDialog.AddControl(directoryList, listBox);
            }
        }

        private void AddSelectionTrees(DesignerForm newDialog, XmlNodeList selectionTrees) {
            foreach (XmlNode selectionTree in selectionTrees) {
                TreeView treeView = new TreeView();
                treeView.Scrollable = false;
                treeView.Nodes.Add(new TreeNode("Selection tree"));

                SetControlSizes(treeView, selectionTree);

                newDialog.AddControl(selectionTree, treeView);
            }
        }
        
        private void AddBackgroundBitmaps(DesignerForm newDialog, XmlNodeList bitmaps) {
            PictureControl pictureCtrl = null;
            ArrayList allPictureControls = new ArrayList();

            foreach (XmlNode bitmap in bitmaps) {
                string binaryId = GetTextFromXmlElement(bitmap);

                Bitmap bmp = null;
                try {
                    bmp = new Bitmap(GetBinaryStream(binaryId));
                } catch {
                }

                pictureCtrl = new PictureControl(bmp, allPictureControls);
                allPictureControls.Add(pictureCtrl);

                SetControlSizes(pictureCtrl, bitmap);
               
                newDialog.AddControl(bitmap, pictureCtrl);
            }

            if (pictureCtrl != null) {
                pictureCtrl.Draw();
            }
        }

        private void SetControlSizes(Control control, XmlNode controlElement) {
            control.Left = DialogUnitsToPixelsWidth(XmlConvert.ToInt32(controlElement.Attributes["X"].Value));
            control.Top = DialogUnitsToPixelsHeight(XmlConvert.ToInt32(controlElement.Attributes["Y"].Value));
            control.Width = DialogUnitsToPixelsWidth(XmlConvert.ToInt32(controlElement.Attributes["Width"].Value));
            control.Height = DialogUnitsToPixelsHeight(XmlConvert.ToInt32(controlElement.Attributes["Height"].Value));

            //control.ClientSize = new Size(DialogUnitsToPixels(XmlConvert.ToInt32(controlElement.Attributes["Width"].Value)), DialogUnitsToPixels(XmlConvert.ToInt32(button.Attributes["Height"].Value)));
        }

        private void SetText(Control textControl, XmlNode textElement) {
            string textValue = GetTextFromXmlElement(textElement);

            int startFont = textValue.IndexOf("{\\");
            if (startFont < 0) {
                startFont = textValue.IndexOf("{&");
            }
            if (startFont >= 0) {
                int endFont = textValue.IndexOf("}", startFont);

                Font font = definedFonts[textValue.Substring(startFont+2, endFont-startFont-2)] as Font;
                if (font != null) {
                    textControl.Font = font;
                }

                textValue = textValue.Remove(startFont, endFont-startFont+1);
            }

            textControl.Text = textValue;
        }

        private void SetTag(Control textControl, XmlNode textElement) {
            string textValue = textElement.InnerText;         

            textControl.Tag = textValue;
        }


        private string GetTextFromXmlElement(XmlNode textElement) {
            return GetFromXmlElement(textElement, "Text");
        }

        private string GetFromXmlElement(XmlNode textElement, string propertyToGet) {
            string textValue = String.Empty;

            if (textElement.Attributes[propertyToGet] != null) {
                textValue = ExpandWixProperties(textElement.Attributes[propertyToGet].Value);
            } else {
                XmlNode text = textElement.SelectSingleNode("wix:"+propertyToGet, wixFiles.WxsNsmgr);
                if (text != null) {
                    textValue = ExpandWixProperties(text.InnerText);
                }
            }

            return textValue;
        }

        private Stream GetBinaryStream(string binaryId) {
            XmlNode binaryNode = wixFiles.WxsDocument.SelectSingleNode(String.Format("//wix:Binary[@Id='{0}']", binaryId), wixFiles.WxsNsmgr);
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
                    FileInfo[] files = wixFiles.WxsDirectory.GetFiles(src);
                    if (files.Length != 1) {
                        throw new FileNotFoundException(String.Format("File of binary with id \"{0}\" is not found.", binaryId), src);
                    }

                    return files[0].OpenRead();
                }
            }
        }
    }
}
