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
using System.IO;
using System.Xml;
using System.Windows.Forms;

namespace WixEdit.Import {
    /// <summary>
    /// Summary description for FileImport.
    /// </summary>
    public class FileImport {
        WixFiles wixFiles;
        FileInfo fileInfo;
        XmlNode componentElement;
        public FileImport(WixFiles wixFiles, FileInfo fileInfo, XmlNode componentElement) {
            this.wixFiles = wixFiles;
            this.fileInfo = fileInfo;
            this.componentElement = componentElement;
        }

        public void Import(TreeNode treeNode) {
            XmlElement newElement = componentElement.OwnerDocument.CreateElement("File", WixFiles.WixNamespaceUri);

            newElement.SetAttribute("Id", fileInfo.Name);
            newElement.SetAttribute("LongName", fileInfo.Name);
            newElement.SetAttribute("Name", PathHelper.GetShortFileName(fileInfo, wixFiles, componentElement));
            newElement.SetAttribute("Source", PathHelper.GetRelativePath(fileInfo.FullName, wixFiles));

            TreeNode newNode = new TreeNode(fileInfo.Name);
            newNode.Tag = newElement;
            
            int imageIndex = ImageListFactory.GetImageIndex("File");
            if (imageIndex >= 0) {
                newNode.ImageIndex = imageIndex;
                newNode.SelectedImageIndex = imageIndex;
            }

            XmlNodeList sameNodes = componentElement.SelectNodes("wix:File", wixFiles.WxsNsmgr);
            if (sameNodes.Count > 0) {
                componentElement.InsertAfter(newElement, sameNodes[sameNodes.Count - 1]);
            } else {
                componentElement.AppendChild(newElement);
            }

            treeNode.Nodes.Add(newNode);
        }
    }
}
