
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

using WixEdit.PropertyGridExtensions;

namespace WixEdit {
    /// <summary>
    /// Summary description for EditDialogPanel.
    /// </summary>
    public class EditPropertiesPanel : Panel {
        #region Controls
        private System.Windows.Forms.PropertyGrid propertyGrid;
        #endregion

        private WixFiles wixFiles;

        public EditPropertiesPanel(WixFiles wixFiles) {
            this.wixFiles = wixFiles;

            InitializeComponent();
        }

        #region Initialize Controls
        private void InitializeComponent() {
            XmlNodeList properties = wixFiles.WxsDocument.SelectNodes("/wix:Wix/wix:Product/wix:Property", wixFiles.WxsNsmgr);

            this.propertyGrid = new PropertyGrid();

            // 
            // propertyGrid
            //
            this.propertyGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.propertyGrid.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
            this.propertyGrid.Location = new System.Drawing.Point(140, 0);
            this.propertyGrid.Name = "propertyGrid";
            this.propertyGrid.Size = new System.Drawing.Size(269, 266);
            this.propertyGrid.TabIndex = 1;
            this.propertyGrid.PropertySort = PropertySort.Alphabetical;
            this.propertyGrid.ToolbarVisible = false;
            this.propertyGrid.HelpVisible = false;

            PropertyElementAdapter propAdapter = new PropertyElementAdapter(properties, wixFiles);
            this.propertyGrid.SelectedObject = propAdapter;

            this.Controls.Add(this.propertyGrid);
        }
        #endregion
    }
}
