using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CodeTranslator.Tags;
using CodeTranslator.Attributes;
using CodeTranslator.Options;

namespace CodeTranslator.Nodes
{
    /// <summary>
    /// Represents an Html syntax node
    /// </summary>
    public class HtmlSyntaxNode : SyntaxNode
    {
        /// <summary>
        /// The tag option which is used to convert the tag. It is based on the representation.
        /// </summary>
        public HtmlOption TagOption { get; private set; }
        /// <summary>
        /// Creates an empty html syntax node. 
        /// </summary>
        /// <param name="tag">The tag that the node represents. If the tag is null - this is the main node.</param>
        /// <param name="tagRepresentation">The way that the tag is represented (&lt;a&gt; can be represented &lt;a href="..."&gt;)</param>
        public HtmlSyntaxNode(HtmlTag tag, string tagRepresentation) : base(tag, tagRepresentation) { }

        /// <summary>
        /// Creates an html syntax node with children
        /// </summary>
        /// <param name="tag">The tag that the node represents.</param>
        /// <param name="tagRepresentation">The way that the tag is represented (&lt;a&gt; can be represented &lt;a href="..."&gt;)</param>
        /// <param name="children">The children</param>        
        public HtmlSyntaxNode(HtmlTag tag, string tagRepresentation, List<SyntaxNode> children) : base(tag, tagRepresentation, children) { }

        /// <summary>
        /// Can't convert to Html, because it is in html...
        /// </summary>
        /// <returns></returns>
        public override string ToHtml()
        {
            //I can write just a normal return, but maybe later.
            throw new NotSupportedException("Html syntax nodes are already represented as html.");
        }

        /// <summary>
        /// Converts the Html to BBCode
        /// </summary>
        /// <param name="isDecodeOutput">If the output should be decoded. The input (bbcode to html) is always encoded, but it depends on the control that is
        /// visualizing the bbcode -> sometimes it automatically encodes and it's more correct to give it decoded text!</param>
        public override string ToBBCode(bool isDecodeOutput)
        {
            if (Tag == null) //this is the root node
            {
                return String.Concat((from c in Children
                                      select c.ToBBCode(isDecodeOutput)));//concatenate ToHtml of all children
            }
            return (Tag as HtmlTag).ToBBCodeString(Option, String.Concat((from c in Children
                                                           select c.ToBBCode(isDecodeOutput))), TagOption); 
        }

        /// <summary>
        /// Adds a child to the children collection
        /// </summary>
        /// <param name="child">The child</param>
        public override void AddChild(SyntaxNode child)
        {
            if (child is TextSyntaxNode)
            {
                //when the node is root (no newline replacer) or something is messed up, we have to use a default newline (</p><p>)
                child.NewlineRepresentation = string.IsNullOrEmpty(NewlineRepresentation) ? Constants.Constants.CloseHtmlParagraph + Constants.Constants.OpenHtmlParagraph : NewlineRepresentation;
                //This means that we can also work with other text nodes here, but not when outputing, but we will also have to encode it, etc. and it's better to leave this job to
                //the TextSyntaxNode. The only exclusion is here, because of the parser specifics.
                (child as TextSyntaxNode).Text = (child as TextSyntaxNode).Text.Replace(Constants.Constants.CloseHtmlParagraph + Constants.Constants.OpenHtmlParagraph, "\n")
                    .Replace(Constants.Constants.OpenHtmlParagraph, "")
                    .Replace(Constants.Constants.CloseHtmlParagraph, "");
            }
            Children.Add(child);
        }

        /// <summary>
        /// Initializes the html syntax node
        /// </summary>
        /// <param name="tagRepresentation">The representation in the text</param>
        protected override void Initialize(string tagRepresentation)
        {
            if (Tag != null && string.IsNullOrEmpty(tagRepresentation)) //if the tag is not null, we need a representation. This is to make sure I can create only root nodes like this
            {
                throw new ArgumentNullException("tagRepresentation");
            }                
            if (Tag != null)
            {
                GetTagOptionFromRepresentation(Tag, tagRepresentation);
            }
        }

        /// <summary>
        /// Gets the value of the option of the tag. Also, if the tag content is encoded in some of its attributes,
        /// adds the content as a child.
        /// </summary>
        /// <param name="tag">The tag.</param>
        /// <param name="tagRepresentation">The representation of the tag.</param>
        protected override void GetTagOptionFromRepresentation(Tag tag, string tagRepresentation)
        {
            TagOption = (tag as HtmlTag).GetOptionFromRepresentation(tagRepresentation); //get the option that best describes the tag
            if (TagOption == null) return; //this is when it's a closing tag...
            if (TagOption.OptionHtmlAttribute != null)
            {
                Option = HtmlAttribute.GetAttributeValue(tagRepresentation, TagOption.OptionHtmlAttribute);
            }
            if (TagOption.ContentHtmlAttribute != null)
            {
                AddChild(new TextSyntaxNode(HtmlAttribute.GetAttributeValue(tagRepresentation, TagOption.ContentHtmlAttribute)));
            }
        }
    }
}
