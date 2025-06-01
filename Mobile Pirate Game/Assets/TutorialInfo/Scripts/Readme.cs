using System;
using UnityEngine;

/// <summary>
/// A ScriptableObject to store data for a readme or tutorial display.
/// </summary>
public class Readme : ScriptableObject
{
    [Tooltip("Icon to be displayed.")]
    public Texture2D icon;

    [Tooltip("Main title of the readme or tutorial.")]
    public string title;

    [Tooltip("Array of sections containing content.")]
    public Section[] sections;

    [Tooltip("Flag to indicate if a specific layout has been loaded.")]
    public bool loadedLayout;

    /// <summary>
    /// Represents a single section within the readme.
    /// </summary>
    [Serializable]
    public class Section
    {
        [Tooltip("Heading for this section.")]
        public string heading;

        [Tooltip("Main content text for this section.")]
        [TextArea(minLines: 3, maxLines: 10)]
        public string text;

        [Tooltip("Text for the hyperlink.")]
        public string linkText;

        [Tooltip("URL for the hyperlink.")]
        public string url;
    }
}
