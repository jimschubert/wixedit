using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Windows.Forms;
using System.Drawing;
using System.Text.RegularExpressions;
using WixEdit.Controls;

namespace WixEdit.Wizard
{
    class StepSheet : BaseSheet
    {
        ErrorProviderFixed errorProvider;
        Label titleLabel;
        Label descriptionLabel;
        Label lineLabel;
        XmlElement stepElement;
        XmlNamespaceManager xmlnsmgr;
        Dictionary<string, string> existingNsTranslations = new Dictionary<string, string>();

        public StepSheet(XmlElement step, WizardForm creator) : base(creator)
        {
            this.stepElement = step;
            this.AutoScroll = true;

            errorProvider = new ErrorProviderFixed();
            errorProvider.ContainerControl = this;
            errorProvider.AutoPopDelay = 20000;
            errorProvider.BlinkStyle = ErrorBlinkStyle.NeverBlink;
            Bitmap b = new Bitmap(WixFiles.GetResourceStream("bmp.info.bmp"));
            errorProvider.Icon = Icon.FromHandle(b.GetHicon());

            titleLabel = new Label();
            titleLabel.Text = step.SelectSingleNode("Title").InnerText;
            titleLabel.Dock = DockStyle.Top;
            titleLabel.Height = 15;
            titleLabel.Left = 0;
            titleLabel.Top = 0;
            titleLabel.Padding = new Padding(5, 0, 5, 0);
            titleLabel.Font = new Font("Verdana",
                        10,
                        FontStyle.Bold,
                        GraphicsUnit.Point
                    );
            titleLabel.BackColor = Color.White;

            descriptionLabel = new Label();
            descriptionLabel.Text = step.SelectSingleNode("Description").InnerText;
            descriptionLabel.Dock = DockStyle.Top;
            descriptionLabel.Height = 50 - titleLabel.Height;
            descriptionLabel.Left = 0;
            descriptionLabel.Top = titleLabel.Height;
            descriptionLabel.Padding = new Padding(8, 3, 5, 0);
            descriptionLabel.BackColor = Color.White;

            this.Controls.Add(descriptionLabel);

            this.Controls.Add(titleLabel);


            lineLabel = new Label();
            lineLabel.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            lineLabel.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            lineLabel.Location = new System.Drawing.Point(0, titleLabel.Height + descriptionLabel.Height);
            lineLabel.Size = new System.Drawing.Size(this.Width, 2);

            this.Controls.Add(lineLabel);

            Control prevControl = lineLabel;

            xmlnsmgr = new XmlNamespaceManager(step.OwnerDocument.NameTable);

            // Check the TemplatePart@SelectionTarget, then the first control
            // should be a selection control.

            foreach (XmlNode templatePartNode in step.SelectNodes("TemplatePart"))
            {
                XmlElement templatePart = (XmlElement) templatePartNode;
                String selectionTarget = templatePart.GetAttribute("SelectionTarget");
                if (selectionTarget != null && selectionTarget != String.Empty)
                {
                    Label label = new Label();
                    label.Width = this.Width - 10;
                    label.Height = 14;
                    label.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
                    label.Text = "Select target location";
                    label.Top = prevControl.Bottom + 4;
                    label.Left = 5;
                    this.Controls.Add(label);

                    ComboBox text = new ComboBox();
                    text.DropDownStyle = ComboBoxStyle.DropDownList;
                    foreach (XmlNode dir in Wizard.WixFiles.WxsDocument.SelectNodes(selectionTarget, Wizard.WixFiles.WxsNsmgr))
                    {
                        text.Items.Add(dir.Attributes["Id"]);
                    }
                    text.DisplayMember = "Value";
                    text.Width = this.Width - 14;
                    text.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
                    // text.Items...
                    text.Top = prevControl.Bottom + label.Height + 4;
                    text.Left = 7;
                    text.Name = selectionTarget;
                    this.Controls.Add(text);

                    prevControl = text;
                }
            }

            foreach (XmlElement edit in step.SelectNodes("Edit"))
            {
                if (edit.GetAttribute("Mode") == "GenerateGuid")
                {
                    continue;
                }

                string refAtt = edit.GetAttribute("Ref");
                ExtractNamespaces(edit, xmlnsmgr, refAtt);

                Label label = new Label();
                label.Width = this.Width - 10;
                label.Height = 14;
                label.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
                label.Text = edit.GetAttribute("Description");
                if (label.Text == String.Empty)
                {
                    label.Text = edit.GetAttribute("Name");
                }
                if (label.Text == String.Empty)
                {
                    label.Text = refAtt.Replace('/', ' ').Replace('[', ' ').Replace(']', ' ').Replace(':', ' ').Replace('@', ' ').Replace("  ", " ");
                }
                label.Top = prevControl.Bottom + 4;
                label.Left = 5;
                this.Controls.Add(label);

                XmlNode theNode = step.SelectSingleNode("TemplatePart/" + TranslateNamespace(refAtt), xmlnsmgr);
                XmlDocumentationManager mgr = new XmlDocumentationManager(this.Wizard.WixFiles);
                XmlNode xmlNodeDefinition = mgr.GetXmlNodeDefinition(theNode);

                switch (edit.GetAttribute("Mode"))
                {
                    case "Select":
                        ComboBox select = new ComboBox();
                        select.DropDownStyle = ComboBoxStyle.DropDownList;
                        select.Width = this.Width - 14;
                        select.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;

                        String selectionTarget = edit.GetAttribute("Selection");

                        foreach (XmlNode dir in Wizard.WixFiles.WxsDocument.SelectNodes(selectionTarget, Wizard.WixFiles.WxsNsmgr))
                        {
                            select.Items.Add(dir);
                        }

                        select.DisplayMember = "Value";
                        select.Top = prevControl.Bottom + label.Height + 4;
                        select.Left = 7;
                        select.Name = refAtt;
                        this.Controls.Add(select);

                        prevControl = select;
                        break;
                    case "Dropdown":
                        ComboBox combo = new ComboBox();
                        combo.DropDownStyle = ComboBoxStyle.DropDownList;
                        combo.Width = this.Width - 14;
                        combo.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
                        combo.DisplayMember = "InnerText";
                        foreach (XmlNode optionNode in edit.SelectNodes("Option"))
                        {
                            XmlElement optionElement = (XmlElement)optionNode;
                            combo.Items.Add(optionNode);
                            if (optionElement.GetAttribute("Value") == theNode.InnerText)
                            {
                                combo.SelectedItem = optionNode;
                            }
                        }

                        combo.Top = prevControl.Bottom + label.Height + 4;
                        combo.Left = 7;
                        combo.Name = refAtt;
                        this.Controls.Add(combo);

                        prevControl = combo;
                        break;
                    default:
                        TextBox text = new TextBox();
                        text.Width = this.Width - 14;
                        text.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
                        text.Text = theNode.Value;
                        text.Top = prevControl.Bottom + label.Height + 4;
                        text.Left = 7;
                        text.Name = refAtt;
                        this.Controls.Add(text);

                        prevControl = text;
                        break;
                }

                if (xmlNodeDefinition != null)
                {
                    string docu = mgr.GetDocumentation(xmlNodeDefinition, true);
                    if (!string.IsNullOrEmpty(docu))
                    {
                        prevControl.Width = prevControl.Width - 18;
                        errorProvider.SetError(prevControl, docu);
                        errorProvider.SetIconPadding(prevControl, 4);
                    }
                }
            }

        }

