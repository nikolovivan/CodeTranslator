using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CodeTranslator.Options
{
    /// <summary>
    /// This class represents the connection between a value of an option and the Html that is generated
    /// when an option has such a value.
    /// </summary>
    public class BBCodeOption : TagOption
    {
        /// <summary>
        /// The value of the option. Three possibilities:
        /// 1. Null - no option
        /// 2. Empty string - all possibilities of the option
        /// 3. Specific value.
        /// </summary>
        public string Value { get; private set; }
        /// <summary>
        /// The string which is used to replace the new lines.
        /// </summary>
        public string NewlineReplacer { get; private set; }
        /// <summary>
        /// Shows if the option generates an Html tag, that must not be in a paragraph.
        /// </summary>
        public bool IsNotInParagraph { get; private set; }
        /// <summary>
        /// Should the content be parsed.
        /// </summary>
        public bool IsParseContent { get; private set; }
        /// <summary>
        /// Tells if the current object needs an option.
        /// </summary>
        public bool IsNeedOption { get { return Value != null; } }
        /// <summary>
        /// Tells if the current object covers all options.
        /// </summary>
        public bool IsCoverAllOptions { get { return Value == ""; } }
        /// <summary>
        /// Tells if the current object covers a specific option.
        /// </summary>
        public bool IsCoverSpecificOption { get { return !string.IsNullOrEmpty(Value); } }

        /// <summary>
        /// Creates a description object for the required html for a specific option value.
        /// </summary>
        /// <param name="value">The value - null for no option; empty string for all values; specific value</param>
        /// <param name="openHtmlTag">The open html tag (with &lt;...&gt;)</param>
        /// <param name="closeHtmlTag">The close html tag (with &lt;/...&gt;)</param>
        /// <param name="newlineReplacer">The string which is used to replace the new lines.</param>
        /// <param name="isParseContent">Should the content be parsed.</param>
        public BBCodeOption(string value, string openHtmlTag, string closeHtmlTag, string newlineReplacer, bool isParseContent) : this(value, openHtmlTag, closeHtmlTag, newlineReplacer, isParseContent, false) { }

        /// <summary>
        /// Creates a description object for the required html for a specific option value.
        /// </summary>
        /// <param name="value">The value - null for no option; empty string for all values; specific value</param>
        /// <param name="openHtmlTag">The open html tag (with &lt;...&gt;)</param>
        /// <param name="closeHtmlTag">The close html tag (with &lt;/...&gt;)</param>
        /// <param name="newlineReplacer">The string which is used to replace the new lines.</param>
        /// <param name="isParseContent">Should the content be parsed.</param>
        /// <param name="isNotInParagraph">Shows if the option generates an Html tag, that must not be in a paragraph.</param>        
        public BBCodeOption(string value, string openHtmlTag, string closeHtmlTag, string newlineReplacer, bool isParseContent, bool isNotInParagraph)
            : base(openHtmlTag, closeHtmlTag)
        {
            Value = value;
            NewlineReplacer = string.IsNullOrEmpty(newlineReplacer) ? "\n" : newlineReplacer; //use the \n in order to make things work correctly.
            IsParseContent = isParseContent;
            IsNotInParagraph = isNotInParagraph;
        }

        public override bool Equals(object obj)
        {
            BBCodeOption ov = obj as BBCodeOption;
            if (ov == null) return false; //different types.
            return Value == ov.Value; //only the value of the option counts here!
        }

        public override int GetHashCode()
        {
            return Value == null ? -1 : Value.GetHashCode();
        }

        /// <summary>
        /// Returns true if a value is valid according to the object settings.
        /// This works only for opening tags!
        /// </summary>        
        /// <param name="value">The value of the tag</param>        
        public bool IsTagValueValid(string value)
        {           
            if (string.IsNullOrEmpty(value) && Value == null) return true; //no options.
            if (!string.IsNullOrEmpty(value) && IsCoverAllOptions) return true; //we have a value and it's covered
            return value == Value && IsCoverSpecificOption; //if it covers a specific option.
        }
    }
}
