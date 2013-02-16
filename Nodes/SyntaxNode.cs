using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CodeTranslator.Tags;

namespace CodeTranslator.Nodes
{
    /// <summary>
    /// Represents a single node in the syntax. This is a base class for both - BBCode nodes and HTML nodes
    /// </summary>
    public abstract class SyntaxNode
    {
        /// <summary>
        /// The html tag represented by the node.
        /// </summary>
        public Tag Tag { get; protected set; }
        /// <summary>
        /// The string which is used to replace the newline.
        /// </summary>
        public string NewlineRepresentation { get; set; }
        /// <summary>
        /// The list of the children nodes.
        /// </summary>
        public List<SyntaxNode> Children { get; protected set; }
        /// <summary>
        /// The option if the tag has one. The content of the node will be in its children in the form of other nodes.
        /// </summary>
        public string Option { get; protected set; }
        /// <summary>
        /// Creates an empty syntax node
        /// </summary>
        /// <param name="tag">The tag used...</param>
        /// <param name="tagRepresentation">The representation of the tag.</param>
        protected SyntaxNode(Tag tag, string tagRepresentation) : this(tag, tagRepresentation, new List<SyntaxNode>()) { }
        /// <summary>
        /// Creates a syntax node with children
        /// </summary>      
        /// <param name="tag">The tag used...</param>
        /// <param name="tagRepresentation">The representation of the tag.</param>  
        /// <param name="children">The children</param>
        protected SyntaxNode(Tag tag, string tagRepresentation, List<SyntaxNode> children)
        {
            if (children == null) throw new ArgumentNullException("children");
            this.Tag = tag;
            Children = children;
            NewlineRepresentation = tag == null ? null : tag.GetNewlineRepresentation(tagRepresentation);
            Initialize(tagRepresentation);
        }
        /// <summary>
        /// Converts the node to Html
        /// </summary>        
        public abstract string ToHtml();
        /// <summary>
        /// Converts the node to BBCode
        /// </summary>
        /// <param name="isDecodeOutput">If the output should be decoded. The input (bbcode to html) is always encoded, but it depends on the control that is
        /// visualizing the bbcode -> sometimes it automatically encodes and it's more correct to give it decoded text!</param>
        /// <returns></returns>
        public abstract string ToBBCode(bool isDecodeOutput);
        /// <summary>
        /// Adds a child to the children collection
        /// </summary>
        /// <param name="child">The child</param>
        public abstract void AddChild(SyntaxNode child);
        /// <summary>
        /// Returns the tag option from its string representation
        /// </summary>
        protected abstract void GetTagOptionFromRepresentation(Tag tag, string representation);
        /// <summary>
        /// Initializes the syntax node
        /// </summary>
        /// <param name="tagRepresentation">The representation in the text</param>
        protected abstract void Initialize(string tagRepresentation);

        public bool hasSameTags(SyntaxNode other)
        {
            if (other == null) return false;
            return this.Tag.Equals(other.Tag);
        }
    }
}
