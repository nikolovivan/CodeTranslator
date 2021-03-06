﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CodeTranslator.Tags;
using CodeTranslator.Constants;

namespace CodeTranslator.Nodes
{
    /// <summary>
    /// Represents a BBCode syntax node
    /// </summary>
    public class BBCodeSyntaxNode : SyntaxNode
    {
        /// <summary>
        /// If the content should be parsed.
        /// </summary>
        public bool IsParseContent { get; private set; }
        /// <summary>
        /// Creates an empty bbcode syntax node.
        /// </summary>
        /// <param name="tag">The tag that the node represents. If the tag is null - this is the main node.</param>
        /// <param name="tagRepresentation">The way that the tag is represented ([size] can be represented [size=100])</param>
        public BBCodeSyntaxNode(BBTag tag, string tagRepresentation) : base(tag, tagRepresentation) { }
        /// <summary>
        /// Creates a bbcode syntax node with children
        /// </summary>
        /// <param name="tag">The tag that the node represents.</param>
        /// <param name="tagRepresentation">The way that the tag is represented ([size] can be represented [size=100])</param>
        /// <param name="children">The children</param>        
        public BBCodeSyntaxNode(BBTag tag, string tagRepresentation, List<SyntaxNode> children) : base(tag, tagRepresentation, children) { }
        /// <summary>
        /// Converts the syntax node to Html
        /// </summary>
        public override string ToHtml()
        {
            if (Tag == null) //this is the root node
            {
                return String.Concat((from c in Children
                                      select c.ToHtml()));//concatenate ToHtml of all children
            }
            return (Tag as BBTag).ToHtmlString(Option, String.Concat((from c in Children
                                                           select c.ToHtml()))); 
        }

        public override string ToBBCode(bool isDecodeOutput)
        {
            throw new NotSupportedException("BBCode syntax nodes are already represented as bbcode!");
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
            }
            Children.Add(child);
        }

        /// <summary>
        /// Gets the option from the representation of the tag representation.
        /// </summary>
        /// <param name="tag">The defined bbtag</param>
        /// <param name="representation">The way it was represented in the text (with [])</param>
        protected override void GetTagOptionFromRepresentation(Tag tag, string representation)
        {
            Option = (tag as BBTag).GetOptionValue(representation);
        }

        /// <summary>
        /// Initializes the bbcode syntax node
        /// </summary>
        /// <param name="tag">The bbtag</param>
        /// <param name="tagRepresentation">The representation in the text</param>
        protected override void Initialize(string tagRepresentation)
        {
            if (Tag != null && string.IsNullOrEmpty(tagRepresentation)) //if the tag is not null, we need a representation. This is to make sure I can create only root nodes like this
            {
                throw new ArgumentNullException("tagRepresentation");
            }
            IsParseContent = true; //set the default value. If the tag is not null, it will take it again.
            if (Tag != null)
            {
                GetTagOptionFromRepresentation(Tag, tagRepresentation);
                IsParseContent = (Tag as BBTag).IsParseContent(tagRepresentation);
            }
        }

        /// <summary>
        /// Returns true if the node needs only text to close itself.
        /// </summary>
        /// <returns></returns>
        public bool IsSelfCloserNode()
        {
            return Tag == null ? false : !(Tag as BBTag).IsBBCodeNeedClosingTag;
        }
    }
}
