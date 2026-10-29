using UnityEngine;

[CreateAssetMenu(fileName = "Tutorial", menuName = "Tutorials/Tutorial Definition")]
public sealed class TutorialDefinitionSO : ScriptableObject
{
    [Header("Identity")]
    [SerializeField] private string tutorialId;
    [SerializeField] private bool showOncePerSave = true;

    [Header("Content")]
    [SerializeField] private string title;
    [TextArea(3, 8)]
    [SerializeField] private string instructions;
    [TextArea(1, 3)]
    [SerializeField] private string actionHint;

    [Header("Visual")]
    [SerializeField] private Sprite illustration;
    [SerializeField] private Sprite[] animationFrames;
    [SerializeField, Min(1f)] private float framesPerSecond = 6f;
    [SerializeField] private bool loopAnimation = true;

    [Header("Optional condition")]
    [SerializeField] private TutorialConditionSO condition;

    public string Id => string.IsNullOrWhiteSpace(tutorialId) ? name : tutorialId.Trim();
    public bool ShowOncePerSave => showOncePerSave;
    public string Title => title;
    public string Instructions => instructions;
    public string ActionHint => actionHint;
    public Sprite Illustration => illustration;
    public Sprite[] AnimationFrames => animationFrames;
    public float FramesPerSecond => framesPerSecond;
    public bool LoopAnimation => loopAnimation;
    public TutorialConditionSO Condition => condition;
}
