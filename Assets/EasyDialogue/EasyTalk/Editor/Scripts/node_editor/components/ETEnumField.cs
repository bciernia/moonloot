using EasyTalk.Editor.Ledger.Actions;
using EasyTalk.Editor.Utils;
using System;
using UnityEngine.UIElements;

namespace EasyTalk.Editor.Components
{
    public class ETEnumField : EnumField
    {
        public bool PublishUndoableActions { get; set; } = true;

        public ETEnumField(Enum defaultValue) : base(defaultValue) 
        {
            this.RegisterCallback<ChangeEvent<Enum>>(EnumValueChanged);
        }

        protected void EnumValueChanged(ChangeEvent<Enum> evt)
        {
            if (PublishUndoableActions)
            {
                EasyTalkNodeEditor.Instance.Ledger.AddAction(new NodeEnumFieldChangedAction(ETUtils.FindNodeParent(this), this, evt.previousValue));
            }
        }
    }
}
