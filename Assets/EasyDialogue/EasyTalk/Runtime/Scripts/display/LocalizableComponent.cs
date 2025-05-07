using EasyTalk.Localization;

namespace EasyTalk.Display
{
    /// <summary>
    /// This interface defines methods for components which contain localizable Text components.
    /// </summary>
    public interface LocalizableComponent
    {
        /// <summary>
        /// Gets or sets whether the component should override the default font sizes.
        /// </summary>
        public bool OverrideFontSizes { get; set; }

        /// <summary>
        /// Gets or sets the minimum font size to use on the component.
        /// </summary>
        public int MinFontSize { get; set; }

        /// <summary>
        /// Gets or sets the maximum font size to use on the component.
        /// </summary>
        public int MaxFontSize { get; set; }

        /// <summary>
        /// Gets or sets the LanguageFontOverrides to use on the component.
        /// </summary>
        public LanguageFontOverrides LanguageFontOverrides { get; set; }
    }
}
