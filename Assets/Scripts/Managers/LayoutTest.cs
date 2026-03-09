using UnityEngine;
using UnityEngine.UI;

public class LayoutTest : MonoBehaviour
{
    public RectTransform content;

    void Start()
    {
        LayoutRebuilder.ForceRebuildLayoutImmediate(content);
    }
}