using System;
using System.IO;
using System.Windows.Forms;
using System.Xml;

using WixEdit.Settings;

namespace WixEdit.PropertyGridExtensions {
    /// <summary>
    /// PropertyDescriptor for CustomTableRowElements.
    /// </summary>
    public class CustomTableRowElementPropertyDescriptor : CustomXmlPropertyDescriptorBase {
        public CustomTableRowElementPropertyDescriptor(XmlNode rowElement, WixFiles wixFiles, string name, Attribute[] attrs)
            :
            base(wixFiles, rowElement, name, attrs)
        {
        }

        public override Type PropertyType
        {
            get
            {
                Type result = typeof(string);

                XmlElement node = (XmlElement)XmlElement.ParentNode.SelectSingleNode(String.Format("wix:Column[@Id='{0}']", this.Name), this.wixFiles.WxsNsmgr);
                if (node != null)
                {
                    switch (node.GetAttribute("Type"))
                    {
                        case "int":
                        case "integer":
                            result = typeof(int);
                            break;
                        case "string":
                            result = typeof(string);
                            break;
                        case "binary":
                            result = typeof(string);
                            break;
                    }
                }

                return result;
            }
        }

        public override object GetValue(object component) {
            CustomTableRowElementAdapter adapter = (CustomTableRowElementAdapter)component;

            XmlNode node = adapter.XmlElement.SelectSingleNode(String.Format("wix:Data[@Column='{0}']", this.Name), this.wixFiles.WxsNsmgr);
            if (node == null)
            {
                return String.Empty;
            }

            return node.InnerText;
        }

        public override void SetValue(object component, object value) {
            wixFiles.UndoManager.BeginNewCommandRange();

            CustomTableRowElementAdapter adapter = (CustomTableRowElementAdapter)component;

            XmlNode node = adapter.XmlElement.SelectSingleNode(String.Format("wix:Data[@Column='{0}']", this.Name), this.wixFiles.WxsNsmgr);
            if (node == null)
            {
                XmlElement newNode = adapter.XmlElement.OwnerDocument.CreateElement("Data", WixFiles.WixNamespaceUri);
                adapter.XmlElement.AppendChild(newNode);
                newNode.SetAttribute("Column", this.Name);
                node = newNode;
            }

            if (value == null || value.ToString().Length == 0) {
                node.InnerText = String.Empty;
            } else {
                node.InnerText = value.ToString();
            }
        }

        public override bool CanResetValue(object component) {
            return true;
        }
    }
}
