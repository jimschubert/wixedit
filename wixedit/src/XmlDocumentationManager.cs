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
using System.Collections.Specialized;
using System.Xml;
using System.Text;

namespace WixEdit {
    /// <summary>
    /// The DocumentationManager retrieves the documentation about xml elements from the xsd definition.
    /// </summary>
    public class XmlDocumentationManager {
        private WixFiles wixFiles;

        public XmlDocumentationManager(WixFiles wixFiles) {
            this.wixFiles = wixFiles;
        }

        public bool HasDocumentation(XmlNode xmlNodeDefinition) {
            if (xmlNodeDefinition == null) {
                return false;
            }

            XmlNode documentation = xmlNodeDefinition.SelectSingleNode("xs:annotation/xs:documentation", wixFiles.XsdNsmgr);
            if(documentation == null) {
                documentation = xmlNodeDefinition.SelectSingleNode("xs:simpleContent/xs:extension/xs:annotation/xs:documentation", wixFiles.XsdNsmgr);
            }
            if(documentation == null && xmlNodeDefinition.ParentNode.Name == "xs:element") {
                documentation = xmlNodeDefinition.SelectSingleNode("../xs:annotation/xs:documentation", wixFiles.XsdNsmgr);
            }
            if(documentation == null && xmlNodeDefinition.ParentNode.ParentNode.Name == "xs:element") {
                documentation = xmlNodeDefinition.SelectSingleNode("../../xs:annotation/xs:documentation", wixFiles.XsdNsmgr);
            }

            return (documentation != null);
        }

        public string GetDocumentation(XmlNode xmlNodeDefinition) {
            StringCollection selectQueries = new StringCollection();

            if(xmlNodeDefinition.ParentNode.ParentNode.Name == "xs:element") {
                selectQueries.Add("../../xs:annotation/xs:documentation");
            }
            if(xmlNodeDefinition.ParentNode.Name == "xs:element") {
                selectQueries.Add("../xs:annotation/xs:documentation");
            }
            selectQueries.Add("xs:annotation/xs:documentation");
            selectQueries.Add("xs:simpleContent/xs:extension/xs:annotation/xs:documentation");

            StringCollection documentationStrings = new StringCollection();

            string message = null;
            XmlNode documentation = null;

            foreach (string query in selectQueries) {
                documentation = xmlNodeDefinition.SelectSingleNode(query, wixFiles.XsdNsmgr);  
                
                if(documentation != null) {
                    message = documentation.InnerText;
                    message = message.Replace("\t", " ");
                    message = message.Replace("\r\n", " ");
    
                    string newMessage = message;
                    do {
                        message = newMessage;
                        newMessage = message.Replace("  ", " ");
                    } while (message != newMessage);

                    message = newMessage;
                    message = message.Trim(' ');

                    documentationStrings.Add(message);
                }
            }

            if(documentationStrings.Count > 0) {
                StringBuilder messageBuilder = new StringBuilder();
                foreach (string documentationString in documentationStrings) {
                    messageBuilder.Append(documentationString);
                    messageBuilder.Append("\r\n\r\n");
                }
                messageBuilder.Remove(messageBuilder.Length-4, 4);
                message = messageBuilder.ToString();
            } else {
                message = "No documentation found.";
            }

            return message;
        }
    }
}