        private void ExtractNamespaces(XmlElement xmlElement, XmlNamespaceManager xmlnsmgr, string refAtt)
        {
            Regex r = new Regex(@"[/\[]?(?<name>[A-Za-z0-9]+)[:]");
            foreach (Match m in r.Matches(refAtt))
            {
                string pre = m.Groups["name"].Value;
                string xmlns = "";

                XmlNode parent = xmlElement;
                while (xmlns == String.Empty && parent != null)
                {
                    xmlns = parent.GetNamespaceOfPrefix(pre);
                    parent = parent.ParentNode;
                }
                if (xmlns != String.Empty)
                {
                    string existingPre = Wizard.WixFiles.WxsNsmgr.LookupPrefix(xmlns);
                    if (existingPre != null)
                    {
                        xmlnsmgr.AddNamespace(existingPre, xmlns);
                        if (existingNsTranslations.ContainsKey(pre) == false)
                        {
                            existingNsTranslations.Add(pre, existingPre);
                        }
                    }
                    else
                    {
                        xmlnsmgr.AddNamespace(pre, xmlns);
                    }
                }
            }
        }

        public override bool UndoNext()
        {
            Wizard.WixFiles.UndoManager.Undo();

            return true;
        }

        public override bool OnNext()
        {
            Wizard.WixFiles.UndoManager.BeginNewCommandRange();

            try
            {
                XmlElement stepElementClone = (XmlElement) this.stepElement.Clone();

                // Update the values in the clone element.
                foreach (XmlElement edit in stepElementClone.SelectNodes("Edit"))
                {
                    string refAtt = edit.GetAttribute("Ref");
                    XmlNode theNode = stepElementClone.SelectSingleNode("TemplatePart/" + TranslateNamespace(refAtt), xmlnsmgr);

                    if (edit.GetAttribute("Mode") == "GenerateGuid")
                    {
                        theNode.Value = Guid.NewGuid().ToString().ToUpper();
                    }
                    else if (edit.GetAttribute("Mode") == "Dropdown")
                    {
                        ComboBox combo = Controls.Find(refAtt, true)[0] as ComboBox;
                        if (combo != null)
                        {
                            XmlElement item = combo.SelectedItem as XmlElement;
                            if (item != null)
                            {
                                theNode.Value = item.GetAttribute("Value");
                            }
                        }
                    }
                    else
                    {
                        Control ctrl = Controls.Find(refAtt, true)[0];
                        if (ctrl != null)
                        {
                            theNode.Value = ctrl.Text;
                        }
                    }
                }

                foreach (XmlNode templateNode in stepElementClone.SelectNodes("TemplatePart"))
                {
                    XmlElement templatePart = (XmlElement)templateNode;

                    // Import the nodes into the wxs file.
                    List<XmlNode> importedNodes = new List<XmlNode>();
                    foreach (XmlNode toImport in templatePart.ChildNodes)
                    {
                        importedNodes.Add(Wizard.WixFiles.WxsDocument.ImportNode(toImport, true));
                    }

                    XmlNode target = null;
                    if (templatePart.HasAttribute("SelectionTarget"))
                    {
                        // Get the selection target and filter by selected Id.
                        ComboBox combo = (ComboBox)Controls.Find(templatePart.GetAttribute("SelectionTarget"), true)[0];
                        XmlAttribute att = (XmlAttribute)combo.SelectedItem;
                        target = att.OwnerElement;
                    }
                    else
                    {
                        // Fix the prefix of namespaces, namespaces could 
                        // have a different prefix in the templatepart.
                        target = Wizard.WixFiles.WxsDocument.SelectSingleNode(TranslateNamespace(templatePart.GetAttribute("Target")), Wizard.WixFiles.WxsNsmgr);
                    }

                    foreach (XmlNode imported in importedNodes)
                    {
                        target.AppendChild(imported);

                        if (existingNsTranslations.Count > 0)
                        {
                            FixPrefixRecursive(imported);
                        }
                    }
                }
            }
            catch
            {
                return false;
            }

            return true;
        }

