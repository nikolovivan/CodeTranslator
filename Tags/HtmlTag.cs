using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CodeTranslator.Options;
using CodeTranslator.Attributes;
using CodeTranslator.Exceptions;

namespace CodeTranslator.Tags
{
    /// <summary>
    /// A class that represents an Html tag
    /// </summary>
    public class HtmlTag : Tag
    {
        /// <summary>
        /// The string which is used to replace the new line in Html. It gets converted to \n in BBCode
        /// </summary>
        public string NewlineReplacer { get; private set; }
        /// <summary>
        /// Creates an html tag description with information how to convert to bbcode.
        /// </summary>
        /// <param name="openHtmlTag">The open tag, e. g. 'div' for &lt;div&gt;</param>
        /// <param name="closeHtmlTag">The close tag, e. g. '/div' for &lt;/div&gt;</param>
        /// <param name="newlineReplacer">The string that replaces the new line in Html. It's replaced to /n in BBCode.</param>
        /// <param name="optionValues">The options.</param>
        public HtmlTag(string openHtmlTag, string closeHtmlTag, string newlineReplacer, params TagOption[] optionValues)
            : base(openHtmlTag, closeHtmlTag, optionValues)
        {
            NewlineReplacer = newlineReplacer;
        }

        /// <summary>
        /// Validates the given options for converting the current html tag to bbcode.
        /// </summary>
        /// <param name="tagOptions">The different possible options</param>
        protected override void ValidateOptions(TagOption[] tagOptions)
        {
            if (tagOptions.Distinct().Count() != tagOptions.Length) throw new ArgumentException("All option values must be unique!");
            if (tagOptions.FirstOrDefault(o => !(o is HtmlOption)) != null) throw new ArgumentException("All option values must be of type HtmlOption!");
        }

        /// <summary>
        /// Returns true if the value written in the parameter is a valid tag. The result is produced for the whole tag.
        /// </summary>
        /// <param name="tagValue">The tag value</param>
        public override bool IsValid(string tagValue)
        {
            if (string.IsNullOrEmpty(tagValue)) throw new ArgumentNullException("tagValue");
            tagValue = tagValue.Trim('<', '>'); //trim the open and close brackets
            //get the tag name (until the first space character if attributes are present or the whole string.)
            string tagName = tagValue.Contains(' ') ? tagValue.Substring(0, tagValue.IndexOf(' ')) : tagValue;
            if (OpenTag != tagName)
            {
                if (CloseTag == tagName) return true; //the close tag is always valid
                return false; //something unknown...
            }
            //from this point we know that we have an opening tag and we have to check the options...
            HtmlAttribute[] usedAttributes = HtmlAttribute.GetAttributes(tagValue);
            if (usedAttributes == null || usedAttributes.Length == 0) //if the tag doesn't have attributes in the representation
            {
                // if we have an option with no attributes
                if (Options.FirstOrDefault(o => (o as HtmlOption).ContentHtmlAttribute == null && (o as HtmlOption).OptionHtmlAttribute == null) != null) return true;
                return false;//if we don't have such an option.
            }            
            //here we have to see only those options which have at least one attribute set.
            foreach (HtmlOption opt in Options.Where(o => (o as HtmlOption).ContentHtmlAttribute != null || (o as HtmlOption).OptionHtmlAttribute != null))
            {
                //cases:
                //1. Only content attribute, but it's not used in the representation
                //2. Only option attribute, but it's not used in the representation
                //3. Both attributes, but one of them is not used in the representation
                if (opt.ContentHtmlAttribute != null && !usedAttributes.Contains(opt.ContentHtmlAttribute) ||
                    opt.OptionHtmlAttribute != null && !usedAttributes.Contains(opt.OptionHtmlAttribute)) continue;
                //here I will have only options that satisfy the representation.
                return true;
            }
            return false;
        }

