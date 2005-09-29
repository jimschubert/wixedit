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
using System.Xml;

using WixEdit.Settings;

namespace WixEdit {
    public class UndoManager {
        ArrayList undoCommands;
        ArrayList redoCommands;

        bool beginNewCommandRange;
        
        XmlDocument wxsDocument;

        XmlNodeChangedEventHandler nodeChangedHandler;
        XmlNodeChangedEventHandler nodeChangingHandler; 
        XmlNodeChangedEventHandler nodeInsertedHandler;
        XmlNodeChangedEventHandler nodeRemovingHandler;

        DateTime timeCheck;

        public UndoManager(XmlDocument wxsDocument) {
            undoCommands = new ArrayList();
            redoCommands = new ArrayList();

            beginNewCommandRange = true;

            this.wxsDocument = wxsDocument;

            nodeChangedHandler  = new XmlNodeChangedEventHandler(OnNodeChanged); 
            nodeChangingHandler = new XmlNodeChangedEventHandler(OnNodeChanging);
            nodeInsertedHandler = new XmlNodeChangedEventHandler(OnNodeInserted);
            nodeRemovingHandler = new XmlNodeChangedEventHandler(OnNodeRemoving);

            this.wxsDocument.NodeChanged += nodeChangedHandler;
            this.wxsDocument.NodeChanging += nodeChangingHandler; 
            this.wxsDocument.NodeInserted += nodeInsertedHandler;
            this.wxsDocument.NodeRemoving += nodeRemovingHandler;

            timeCheck = DateTime.Now;
        }

        private void RegisterHandlers() {
            wxsDocument.NodeChanged += nodeChangedHandler;
            wxsDocument.NodeChanging += nodeChangingHandler; 
            wxsDocument.NodeInserted += nodeInsertedHandler;
            wxsDocument.NodeRemoving += nodeRemovingHandler;
        }

        private void DeregisterHandlers() {
            wxsDocument.NodeChanged -= nodeChangedHandler;
            wxsDocument.NodeChanging -= nodeChangingHandler; 
            wxsDocument.NodeInserted -= nodeInsertedHandler;
            wxsDocument.NodeRemoving -= nodeRemovingHandler;
        }

        public void BeginNewCommandRange() {
            timeCheck = DateTime.Now;

            beginNewCommandRange = true;
        }

        /// <summary>
        /// Every action needs to start with a call to BeginNewCommandRange(), because there can 
        /// be multiple commands following right after each other in one action. If it's longer 
        /// ago than 250 ms, somewhere the call to BeginNewCommandRange() might be forgotten.
        /// </summary>
        /// <remarks>Disable for releases, but in develop time it could be handy.</remarks>
        public void CheckTime() {
            TimeSpan diff = DateTime.Now.Subtract(timeCheck);
            if (diff.TotalMilliseconds > 250) {
                // System.Windows.Forms.MessageBox.Show("Warning, the undo-system might be corrupted.");
            }
        }

        public void OnNodeChanged(Object src, XmlNodeChangedEventArgs args) {
            // Get new value and node
            redoCommands.Clear();

            CheckTime();
            undoCommands.Add(new ChangeCommand(args.Node, oldNodeValue, args.Node.Value, beginNewCommandRange));

            beginNewCommandRange = false;
        }

        string oldNodeValue;
        public void OnNodeChanging(Object src, XmlNodeChangedEventArgs args) {
            oldNodeValue = args.Node.Value;
        }

        public void OnNodeInserted(Object src, XmlNodeChangedEventArgs args) {
            // Get parent node and node
            if (args.NewParent.Name == "xmlns:xml") {
                return;
            }

            redoCommands.Clear();

            CheckTime();
            undoCommands.Add(new InsertCommand(args.NewParent, args.Node, beginNewCommandRange));

            beginNewCommandRange = false;
        }

