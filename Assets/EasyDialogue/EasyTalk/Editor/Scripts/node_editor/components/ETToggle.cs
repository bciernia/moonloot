using EasyTalk.Editor.Ledger.Actions;
using EasyTalk.Editor.Utils;
using UnityEngine.UIElements;

namespace EasyTalk.Editor.Components
{
    public class ETToggle : Toggle
    {
        public bool PublishUndoableActions { get; set; } = true;

        public ETToggle() : base() 
        {
            RegisterCallback<ChangeEvent<bool>>(ToggleChanged);
        }

        public ETToggle(string label) : base(label) 
        {
            RegisterCallback<ChangeEvent<bool>>(ToggleChanged);
        }

        public void ToggleChanged(ChangeEvent<bool> evt)
        {
            if (PublishUndoableActions)
            {
                EasyTalkNodeEditor.Instance.Ledger.AddAction(new NodeToggleChangedAction(ETUtils.FindNodeParent(this), this, evt.previousValue));
                EasyTalkNodeEditor.Instance.NodesChanged();
            }
        }
    }
}
