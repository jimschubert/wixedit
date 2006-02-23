using System;
using System.Xml;

namespace WixEdit {
    /// <summary>
    /// Class to contain logic to find and/or create elements.
    /// </summary>
    public class ElementLocator {
        public static XmlNode GetUIElement(WixFiles wixFiles) {
            XmlNode ui = wixFiles.WxsDocument.SelectSingleNode("/wix:Wix/*/wix:UI", wixFiles.WxsNsmgr);
            if (ui == null) {
                XmlNodeList parents = wixFiles.WxsDocument.SelectNodes("/wix:Wix/*", wixFiles.WxsNsmgr);
                if (parents.Count == 0) {
                    return null;
                }

                XmlNode theParent = null;
                foreach (XmlNode possibleParent in parents) {
                    XmlNode def = wixFiles.XsdDocument.SelectSingleNode(String.Format("/xs:schema/xs:element[@name='{0}']/xs:complexType/xs:sequence//xs:element[@ref='UI']", possibleParent.Name), wixFiles.XsdNsmgr);
                    if (def != null) {
                        theParent = possibleParent;
                        break;
                    }
                }

                if (theParent == null) {
                    return null;
                }

                ui = wixFiles.WxsDocument.CreateElement("UI", "http://schemas.microsoft.com/wix/2003/01/wi");

                theParent.AppendChild(ui);
            }

            return ui;
        }
    }
}
