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
using System.Collections;
using System.ComponentModel;
using System.Xml;

namespace WixEdit.PropertyGridExtensions {
    public class SimpleTypeConverter: StringConverter {
        XmlNodeList enumeration; 
        
        public override bool GetStandardValuesSupported(ITypeDescriptorContext context) {
            XmlNodeList e = GetEnumeration(context);

            return IsValidEnumeration(e);
        }
        
        public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context) {
            XmlNodeList e = GetEnumeration(context);

            if (IsValidEnumeration(e)) {
                ArrayList strings = new ArrayList();
                foreach (XmlNode node in e) {
                    strings.Add(node.Attributes["value"].Value);
                }

                return new StandardValuesCollection(strings.ToArray(typeof(string)));
            }

            return new StandardValuesCollection(new string[]{});
        } 
        
        public override bool GetStandardValuesExclusive(ITypeDescriptorContext context) {
            XmlNodeList e = GetEnumeration(context);

            return IsValidEnumeration(e);
        }

        private XmlNodeList GetEnumeration(ITypeDescriptorContext context) {
            if (enumeration == null) {
                XmlAttributeAdapter adapter = context.Instance as XmlAttributeAdapter;
                XmlAttributePropertyDescriptor desc = context.PropertyDescriptor as XmlAttributePropertyDescriptor;
    
                XmlAttribute typeAttrib = desc.AttributeDescription.Attributes["type"];
                if (typeAttrib == null) {
                    enumeration = desc.AttributeDescription.SelectNodes("xs:simpleType/xs:restriction/xs:enumeration", adapter.WixFiles.XsdNsmgr);
                } else {
                    string simpleType = desc.AttributeDescription.Attributes["type"].Value;
                    string selectString = String.Format("/xs:schema/xs:simpleType[@name='{0}']/xs:restriction/xs:enumeration", simpleType);

                    enumeration = adapter.WixFiles.XsdDocument.SelectNodes(selectString, adapter.WixFiles.XsdNsmgr);
                }
            }

            return enumeration;
        }

        private bool IsValidEnumeration(XmlNodeList e) {
            if (e != null && e.Count > 0) {
                return true;
            }

            return false;
        }
    }
}