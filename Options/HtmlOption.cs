using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CodeTranslator.Attributes;

namespace CodeTranslator.Options
{
    public class HtmlOption : TagOption
    {
        /// <summary>
        /// The attribute to use for the content. Can be the whole attribute, part of it or nothing (content is taken from the enclosing tags).
        /// </summary>
        public HtmlAttribute ContentHtmlAttribute { get; private set; }
        /// <summary>
        /// The attribute to use for the option. Can be the whole attribute, part of it or nothing (we don't use options here).
        /// </summary>
        public HtmlAttribute OptionHtmlAttribute { get; private set; }
        /// <summary>
        /// Creates a new option describing how to present a certain tag depending on html attributes.
        /// </summary>
        /// <param name="openBBCodeTag">The open bbcode tag. No []</param>
        /// <param name="closeBBCodeTag">The close bbcode tag. No [], but something like /b</param>
        /// <param name="contentHtmlAttribute">The html attribute that gets the content.</param>
        /// <param name="optionHtmlAttribute">The html attribute that gets the option.</param>
        public HtmlOption(string openBBCodeTag, string closeBBCodeTag, HtmlAttribute contentHtmlAttribute, HtmlAttribute optionHtmlAttribute)
            : base(openBBCodeTag, closeBBCodeTag)
        {
            //we can have bbcode without closing tags, though...
            //there is no need in attribute validations. See the commented IsAttributesValid function and see why.
            ContentHtmlAttribute = contentHtmlAttribute;
            OptionHtmlAttribute = optionHtmlAttribute;
        }

        public override bool Equals(object obj)
        {
            HtmlOption opt = obj as HtmlOption;
            if (opt == null) return false;
            return this.GetHashCode() == opt.GetHashCode();
        }

        public override int GetHashCode()
        {
            //the different attributes are with the same weights. This will prevent (A B != B A), because we don't want that!
            return (ContentHtmlAttribute == null ? -1 : ContentHtmlAttribute.GetHashCode()) ^ (OptionHtmlAttribute == null ? -1 : OptionHtmlAttribute.GetHashCode());
        }

        ///// <summary>
        ///// Tries to validate the attributes list.
        ///// </summary>         
        //private bool IsAttributesValid(HtmlAttribute contentHtmlAttribute, HtmlAttribute optionHtmlAttribute)
        //{
        //    if (contentHtmlAttribute == null && optionHtmlAttribute == null) return true; //there are bbcode tags where we don't get anything from html attributes.
        //    if (contentHtmlAttribute != null && optionHtmlAttribute == null) return true; //only one attribute is set
        //    if (contentHtmlAttribute == null && optionHtmlAttribute != null) return true; //the other one attribute is set
        //    //both attributes are set.
        //    if (contentHtmlAttribute.Name != optionHtmlAttribute.Name) return true; //content and option are taken from different html attributes.
        //    //both values are taken from the same attribute.
        //}
    }
}
