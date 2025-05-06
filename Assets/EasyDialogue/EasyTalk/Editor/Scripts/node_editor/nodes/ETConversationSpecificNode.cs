using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EasyTalk.Editor.Nodes
{
    public interface ETConversationSpecificNode
    {
        void SetConvoIDs(List<string> convoIDs);
    }
}
