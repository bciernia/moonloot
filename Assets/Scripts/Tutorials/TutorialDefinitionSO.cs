using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public sealed class TutorialPageDefinition
{
    [TextArea(3, 10)] public string Description;
    public Sprite Illustration;
    public Sprite[] AnimationFrames;
    [Min(1f)] public float FramesPerSecond = 6f;
    public bool LoopAnimation = true;
}

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

    [Header("Pages (optional)")]
    [SerializeField] private List<TutorialPageDefinition> pages = new();

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
    public IReadOnlyList<TutorialPageDefinition> Pages => pages;

    public void ConfigureRuntime(
        string id,
        string tutorialTitle,
        string tutorialInstructions,
        string tutorialActionHint,
        bool showOnce = true,
        List<TutorialPageDefinition> tutorialPages = null)
    {
        tutorialId = id;
        title = tutorialTitle;
        instructions = tutorialInstructions;
        actionHint = tutorialActionHint;
        showOncePerSave = showOnce;
        condition = null;
        illustration = null;
        animationFrames = null;
        pages = tutorialPages ?? new List<TutorialPageDefinition>();
    }
}
