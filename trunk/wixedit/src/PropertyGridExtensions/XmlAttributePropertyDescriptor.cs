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
using System.Xml;

namespace WixEdit.PropertyGridExtensions {
    /// <summary>
    /// Summary description for XmlAttributePropertyDescriptor.
    /// </summary>
    public class XmlAttributePropertyDescriptor : CustomPropertyDescriptorBase {
        XmlAttribute attribute;
        XmlNode description;

        public XmlAttributePropertyDescriptor(XmlAttribute attribute, XmlNode description, string name, Attribute[] attrs) :
            base(name, attrs) {
            this.attribute = attribute;
            this.description = description;
        }

        public XmlNode AttributeDescription {
            get { return description; }
        }

        public XmlAttribute Attribute {
            get { return attribute; }
        }

        public override object GetValue(object component) {
            return attribute.Value;
        }

        public override void SetValue(object component, object value) {
            // Object can be a Int or DateTime or String. Etc.
            if (value == null) {
                attribute.Value = String.Empty;
            } else {
                attribute.Value = value.ToString();
            }
        }

    }
}
