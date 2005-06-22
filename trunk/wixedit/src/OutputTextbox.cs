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
using System.Runtime.InteropServices;
using System.Windows.Forms;

public class OutputTextbox : RichTextBox {
    public OutputTextbox() : base() {
        SetStyle(ControlStyles.StandardClick, true);
    }


    #region "Property: SelectionBackColor"
    [StructLayout(LayoutKind.Sequential)]
    private struct CharFormat2 {
        public Int32 cbSize; 
        public Int32 dwMask;
        public Int32 dwEffects;
        public Int32 yHeight;
        public Int32 yOffset;
        public Int32 crTextColor;
        public Byte bCharSet;
        public Byte bPitchAndFamily;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst=32)]
        public String szFaceName;
        public Int16 wWeight;
        public Int16 sSpacing;
        public Int32 crBackColor;
        public Int32 lcid;
        public Int32 dwReserved;
        public Int16 sStyle;
        public Int16 wKerning;
        public Byte bUnderlineType;
        public Byte bAnimation;
        public Byte bRevAuthor;
        public Byte bReserved1;
    }
    #endregion

    private const Int32 LF_FACESIZE = 32;
    private const Int32 CFM_BACKCOLOR = 0x4000000;
    private const Int32 CFE_AUTOBACKCOLOR = CFM_BACKCOLOR;
    private const Int32 WM_USER = 0x400;
    private const Int32 EM_SETCHARFORMAT = (WM_USER + 68);
    private const Int32 EM_SETBKGNDCOLOR = (WM_USER + 67);
    private const Int32 EM_GETCHARFORMAT = (WM_USER + 58);
    private const Int32 WM_SETTEXT = 0xC;
    private const Int32 SCF_SELECTION = 0x1;

    [DllImport("user32.dll",EntryPoint="SendMessage")]
    private static extern int SendMessage(IntPtr hWnd, int msg, int wParam, ref CharFormat2 lParem);

    public Color SelectionBackColor {
        get {
            IntPtr HWND = this.Handle;
            CharFormat2 Format= new CharFormat2();
            Format.dwMask = CFM_BACKCOLOR;
            Format.cbSize = Marshal.SizeOf(Format);
            SendMessage(this.Handle, EM_GETCHARFORMAT, SCF_SELECTION, ref Format);
            return ColorTranslator.FromOle(Format.crBackColor);
        }
        set {
            IntPtr HWND = this.Handle;
            CharFormat2 Format = new CharFormat2();
            Format.crBackColor = ColorTranslator.ToOle(value);
            Format.dwMask = CFM_BACKCOLOR;
            Format.cbSize = Marshal.SizeOf(Format);

            SendMessage(this.Handle, EM_SETCHARFORMAT, SCF_SELECTION, ref Format);
        }
    }
}