        /// <summary>
        /// Returns the most suitable HtmlOption, based on the tag representation
        /// </summary>
        /// <param name="tagRepresentation">The tag representation.</param>
        public HtmlOption GetOptionFromRepresentation(string tagRepresentation)
        {
            if (string.IsNullOrEmpty(tagRepresentation)) throw new ArgumentNullException("tagRepresentation");
            tagRepresentation = tagRepresentation.Trim('<','>');
            string tagName = tagRepresentation.Contains(' ') ? tagRepresentation.Substring(0, tagRepresentation.IndexOf(' ')) : tagRepresentation;
            if (tagName != OpenTag) return null; //we can get options only from open tags. If it's not open or something else, return null.
            HtmlAttribute[] attributes = HtmlAttribute.GetAttributes(tagRepresentation);
            if (attributes == null || attributes.Length == 0) //no attributes
            {
                return Options.FirstOrDefault(o => (o as HtmlOption).ContentHtmlAttribute == null && (o as HtmlOption).OptionHtmlAttribute == null) as HtmlOption; //return the option with no attributes
            }
            List<HtmlOption> validOptions = new List<HtmlOption>();
            foreach (HtmlOption opt in Options.Where(o => ((o as HtmlOption).ContentHtmlAttribute == null || attributes.Contains((o as HtmlOption).ContentHtmlAttribute)) &&
                ((o as HtmlOption).OptionHtmlAttribute == null || attributes.Contains((o as HtmlOption).OptionHtmlAttribute)))) //all the options which contain the used attributes. But we still don't know about specific attribute values.
            {
                if (opt.ContentHtmlAttribute == null && opt.OptionHtmlAttribute == null) continue; //ignore the no attribute option, because it has been checked before                
                if (opt.ContentHtmlAttribute != null && string.IsNullOrEmpty(HtmlAttribute.GetAttributeValue(tagRepresentation, opt.ContentHtmlAttribute))) continue; //we have attribute in the option, but can't get its value from the representation
                if (opt.OptionHtmlAttribute != null && string.IsNullOrEmpty(HtmlAttribute.GetAttributeValue(tagRepresentation, opt.OptionHtmlAttribute))) continue;
                validOptions.Add(opt);
            }
            //now we have only valid options, according to the tag representation.
            //Here are the priorities:
            //1. Both attributes - full
            //2. Both attributes - one of them full
            //3. Both attributes
            //4. One attribute - full
            //5. One attribute
            //In some cases this can make a problem if we have more attributes, but in the case I will use this library,
            //it will work fine. This is also up to settings, which sometimes can be pretty stupid if such a thing happens.
            HtmlOption[] priorityList = new HtmlOption[4]; //four elements. The lowest index is the highest priority. This is done to avoid multiple searching through the whole collection, when many options.
            foreach (HtmlOption opt in validOptions)
            {
                if (opt.ContentHtmlAttribute != null && opt.OptionHtmlAttribute != null)
                {
                    if (!opt.ContentHtmlAttribute.HasValue && !opt.OptionHtmlAttribute.HasValue) return opt;
                    if (!opt.ContentHtmlAttribute.HasValue || !opt.OptionHtmlAttribute.HasValue) //one has value and one is full
                    {
                        priorityList[0] = opt;
                    }
                    //both have values
                    priorityList[1] = opt;
                }
                //one attribute
                if (opt.ContentHtmlAttribute != null && !opt.ContentHtmlAttribute.HasValue || opt.OptionHtmlAttribute != null && !opt.OptionHtmlAttribute.HasValue)
                {
                    //one full.
                    priorityList[2] = opt;
                }
                //one attribute from value.
                priorityList[3] = opt;
            }
            //return the one with the highest priority
            int i = 0;
            while (i < priorityList.Length)
            {
                if (priorityList[i] != null) return priorityList[i];
                i++;
            }
            return null; //if nothing is found.
        }

        /// <summary>
        /// Converts the tag to BBCode according to the html option and
        /// the option and content strings
        /// </summary>
        /// <param name="option">The option. Represented as [size={option}] in bbcode</param>
        /// <param name="content">The content. Represented as [b]{content}[/b] in bbcode</param>
        /// <param name="tagOption">The tag option that has to be used to convert the tag.</param>        
        public string ToBBCodeString(string option, string content, HtmlOption tagOption)
        {
            if (tagOption == null) throw new ArgumentNullException("tagOption");
            if (tagOption.OptionHtmlAttribute != null && string.IsNullOrEmpty(option)) throw new ArgumentNullException("tagOption"); //have option but the value is null or empty
            if (!Options.Contains(tagOption)) throw new HtmlParsingException("Option not found in the current html tag representation!");
            return "[" + tagOption.OpenTag + (tagOption.OptionHtmlAttribute != null ? "=" + option : "") + "]" + content + (string.IsNullOrEmpty(tagOption.CloseTag) ? "" : "[" + tagOption.CloseTag + "]");            
        }
    }
}