        public void OnNodeRemoving(Object src, XmlNodeChangedEventArgs args) {
            // Get parent node and node
            redoCommands.Clear();

            CheckTime();
            undoCommands.Add(new RemoveCommand(args.OldParent, args.Node, beginNewCommandRange));

            beginNewCommandRange = false;
        }


        public void Clear() {
            undoCommands.Clear();
            redoCommands.Clear();
        }

        public bool CanRedo() {
            return redoCommands.Count > 0;
        }

        public bool CanUndo() {
            return undoCommands.Count > 0;
        }

        public bool HasChanges() {
            return CanUndo();
        }

        public XmlNode Redo() {
            XmlNode affectedNode = null;
            ArrayList affectedNodes = new ArrayList();

            if (redoCommands.Count > 0) {
                DeregisterHandlers();

                IReversibleCommand command = (IReversibleCommand) redoCommands[redoCommands.Count-1];
                do {
                    affectedNodes.Add(command.Redo());
                    
                    redoCommands.Remove(command);
                    undoCommands.Add(command);

                    if (redoCommands.Count > 0) {
                        command = (IReversibleCommand) redoCommands[redoCommands.Count-1];
                    }
                } while (redoCommands.Count > 0 && command.BeginCommandRange == false);

                RegisterHandlers();
            }

            foreach (XmlNode node in affectedNodes) {
                if (node is XmlText) {
                    if (node.ParentNode is XmlAttribute) {
                        XmlAttribute att = node.ParentNode as XmlAttribute;
                        if (att.OwnerElement != null) {
                            affectedNode = att.OwnerElement;
                            break;
                        }
                    }
                } else if (node.ParentNode != null) {
                    affectedNode = node;
                    break;
                } else if (node is XmlAttribute) {
                    XmlAttribute att = node as XmlAttribute;
                    if (att.OwnerElement != null) {
                        affectedNode = att.OwnerElement;
                        break;
                    }
                }
            }

            return affectedNode;
        }

        public XmlNode Undo() {
            XmlNode affectedNode = null;
            ArrayList affectedNodes = new ArrayList();
            
            if (undoCommands.Count > 0) {
                DeregisterHandlers();

                IReversibleCommand command;
                do {
                    command = (IReversibleCommand) undoCommands[undoCommands.Count-1];

                    affectedNodes.Add(command.Undo());

                    undoCommands.Remove(command);
                    redoCommands.Add(command);
                } while (undoCommands.Count > 0 && command.BeginCommandRange == false);

                RegisterHandlers();

                foreach (XmlNode node in affectedNodes) {
                    if (node.ParentNode != null) {
                        affectedNode = node;
                        break;
                    } else if (node is XmlAttribute) {
                        XmlAttribute att = node as XmlAttribute;
                        if (att.OwnerElement != null) {
                            affectedNode = att.OwnerElement;
                            break;
                        }
                    }
                }
            }

            return affectedNode;
        }

        public string GetNextUndoActionString() {
            if (undoCommands.Count == 0) {
                return String.Empty;
            }

            for (int i = undoCommands.Count-1; i >= 0; i--) {
                IReversibleCommand cmd = (IReversibleCommand) undoCommands[i];
                if (cmd.BeginCommandRange) {
                    return cmd.GetDisplayActionString();
                }
            }

            // If the command starting the CommandRange is not found, return the first...
            return ((IReversibleCommand) undoCommands[undoCommands.Count-1]).GetDisplayActionString();
        }

        public string GetNextRedoActionString() {
            if (redoCommands.Count == 0) {
                return String.Empty;
            }

            return ((IReversibleCommand) redoCommands[redoCommands.Count-1]).GetDisplayActionString();
        }
    }

    public interface IReversibleCommand {
        bool BeginCommandRange {
            get;
            set;
        }

        XmlNode Undo();
        XmlNode Redo();

        string GetDisplayActionString();
    }

