using EasyTalk.Editor.Ledger.Actions;
using System;
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace EasyTalk.Editor.Nodes
{
    public abstract class ETListNode : ETNode
    {
        protected AbstractListNodeContent listPanel;

        public ETListNode(string title, string nodeClass) : base(title, nodeClass) { }

        public ETListNode() : base("LIST", "list-node") { }

        protected override void Initialize()
        {
            base.Initialize();

            this.name = "List Node";
            this.SetDimensions(110, 75);

            ETNodeButton addItemButton = new ETNodeButton();
            addItemButton.ClearClassList();
            addItemButton.AddToClassList("add-item-button");
            this.Add(addItemButton);

            addItemButton.onButtonClicked += OnAddItemButtonClicked;
        }

        public List<ETNodeContent> Items { get { return listPanel.Items; } }

        protected virtual void OnAddItemButtonClicked()
        {
            AddItemToList();
        }

        protected virtual void AddItemToList()
        {
            ETNodeContent newItem = CreateNewItem();
            if (newItem != null)
            {
                listPanel.AddItem(newItem);
            }
        }

        protected abstract ETNodeContent CreateNewItem();

        protected void CreateListPanel(ETListNodeType listNodeType = ETListNodeType.PANEL)
        {
            if (listPanel == null)
            {
                if (listNodeType == ETListNodeType.PANEL)
                {
                    listPanel = new ETListNodeContent(this);
                }
                else
                {
                     listPanel = new ETScrollableListNodeContent(this);
                }
                AddContent(listPanel);
            }
        }

        protected void RemoveItemFromList(ETNodeContent item)
        {
            listPanel.RemoveItem(item);
        }

        public AbstractListNodeContent ListPanel { get { return listPanel; } }
    }


    public class ETListNodeContent : AbstractListNodeContent
    {
        public ETListNodeContent(ETNode parentNode) : base()
        {
            this.parentNode = parentNode;
        }

        protected override void AddItemToPanel(ETNode node, ETNodeContent content)
        {
            contentPanel.Add(content);
        }

        protected override void InsertItemIntoPanel(ETNode node, int idx, ETNodeContent content)
        {
            contentPanel.Insert(idx, content);
        }

        protected override void RemoveItemFromPanel(ETNode node, ETNodeContent content)
        {
            contentPanel.Remove(content);
        }
    }

    public class ETScrollableListNodeContent : AbstractListNodeContent
    {
        private ScrollView scrollView;

        public ETScrollableListNodeContent(ETNode parentNode) : base()
        {
            this.parentNode = parentNode;
        }

        protected override void CreateContent(VisualElement contentContainer)
        {
            base.CreateContent(contentContainer);
            scrollView = new ScrollView();
            scrollView.style.flexGrow = 1;
            scrollView.style.paddingTop = 4;
            scrollView.style.paddingBottom = 4;
            scrollView.style.paddingLeft = 4;
            scrollView.style.paddingRight = 4;
            contentPanel.Add(scrollView);
        }

        protected override void AddItemToPanel(ETNode node, ETNodeContent content)
        {
            scrollView.Add(content);
        }

        protected override void InsertItemIntoPanel(ETNode node, int idx, ETNodeContent content)
        {
            scrollView.Insert(idx, content);
        }

        protected override void RemoveItemFromPanel(ETNode node, ETNodeContent content)
        {
            scrollView.Remove(content);
        }
    }

    public abstract class AbstractListNodeContent : ETNodeContent
    {
        protected ETNode parentNode;

        public List<ETNodeContent> listItems = new List<ETNodeContent>();

        public delegate void OnItemAdded(ETNode node, ETNodeContent content);

        public delegate void OnItemInserted(ETNode node, int idx, ETNodeContent content);

        public delegate void OnItemRemoved(ETNode node, ETNodeContent content);

        public OnItemAdded onItemAdded;

        public OnItemInserted onItemInserted;

        public OnItemRemoved onItemRemoved;

        public void AddItem(ETNodeContent content, bool pushToUndoStack = true)
        {
            AddItemToPanel(parentNode, content);
            listItems.Add(content);

            if (pushToUndoStack)
            {
                EasyTalkNodeEditor.Instance.Ledger.AddAction(new NodeItemAddedAction(parentNode, content));
            }

            if (onItemAdded != null)
            {
                onItemAdded(parentNode, content);
            }
        }

        protected abstract void AddItemToPanel(ETNode node, ETNodeContent content);

        public void InsertItem(int index, ETNodeContent content, bool pushToUndoStack = true)
        {
            InsertItemIntoPanel(parentNode, index, content);
            listItems.Insert(index, content);

            if (pushToUndoStack)
            {
                EasyTalkNodeEditor.Instance.Ledger.AddAction(new NodeItemAddedAction(parentNode, content));
            }

            if (onItemAdded != null)
            {
                onItemAdded(parentNode, content);
            }
        }

        protected abstract void InsertItemIntoPanel(ETNode node, int idx, ETNodeContent content);

        public void RemoveItem(ETNodeContent content, bool pushToUndoStack = true)
        {
            RemoveItemFromPanel(parentNode, content);
            int itemIdx = listItems.IndexOf(content);
            listItems.Remove(content);

            if (pushToUndoStack)
            {
                EasyTalkNodeEditor.Instance.Ledger.AddAction(new NodeItemRemovedAction(parentNode, content, itemIdx));
            }

            if (onItemRemoved != null)
            {
                onItemRemoved(parentNode, content);
            }
        }

        protected abstract void RemoveItemFromPanel(ETNode node, ETNodeContent content);

        public void RemoveItem(int idx)
        {
            ETNodeContent content = Items[idx];
            RemoveItem(content);
        }

        public override List<ETInput> GetInputs()
        {
            List<ETInput> inputs = new List<ETInput>();
            foreach (ETNodeContent content in Items)
            {
                inputs.AddRange(content.GetInputs());
            }
            return inputs;
        }

        public override List<ETOutput> GetOutputs()
        {
            List<ETOutput> outputs = new List<ETOutput>();
            foreach (ETNodeContent content in Items)
            {
                outputs.AddRange(content.GetOutputs());
            }
            return outputs;
        }

        public List<ETNodeContent> Items { get { return this.listItems; } }
    }

    public enum ETListNodeType
    {
        PANEL, SCROLL
    }
}