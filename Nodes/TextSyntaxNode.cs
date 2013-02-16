using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using CodeTranslator.Tags;

namespace CodeTranslator.Nodes
{
    /// <summary>
    /// Represents a text syntax node. This is usually the free text or the text inside another syntax node.
    /// </summary>
    public class TextSyntaxNode : SyntaxNode
    {
        /// <summary>
        /// The text of the text syntax node.
        /// </summary>
        public string Text { get; set; }
        /// <summary>
        /// Creates a text syntax node.
        /// </summary>
        /// <param name="text">The text of the text syntax node.</param>
        public TextSyntaxNode(string text)
            : base(null, text)
        {
            if (string.IsNullOrEmpty(text)) throw new ArgumentNullException("text");
            Text = text;
        }
        /// <summary>
        /// Returns the text node value - it will be HtmlEncoded...
        /// </summary>
        public override string ToHtml()
        {
            //I first decode the text and then encode it just in case if it comes encoded, because double encoding results in faulty results...
            return HttpUtility.HtmlEncode(HttpUtility.HtmlDecode(Text)).Replace("\n", NewlineRepresentation).Replace("\r",""); //return the text encoded and with replaced newlines. Have to remove the /r also.
            //the text will be always encoded, because this is safer!!! => no parameters.
        }

        /// <summary>
        /// Returns the textnode value in BBCode representation
        /// </summary>
        /// <param name="isDecodeOutput">If the output should be decoded. The input (bbcode to html) is always encoded, but it depends on the control that is
        /// visualizing the bbcode -> sometimes it automatically encodes and it's more correct to give it decoded text!</param>
        public override string ToBBCode(bool isDecodeOutput)
        {
            if (string.IsNullOrEmpty(NewlineRepresentation)) throw new ArgumentNullException("NewlineRepresentation");//just to take care that the newlinerepresentation is always OK.            
            return isDecodeOutput ? HttpUtility.HtmlDecode(Text).Replace(NewlineRepresentation, "\n") : Text.Replace(NewlineRepresentation, "\n");
        }

        /// <summary>
        /// Adds a child to the children collection. In the text syntax node children are not allowed
        /// </summary>
        /// <param name="child">The child</param>
        public override void AddChild(SyntaxNode child)
        {
            throw new NotSupportedException("You can't add children to a text syntax node!"); //the text syntax node doesn't have children
        }

        protected override void GetTagOptionFromRepresentation(Tag tag, string representation)
        {
            throw new NotSupportedException("You can't call this method from a text syntax node.");
        }

        protected override void Initialize(string tagRepresentation)
        {
            //nothing to do here
        }
    }
}