    public class InsertCommand : IReversibleCommand {
        XmlNode parentNode;
        XmlNode insertedNode;
        XmlNode previousSiblingNode;

        bool beginCommandRange;

        public InsertCommand(XmlNode parentNode, XmlNode insertedNode, bool beginCommandRange) {
            this.parentNode = parentNode;
            this.insertedNode = insertedNode;
            this.beginCommandRange = beginCommandRange;
        }

        public bool BeginCommandRange {
            get {
                return beginCommandRange;
            }
            set {
                beginCommandRange = value;
            }
        }

        public XmlNode Undo() {
            previousSiblingNode = insertedNode.PreviousSibling;
            if (insertedNode is XmlAttribute) {
                parentNode.Attributes.Remove(insertedNode as XmlAttribute);
            } else {
                parentNode.RemoveChild(insertedNode);
            }

            return parentNode;
        }

        public XmlNode Redo() {
            if (previousSiblingNode != null) {
                if (insertedNode is XmlAttribute) {
                    parentNode.Attributes.InsertAfter(insertedNode as XmlAttribute, previousSiblingNode as XmlAttribute);
                } else {
                    parentNode.InsertAfter(insertedNode, previousSiblingNode);
                }
            } else {
                if (insertedNode is XmlAttribute) {
                    parentNode.Attributes.Append(insertedNode as XmlAttribute);
                } else {
                    parentNode.InsertBefore(insertedNode, parentNode.FirstChild);
                }
            }

            return insertedNode;
        }

        public string GetDisplayActionString() {
            return "Insert";
        }
    }

    public class RemoveCommand : IReversibleCommand {
        XmlNode parentNode;
        XmlNode removedNode;
        XmlNode previousSiblingNode;
        
        bool beginCommandRange;

        public RemoveCommand(XmlNode parentNode, XmlNode removedNode, bool beginCommandRange) {
            this.parentNode = parentNode;
            this.removedNode = removedNode;
            previousSiblingNode = removedNode.PreviousSibling;
            this.beginCommandRange = beginCommandRange;
        }

        public bool BeginCommandRange {
            get {
                return beginCommandRange;
            }
            set {
                beginCommandRange = value;
            }
        }

        public XmlNode Undo() {
            if (previousSiblingNode != null) {
                if (removedNode is XmlAttribute) {
                    parentNode.Attributes.InsertAfter(removedNode as XmlAttribute, previousSiblingNode as XmlAttribute);
                } else {
                    parentNode.InsertAfter(removedNode, previousSiblingNode);
                }
            } else {
                if (removedNode is XmlAttribute) {
                    parentNode.Attributes.Append(removedNode as XmlAttribute);
                } else {
                    parentNode.InsertBefore(removedNode, parentNode.FirstChild);
                }
            }

            return removedNode;
        }

        public XmlNode Redo() {
            if (removedNode is XmlAttribute) {
                parentNode.Attributes.Remove(removedNode as XmlAttribute);
            } else {
                parentNode.RemoveChild(removedNode);
            }

            return parentNode;
        }

        public string GetDisplayActionString() {
            return "Delete";
        }
    }

    public class ChangeCommand : IReversibleCommand {
        XmlNode changedNode;
        string oldValue;
        string newValue;

        bool beginCommandRange;

        public ChangeCommand(XmlNode changedNode, string oldValue, string newValue, bool beginCommandRange) {
            this.changedNode = changedNode;
            this.oldValue = oldValue;
            this.newValue = newValue;
            this.beginCommandRange = beginCommandRange;
        }

        public bool BeginCommandRange {
            get {
                return beginCommandRange;
            }
            set {
                beginCommandRange = value;
            }
        }

        public XmlNode Undo() {
            changedNode.Value = oldValue;

            return changedNode;
        }

        public XmlNode Redo() {
            changedNode.Value = newValue;

            return changedNode;
        }

        public string GetDisplayActionString() {
            return "Change";
        }
    }
}