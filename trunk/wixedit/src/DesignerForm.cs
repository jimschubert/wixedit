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
    public class DesignerForm : Form {
        Hashtable controlMap;
        string selectedNodeId;

        public DesignerForm() {
            controlMap = new Hashtable();
        }

        public XmlNode SelectedNode {
            set {
                if (value == null || value.Attributes["Id"] == null || value.Attributes["Id"].Value == null || value.Attributes["Id"].Value.Trim().Length == 0) {
                    selectedNodeId = null;
                } else {
                    selectedNodeId = value.Attributes["Id"].Value;
                }

                this.Invalidate();
            }
        }

        protected override void OnPaint(PaintEventArgs e) {
            base.OnPaint(e);

            if (selectedNodeId != null) {
                Control ctrl = (Control) controlMap[selectedNodeId];
                if (ctrl != null) {
                    DrawSelection(ctrl, e.Graphics);
                }
            }
        }

        public void AddControl(XmlNode controlDefinition, Control control) {
            Controls.Add(control);

            String nodeId = controlDefinition.Attributes["Id"].Value;
            controlMap.Add(nodeId, control);
        }

        public void DrawSelection(Control ctrl, Graphics formGraphics) {
            Brush horBorderBrush = new TextureBrush(new Bitmap(WixFiles.GetResourceStream("hcontrolborder.bmp")));
            Brush verBorderBrush = new TextureBrush(new Bitmap(WixFiles.GetResourceStream("vcontrolborder.bmp")));

            Rectangle topBorder = new Rectangle(ctrl.Left, ctrl.Top - 7, ctrl.Width, 7);
            Rectangle bottomBorder = new Rectangle(ctrl.Left, ctrl.Bottom, ctrl.Width, 7);

            formGraphics.FillRectangles(horBorderBrush, new Rectangle[] {topBorder, bottomBorder});

            Rectangle rightBorder = new Rectangle(ctrl.Right, ctrl.Top, 7, ctrl.Height);
            Rectangle leftBorder = new Rectangle(ctrl.Left - 7, ctrl.Top, 7, ctrl.Height);

            formGraphics.FillRectangles(verBorderBrush, new Rectangle[] {rightBorder, leftBorder});


            Rectangle leftTop = new Rectangle(ctrl.Left - 7, ctrl.Top - 7, 7, 7);
            Rectangle rightTop = new Rectangle(ctrl.Right, ctrl.Top - 7, 7, 7);

            Rectangle leftBottom = new Rectangle(ctrl.Left - 7, ctrl.Bottom, 7, 7);
            Rectangle rightBottom = new Rectangle(ctrl.Right, ctrl.Bottom, 7, 7);

            Rectangle leftMid = new Rectangle(ctrl.Left - 7, ctrl.Top + (ctrl.Height-7)/2, 7, 7);
            Rectangle rightMid = new Rectangle(ctrl.Right, ctrl.Top + (ctrl.Height-7)/2, 7, 7);

            Rectangle midBottom = new Rectangle(ctrl.Left + (ctrl.Width-7)/2, ctrl.Bottom, 7, 7);
            Rectangle midTop = new Rectangle(ctrl.Left + (ctrl.Width-7)/2, ctrl.Top - 7, 7, 7);


            formGraphics.FillRectangles(Brushes.White, new Rectangle[] {leftTop, rightTop, leftBottom, rightBottom, leftMid, rightMid, midBottom, midTop});
            formGraphics.DrawRectangles(Pens.Black, new Rectangle[] {leftTop, rightTop, leftBottom, rightBottom, leftMid, rightMid, midBottom, midTop});
        }
    }
}