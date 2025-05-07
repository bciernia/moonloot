using EasyTalk.Editor.Ledger.Actions;
using EasyTalk.Editor.Utils;
using UnityEngine.UIElements;

namespace EasyTalk.Editor.Components
{
    public class ETDropdownField : DropdownField
    {
        public bool PublishUndoableActions { get; set; } = true;

        public ETDropdownField() : base() 
        { 
            this.RegisterCallback<ChangeEvent<string>>(OnChoiceChanged);

            this.style.unityFont = EasyTalkNodeEditor.Instance.EditorSettings.GetCurrentFont();
            this.style.unityFontDefinition = new StyleFontDefinition(EasyTalkNodeEditor.Instance.EditorSettings.GetCurrentFont());
        }

        private void OnChoiceChanged(ChangeEvent<string> evt)
        {
            if (PublishUndoableActions)
            {
                EasyTalkNodeEditor.Instance.Ledger.AddAction(new NodeDropdownFieldChangedAction(ETUtils.FindNodeParent(this), this, evt.previousValue));
                EasyTalkNodeEditor.Instance.NodesChanged();
            }
        }
    }
}
