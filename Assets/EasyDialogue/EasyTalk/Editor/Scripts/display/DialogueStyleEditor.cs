using EasyTalk.Display.Style;
using UnityEditor;

namespace EasyTalk.Editor.Display
{
    [CustomEditor(typeof(DialogueStyle))]
    public class DialogueStyleEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
        }
    }
}