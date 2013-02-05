using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CodeTranslator.Options;
using CodeTranslator.Exceptions;

namespace CodeTranslator.Tags
{
    public class BBTag : Tag
    {
        /// <summary>
        /// This is used for tags like 'img' where the content is given as the src attribute.
        /// </summary>
        public const string ContentPlaceholder = "{content}";
        /// <summary>
        /// This is the placeholder for the option of a bbcode. It is used in 'size' for example, where the bbcode tag looks like
        /// [size=something] and the value of 'something' has to be written to a certain attribute in html...
        /// </summary>
        public const string OptionPlaceholder = "{option}";
        /// <summary>
        /// Shows if a bbcode tag has a version that needs on option.
        /// </summary>
        public bool HasOption { get { return Options.FirstOrDefault(o => (o as BBCodeOption).IsNeedOption) != null; } }
        /// <summary>
        /// Shows if the current bbcode tag accepts only options and doesn't accept tags without options.
        /// </summary>
        public bool IsOnlyOption { get { return Options.FirstOrDefault(o => !(o as BBCodeOption).IsNeedOption) == null; } }
        /// <summary>
        /// Does the bbcode element need a closing tag. The tag [*] doesn't!
        /// </summary>
        public bool IsBBCodeNeedClosingTag { get { return !string.IsNullOrEmpty(CloseTag); } }

        /// <summary>
        /// Creates a definition of a bbcode tag and how it is converted to html or back.
        /// </summary>
        /// <param name="openBBCodeTag">The open bbcode tag (without [])</param>
        /// <param name="closeBBCodeTag">The close bbcode tag (without [], but with /)</param>
        /// <param name="optionValues">The different option values.</param>
        public BBTag(string openBBCodeTag, string closeBBCodeTag, params TagOption[] optionValues)
            : base(openBBCodeTag, closeBBCodeTag, optionValues)
        {
 
        }        

        /// <summary>
        /// Validates the tag options
        /// </summary>
        /// <param name="tagOptions">Tag options.</param>
        protected override void ValidateOptions(TagOption[] tagOptions)
        {

            if (tagOptions.Distinct().Count() != tagOptions.Length) throw new ArgumentException("All option values must be different!");
            if (tagOptions.FirstOrDefault(o => !(o is BBCodeOption)) != null) throw new ArgumentException("All option values must be of type BBCodeOption!");
            if (tagOptions.FirstOrDefault(o => (o as BBCodeOption).IsCoverAllOptions) != null && tagOptions.FirstOrDefault(o => (o as BBCodeOption).IsCoverSpecificOption) != null) throw new ArgumentException("One of your rules covers some of the others!");
        }

        /// <summary>
        /// Converts the bbtag to html.
        /// </summary>
        /// <param name="option">The option value, if there is such.</param>
        /// <param name="content">The contents (the contents are always html encoded because they come from TextSyntaxNodes)</param>        
        public string ToHtmlString(string option, string content)
        {
            if (!HasOption && !string.IsNullOrEmpty(option)) //no rules for options, but such an option in the parameters...
            {
                throw new Exception("The tag '" + OpenTag + "' doesn't have an option, but you have provided a value for it!");
            }
            if (IsOnlyOption && string.IsNullOrEmpty(option))
            {
                throw new Exception("The tag '" + OpenTag + "' needs an option and it's missing!");
            }
            TagOption ov = string.IsNullOrEmpty(option) ? Options.FirstOrDefault(o => !(o as BBCodeOption).IsNeedOption) : GetOptionValuesForOption(option);
            if (ov == null) throw new ArgumentException("There was no " + (string.IsNullOrEmpty(option) ? "universal definition" : "definition option '" + option + "'") + " for the tag '" + OpenTag + "'.");
            string result = ((ov as BBCodeOption).IsInParagraph ? "" : Constants.Constants.CloseHtmlParagraph) + ov.OpenTag; //initialize
            //content will be set in both the open tag (if any) and the content
            if (ov.OpenTag.Contains(ContentPlaceholder))
            {
                result = result.Replace(ContentPlaceholder, content);
            }
            if (ov.OpenTag.Contains(OptionPlaceholder))
            {
                result = result.Replace(OptionPlaceholder, option);
            }
            return result + content + (string.IsNullOrEmpty(ov.CloseTag) ? "" : ov.CloseTag) + ((ov as BBCodeOption).IsInParagraph ? "" : Constants.Constants.OpenHtmlParagraph);
        }

