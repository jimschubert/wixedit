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

        XmlDocument wxsDocument;

        XmlNodeChangedEventHandler nodeChangedHandler;
        XmlNodeChangedEventHandler nodeChangingHandler; 
        XmlNodeChangedEventHandler nodeInsertedHandler;
        XmlNodeChangedEventHandler nodeRemovingHandler;

        public UndoManager(XmlDocument wxsDocument) {
            undoCommands = new ArrayList();
            redoCommands = new ArrayList();


            this.wxsDocument = wxsDocument;

            nodeChangedHandler  = new XmlNodeChangedEventHandler(OnNodeChanged); 
            nodeChangingHandler = new XmlNodeChangedEventHandler(OnNodeChanging);
            nodeInsertedHandler = new XmlNodeChangedEventHandler(OnNodeInserted);
            nodeRemovingHandler = new XmlNodeChangedEventHandler(OnNodeRemoving);

            this.wxsDocument.NodeChanged += nodeChangedHandler;
            this.wxsDocument.NodeChanging += nodeChangingHandler; 
            this.wxsDocument.NodeInserted += nodeInsertedHandler;
            this.wxsDocument.NodeRemoving += nodeRemovingHandler;
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

        public void OnNodeChanged(Object src, XmlNodeChangedEventArgs args) {
            // Get new value and node
            redoCommands.Clear();

            undoCommands.Add(new ChangeCommand(args.Node, oldNodeValue, args.Node.Value));
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

            undoCommands.Add(new InsertCommand(args.NewParent, args.Node));
        }

        public void OnNodeRemoving(Object src, XmlNodeChangedEventArgs args) {
            // Get parent node and node
            redoCommands.Clear();

            undoCommands.Add(new RemoveCommand(args.OldParent, args.Node));
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
            if (redoCommands.Count > 0) {
                DeregisterHandlers();

                IReversibleCommand command = (IReversibleCommand) redoCommands[redoCommands.Count-1];
                affectedNode = command.Redo();

                RegisterHandlers();

                redoCommands.Remove(command);
                undoCommands.Add(command);
            }

            return affectedNode;
        }

        public XmlNode Undo() {
            XmlNode affectedNode = null;
            if (undoCommands.Count > 0) {
                DeregisterHandlers();

                IReversibleCommand command = (IReversibleCommand) undoCommands[undoCommands.Count-1];
                affectedNode = command.Undo();

                RegisterHandlers();

                undoCommands.Remove(command);
                redoCommands.Add(command);
            }

            return affectedNode;
        }

        public string GetNextUndoActionString() {
            if (undoCommands.Count == 0) {
                return String.Empty;
            }

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
        XmlNode Undo();
        XmlNode Redo();

        string GetDisplayActionString();
    }

    public class InsertCommand : IReversibleCommand {
        XmlNode parentNode;
        XmlNode insertedNode;
        XmlNode previousSiblingNode;

        public InsertCommand(XmlNode parentNode, XmlNode insertedNode) {
            this.parentNode = parentNode;
            this.insertedNode = insertedNode;
        }

        public XmlNode Undo() {
            previousSiblingNode = insertedNode.PreviousSibling;
            parentNode.RemoveChild(insertedNode);

            return parentNode;
        }

        public XmlNode Redo() {
            if (previousSiblingNode != null) {
                parentNode.InsertAfter(insertedNode, previousSiblingNode);
            } else {
                parentNode.InsertBefore(insertedNode, parentNode.FirstChild);
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

        public RemoveCommand(XmlNode parentNode, XmlNode removedNode) {
            this.parentNode = parentNode;
            this.removedNode = removedNode;
        }

        public XmlNode Undo() {
            parentNode.AppendChild(removedNode);

            return removedNode;
        }

        public XmlNode Redo() {
            parentNode.RemoveChild(removedNode);

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

        public ChangeCommand(XmlNode changedNode, string oldValue, string newValue) {
            this.changedNode = changedNode;
            this.oldValue = oldValue;
            this.newValue = newValue;
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