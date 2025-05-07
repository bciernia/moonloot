namespace EasyTalk.Localization
{
    /// <summary>
    /// An enum defining translation evaluation modes which determine when translations are performed relative to the timing of variable evaluation/injection.
    /// </summary>
    public enum TranslationEvaluationMode
    {
        TRANSLATE_BEFORE_VARIABLE_EVALUATION,
        TRANSLATE_AFTER_VARIABLE_EVALUATION
    }
}