        /// <summary>
        /// Gets the option values object for the specified option value.
        /// </summary>
        /// <param name="option">The option value.</param>
        /// <returns>If there is a universal option, it is returned. Else the option that covers the specific value is returned.</returns>
        private TagOption GetOptionValuesForOption(string option)
        {
            if (string.IsNullOrEmpty(option)) throw new ArgumentNullException("option");
            return Options.FirstOrDefault(o => (o as BBCodeOption).IsCoverAllOptions || (o as BBCodeOption).Value == option);
        }

        /// <summary>
        /// Returns the correct newline replacer by the given tag value. If it's a closing tag, 
        /// we can't say anything.
        /// </summary>
        /// <param name="tagValue">The value of the tag</param>        
        public string GetNewlineRepresentationByTagValue(string tagValue)
        {
            string value = GetAndValidateTagOptionValue(tagValue);
            if (value == null && tagValue.StartsWith("[/")) return null; //if it's a closing tag, we can't say anything.            
            BBCodeOption ov = Options.FirstOrDefault(o => (o as BBCodeOption).IsTagValueValid(value)) as BBCodeOption;//if the value is null or a specific string
            return ov == null ? null : ov.NewlineReplacer;
        }

        /// <summary>
        /// Returns the value of the tag option.
        /// </summary>
        /// <param name="tagValue">The value of the tag</param>
        public string GetOptionValue(string tagValue)
        {
            string value = GetAndValidateTagOptionValue(tagValue);
            if (value == null) return null; //this can happen if we don't have a value or the tag is closing
            if (!HasOption) throw new BBCodeParsingException("The tag '" + OpenTag + "' doesn't have an option, but such was found!");
            return Options.FirstOrDefault(o => (o as BBCodeOption).IsTagValueValid(value)) != null ? value : null;
        }

        /// <summary>
        /// Returns true if the content has to be parsed. Finds out based on the tag value.
        /// </summary>
        /// <param name="tagValue"></param>
        /// <returns></returns>
        public bool IsParseContent(string tagValue)
        {
            string value = GetAndValidateTagOptionValue(tagValue);
            if (value == null && tagValue.StartsWith("[/")) return true; //the default behaviour is to parse the content.
            BBCodeOption ov = Options.FirstOrDefault(o => (o as BBCodeOption).IsTagValueValid(value)) as BBCodeOption;
            return ov == null ? true : ov.IsParseContent;
        }

        /// <summary>
        /// Gets the tag option value (only for opening tags). If the tag doesn't have value, returns null.
        /// If it's a closing tag - null.
        /// Also validates the tag.
        /// </summary>
        /// <param name="tagValue">The value of the tag.</param>
        private string GetAndValidateTagOptionValue(string tagValue)
        {
            if (string.IsNullOrEmpty(tagValue)) throw new ArgumentNullException("tagValue");
            tagValue = tagValue.Trim('[', ']'); //trim the []
            string tag;
            string value;
            if (tagValue.Contains('=')) //tag with option
            {
                int valueStart = tagValue.IndexOf('=');
                if (valueStart == tagValue.Length - 1) throw new BBCodeParsingException("Wrong tag formatting!"); //this is b= (before trimming [b=])
                tag = tagValue.Substring(0, valueStart);
                value = tagValue.Substring(valueStart + 1);
            }
            else
            {
                tag = tagValue;
                value = null;
            }
            if (tag != OpenTag && tag != CloseTag) throw new BBCodeParsingException("The tag value is not for this type of tag!");
            if (tag.StartsWith("/")) return null; //this is the closing tag
            return value;
        }

        /// <summary>
        /// Returns true if the value written in the parameter is a valid tag
        /// </summary>
        /// <param name="tagValue">The tag value</param>        
        public override bool IsValid(string tagValue)
        {
            if (string.IsNullOrEmpty(tagValue)) throw new ArgumentNullException("tagValue");
            tagValue = tagValue.Trim('[', ']'); //trim the open and close tags.
            if (OpenTag == tagValue || CloseTag == tagValue) return true; //if it's a tag without a parameter
            if (!tagValue.Contains('=') || tagValue.StartsWith("/")) return false; //no '=' sign - a tag without a parameter. '/' - unknown end tag.
            int startValue = tagValue.IndexOf('=');
            string tag = tagValue.Substring(0, startValue); //the tag
            if (tag != OpenTag) return false; //I compare only with the open tag here.
            string value = tagValue.Substring(startValue);
            if (value.Length <= 1) return false; //this is something like [b=] in the original value, which is not valid.
            value = value.Substring(1); //get the real value
            return Options.FirstOrDefault(o => (o as BBCodeOption).IsTagValueValid(value)) != null;
        }
    }
}
