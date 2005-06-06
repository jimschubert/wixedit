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

using System.Windows.Forms;

namespace WixEdit.PropertyGridExtensions {
    /// <summary>
    /// This class adapts attributes of a xml node to properties, suitable for the <c>PropertyGrid</c>.
    /// </summary>
    public class XmlAttributeAdapter : PropertyAdapterBase {
        protected XmlNode xmlNode;
        protected XmlNode xmlNodeDefinition;
        protected bool showInnerTextIfEmpty;

        public bool ShowInnerTextIfEmpty {
            get {
                return this.showInnerTextIfEmpty;
            }
            set {
                this.showInnerTextIfEmpty = value;
            }
        }

        public XmlAttributeAdapter(XmlNode xmlNode, WixFiles wixFiles) : base(wixFiles) {
            this.xmlNode = xmlNode;

            this.xmlNodeDefinition = wixFiles.XsdDocument.SelectSingleNode(String.Format("//xs:element[@name='{0}']/xs:complexType/xs:simpleContent/xs:extension", xmlNode.Name), wixFiles.XsdNsmgr);

            if (this.xmlNodeDefinition == null) {
                this.xmlNodeDefinition = wixFiles.XsdDocument.SelectSingleNode(String.Format("//xs:element[@name='{0}']/xs:complexType", xmlNode.Name), wixFiles.XsdNsmgr);
                this.showInnerTextIfEmpty = false;
            } else {
                this.showInnerTextIfEmpty = true;
            }
        }

        public XmlNode XmlNode {
            get { 
                return this.xmlNode;
            }
        }

        public XmlNode XmlNodeDefinition {
            get { 
                return this.xmlNodeDefinition;
            }
        }

        public override PropertyDescriptorCollection GetProperties(Attribute[] attributes) {
            ArrayList props = new ArrayList();

/*
// This shows all existing elements
            foreach(XmlAttribute xmlAttribute in xmlNode.Attributes) {
                // Select attribute definition.
                string selectAttribute = String.Format("//xs:attribute[@name='{0}']", xmlAttribute.Name);
                XmlNode xmlAttributeDefinition = this.xmlNodeDefinition.SelectSingleNode(selectAttribute, wixFiles.XsdNsmgr);
/**/
// This shows all existing + required elements
            XmlNodeList xmlAttributeDefinitions = this.xmlNodeDefinition.SelectNodes("xs:attribute", wixFiles.XsdNsmgr);
            foreach(XmlNode xmlAttributeDefinition in xmlAttributeDefinitions) {
                XmlAttribute xmlAttribute = xmlNode.Attributes[xmlAttributeDefinition.Attributes["name"].Value];

                if (xmlAttribute == null) {
                    if (xmlAttributeDefinition.Attributes["use"] == null || 
                        xmlAttributeDefinition.Attributes["use"].Value != "required") {
                        continue;
                    }

                    // If there is no attibute present, create one.
                    if (xmlAttribute == null) {
                        xmlAttribute = xmlNode.OwnerDocument.CreateAttribute(xmlAttributeDefinition.Attributes["name"].Value);
                        xmlNode.Attributes.Append(xmlAttribute);
                    }                    
                }
/**/

                ArrayList attrs = new ArrayList();

                // Add default attributes Category, TypeConverter and Description
                attrs.Add(new CategoryAttribute("WXS Attribute"));
                attrs.Add(new TypeConverterAttribute(GetAttributeType(xmlAttributeDefinition)));

                XmlNode documentation = xmlAttributeDefinition.SelectSingleNode("xs:annotation/xs:documentation", wixFiles.XsdNsmgr);
                if(documentation != null) {
                    attrs.Add(new DescriptionAttribute(documentation.InnerText));
                }

                // We could add an UITypeEditor if desired
                // attrs.Add(new EditorAttribute(?property.EditorTypeName?, typeof(UITypeEditor)));
                

                // Make Attribute array
                Attribute[] attrArray = (Attribute[])attrs.ToArray(typeof(Attribute));



                // Create and add PropertyDescriptor
                XmlAttributePropertyDescriptor pd = new XmlAttributePropertyDescriptor(xmlAttribute, xmlAttributeDefinition,
                                                                           xmlAttributeDefinition.Attributes["name"].Value, attrArray);
                
                props.Add(pd);
            }

            if (this.XmlNodeDefinition.Name == "xs:extension" &&
                ( (xmlNode.InnerText != null && xmlNode.InnerText.Length > 0) || showInnerTextIfEmpty == true) ) {
//                xmlNode.InnerText = "";

                ArrayList attrs = new ArrayList();

                // Add default attributes Category, TypeConverter and Description
                attrs.Add(new CategoryAttribute("WXS Attribute"));
                attrs.Add(new TypeConverterAttribute(typeof(StringConverter)));

                attrs.Add(new DescriptionAttribute("InnerText of the element."));


                // Make Attribute array
                Attribute[] attrArray = (Attribute[])attrs.ToArray(typeof(Attribute));

                // Create and add PropertyDescriptor
                InnerTextPropertyDescriptor pd = new InnerTextPropertyDescriptor(xmlNode, attrArray);
                
                props.Add(pd);
            }

            PropertyDescriptor[] propArray = props.ToArray(typeof(PropertyDescriptor)) as PropertyDescriptor[];

            return new PropertyDescriptorCollection(propArray);
        }

        private Type GetAttributeType(XmlNode xmlAttributeDefinition) {
            if (xmlAttributeDefinition.Attributes["type"] == null) {
                return typeof(SimpleTypeConverter);
            }

            switch (xmlAttributeDefinition.Attributes["type"].Value.ToLower()) {
                case "xs:string":
                    return typeof(StringConverter);
                case "xs:int":
                case "xs:integer":
                    return typeof(Int32Converter);
                case "xs:datetime":
                    return typeof(DateTimeConverter);
                case "yesnotype":
                    return typeof(SimpleTypeConverter);
                default:
                    return typeof(StringConverter);
            }
        }
    }
}
