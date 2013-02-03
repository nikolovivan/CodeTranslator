using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CodeTranslator.Options;

namespace CodeTranslator.Tags
{
    /// <summary>
    /// Represents a base abstract class to BBCode tags and Html Tags
    /// </summary>
    public abstract class Tag
    {
        /// <summary>
        /// The open tag. Just the name, without the &lt;&gt; or []
        /// </summary>
        public string OpenTag { get; private set; }
        /// <summary>
        /// The close tag. Just the /name, without the &lt;/&gt; or [/]
        /// </summary>
        public string CloseTag { get; private set; }
        /// <summary>
        /// The options for the current tag.
        /// </summary>
        public TagOption[] Options { get; private set; }

        /// <summary>
        /// Creates a tag definition.
        /// </summary>
        /// <param name="openTag">The open tag.</param>
        /// <param name="closeTag">The close tag.</param>
        /// <param name="tagOptions">The options for the tag.</param>
        public Tag(string openTag, string closeTag, params TagOption[] tagOptions)
        {
            if (string.IsNullOrEmpty(openTag.Trim())) throw new ArgumentNullException("openTag");
            if (tagOptions == null || tagOptions.Length == 0) throw new ArgumentNullException("tagOptions");
            ValidateOptions(tagOptions);
            OpenTag = openTag.Trim();
            CloseTag = closeTag.Trim();
            Options = tagOptions;
        }

        /// <summary>
        /// Validates the tag options.
        /// </summary>
        /// <param name="tagOptions">The tag options.</param>
        protected abstract void ValidateOptions(TagOption[] tagOptions);
        /// <summary>
        /// Returns true if the value written in the parameter is a valid tag
        /// </summary>
        /// <param name="tagValue">The tag value</param>
        public abstract bool IsValid(string tagValue);
        /// <summary>
        /// Returns the opposite representation - HTML for BBCode and vice versa.
        /// </summary>
        public abstract string ToOppositeRepresentation();
    }
}
