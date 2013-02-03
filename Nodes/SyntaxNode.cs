using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CodeTranslator.Nodes
{
    /// <summary>
    /// Represents a single node in the syntax. This is a base class for both - BBCode nodes and HTML nodes
    /// </summary>
    public abstract class SyntaxNode
    {
        /// <summary>
        /// The string which is used to replace the newline.
        /// </summary>
        public string NewlineRepresentation { get; set; } //the set has to be public in order to be able to use it in derived classes        
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
        /// <param name="newlineRepresentation">The string to replace the newline</param>
        protected SyntaxNode(string newlineRepresentation)
        {
            Children = new List<SyntaxNode>();
            NewlineRepresentation = newlineRepresentation;
        }
        /// <summary>
        /// Creates a syntax node with children
        /// </summary>       
        /// <param name="newlineRepresentation">The string to replace the newline</param>  
        /// <param name="children">The children</param>
        protected SyntaxNode(string newlineRepresentation, List<SyntaxNode> children)
        {
            if (children == null) throw new ArgumentNullException("children");
            Children = children;
            NewlineRepresentation = newlineRepresentation;
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
    }
}
