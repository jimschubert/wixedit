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

using WixEdit.Settings;

namespace WixEdit {
    public class PathHelper {   
        public static string GetRelativePath(string path, WixFiles wixFiles) {
            if (Path.IsPathRooted(path) == false) {
                path = Path.Combine(wixFiles.WxsDirectory.FullName, path);
            }

            if (File.Exists(path) == false) {
                throw new WixEditException(String.Format("{0} could not be located", path));
            }

            string sepCharString = Path.DirectorySeparatorChar.ToString();
            if (WixEditSettings.Instance.UseRelativeOrAbsolutePaths == PathHandling.ForceAbolutePaths) {
                return path;
            }

            Uri newBinaryPath = new Uri(path, true);

            string binaries = wixFiles.WxsDirectory.FullName;
            if (binaries.EndsWith(sepCharString) == false) {
                binaries = binaries + sepCharString;
            }

            Uri binariesPath = new Uri(binaries, true);
            
            string relativeValue = binariesPath.MakeRelative(newBinaryPath);
            relativeValue = relativeValue.Replace("/", sepCharString);
            
            FileInfo testRelativeValue = null;

            try {
                if (Path.IsPathRooted(relativeValue) == false) {
                    testRelativeValue = new FileInfo(Path.Combine(binaries, relativeValue));
                } else {
                    if (relativeValue.StartsWith("file:")) {
                        relativeValue.Remove(0, 5);
                    }

                    testRelativeValue = new FileInfo(relativeValue);
                }
            } catch (NotSupportedException ex) {
                throw new WixEditException(String.Format("The format of the path \"{0}\" is not supported!", relativeValue), ex);
            } catch (PathTooLongException ex) {
                throw new WixEditException(String.Format("The the path \"{0}\" is too long after being fully qualified. Make sure path is less than 260 characters.", relativeValue), ex);
            }
            
            if (WixEditSettings.Instance.UseRelativeOrAbsolutePaths == PathHandling.ForceRelativePaths && Path.IsPathRooted(relativeValue) == true) {
                throw new WixEditException(String.Format("{0} is invalid. {1} should be relative to {2}, or change your preference in the WixEdit settings.", relativeValue, path, binaries));
            }

            return relativeValue;
        }

        public static string GetShortFileName(FileInfo fileInfo, WixFiles wixFiles, XmlNode componentElement) {
            string nameStart = Path.GetFileNameWithoutExtension(fileInfo.Name).ToUpper().Replace(" ", "");
            int tooShort = 0;
            if (nameStart.Length > 7) {
                nameStart = nameStart.Substring(0, 7);
            } else {
                tooShort = 7 - nameStart.Length;
            }

            string nameExtension = fileInfo.Extension.ToUpper();
            if (nameExtension.Length > 4) {
                nameExtension = nameExtension.Substring(0, 4);
            }
                    
            int i = 1;
            string shortFileName = String.Format("{0}{1}{2}", nameStart, i, nameExtension);

            while (componentElement.SelectSingleNode(String.Format("wix:File[@Name={0}]", XPathHelper.EscapeXPathInputString(shortFileName)), wixFiles.WxsNsmgr) != null) {
                if (i%10 == 9) {
                    if (tooShort > 0) {
                        tooShort--;
                    } else {
                        if (nameStart.Length <= 1) {
                            throw new WixEditException("Cannot determine unique short name for " + fileInfo.Name);
                        }
                        nameStart = nameStart.Substring(0, nameStart.Length - 1);
                    }
                }

                shortFileName = String.Format("{0}{1}{2}", nameStart, ++i, nameExtension);
            }

            return shortFileName;
        }

        public static string GetShortDirectoryName(DirectoryInfo directoryInfo, WixFiles wixFiles, XmlNode componentElement) {
            string nameStart = directoryInfo.Name;
            int tooShort = 0;
            if (nameStart.Length > 7) {
                nameStart = nameStart.Substring(0, 7);
            } else {
                tooShort = 7 - nameStart.Length;
            }

            int i = 1;
            string shortDirectoryName = String.Format("{0}{1}", nameStart, i);

            while (componentElement.SelectSingleNode(String.Format("wix:Directory[@Name={0}]", XPathHelper.EscapeXPathInputString(shortDirectoryName)), wixFiles.WxsNsmgr) != null) {
                if (i%10 == 9) {
                    if (tooShort > 0) {
                        tooShort--;
                    } else {
                        nameStart = nameStart.Substring(0, nameStart.Length - 1);
                    }
                }

                shortDirectoryName = String.Format("{0}{1}", nameStart, ++i);
            }

            return shortDirectoryName;
        }
     }
}