        private void FixPrefixRecursive(XmlNode imported)
        {
            if (existingNsTranslations.ContainsKey(imported.Prefix))
            {
                if (Wizard.WixFiles.WxsDocument.DocumentElement.NamespaceURI == xmlnsmgr.LookupNamespace(imported.Prefix))
                {
                    imported.Prefix = "";
                }
                else
                {
                    imported.Prefix = existingNsTranslations[imported.Prefix];
                }
            }

            foreach (XmlNode node in imported.ChildNodes)
            {
                FixPrefixRecursive(node);
            }
        }

        private string TranslateNamespace(string toFix)
        {
            SortedList<int, int> hits = new SortedList<int, int>();
            Dictionary<int, string> nsMap = new Dictionary<int, string>();

            Regex r = new Regex(@"[/\[]?(?<name>[A-Za-z0-9]+)[:]");
            foreach (Match m in r.Matches(toFix))
            {
                string pre = m.Groups["name"].Value;
                if (existingNsTranslations.ContainsKey(pre))
                {
                    pre = existingNsTranslations[pre];
                }
                string ns = xmlnsmgr.LookupNamespace(pre);
                string newPre = Wizard.WixFiles.WxsNsmgr.LookupPrefix(ns);

                if (newPre == null)
                {
                    continue;
                }

                hits.Add(m.Groups["name"].Index, m.Groups["name"].Length);
                nsMap.Add(m.Groups["name"].Index, newPre);
            }

            string result = toFix;
            for (int i = hits.Count-1; i >= 0; i--)
            {
                int key = hits.Keys[i];
                int val = hits[key];

                result = result.Substring(0, key) + nsMap[key] + result.Substring(key + val);
            }

            return result;
        }
    }
}
