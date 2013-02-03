using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CodeTranslator.Nodes;
using CodeTranslator.Tags;
using System.Text.RegularExpressions;
using System.Diagnostics;
using CodeTranslator.Exceptions;

namespace CodeTranslator.Parsers
{
    /// <summary>
    /// A class that parses Html to BBCode
    /// </summary>
    public class HtmlParser : ParserBase
    {
        /// <summary>
        /// If the output should be decoded - depends on the control that will use the outputed BBCode
        /// </summary>
        public bool IsDecodeOutput { get; private set; }
        /// <summary>
        /// Creates a default HtmlParser that decodes the output when presenting in BBCode.
        /// </summary>
        /// <param name="tags"></param>
        public HtmlParser(params HtmlTag[] tags) : this(true, tags) { }
        /// <summary>
        /// Creates an HtmlParser object.
        /// </summary>
        /// <param name="isDecodeOutput">If the output should be decoded. The input (bbcode to html) is always encoded, but it depends on the control that is
        /// visualizing the bbcode -> sometimes it automatically encodes and it's more correct to give it decoded text!</param>
        /// <param name="tags">The supported tags</param>
        public HtmlParser(bool isDecodeOutput, params HtmlTag[] tags)
            : base(tags)
        {
            IsDecodeOutput = isDecodeOutput;
        }

        /// <summary>
        /// Converts the given html to bbcode
        /// </summary>
        /// <param name="html">The html</param>
        /// <returns>The BBCode equivalent of the html.</returns>
        public string ToBBCode(string html)
        {
            if (string.IsNullOrEmpty(html)) throw new ArgumentNullException("html");
            return ParseSyntaxNode(html).ToBBCode(IsDecodeOutput);
        }

        /// <summary>
        /// Parses Hhml to a Syntax Node
        /// </summary>
        /// <param name="code">The html</param>
        protected override SyntaxNode ParseSyntaxNode(string code)
        {
            if (string.IsNullOrEmpty(code)) throw new ArgumentNullException("code");
            Stack<SyntaxNode> syntaxStack = new Stack<SyntaxNode>(); //this is a stack that will hold the bbcode nodes.
            syntaxStack.Push(new HtmlSyntaxNode(null, null)); //push the main node.
            int i = 0;
            while (i < code.Length)
            {
                TagType tagType;
                HtmlSyntaxNode lastNode = syntaxStack.Peek() as HtmlSyntaxNode;
                if (lastNode == null) throw new HtmlParsingException("Bad node type in nodes stack.");
                if (syntaxStack.Count > 1 && lastNode.Tag == null) throw new HtmlParsingException("You can't have more than one root node!");
                SyntaxNode nextNode = GetNextNode(code, ref i, out tagType, lastNode.Tag, true); //always parse the content in html nodes.
                if (nextNode is TextSyntaxNode || tagType == TagType.SelfClose) //the text is always added as a child. If it's self closing - also add it as a child.
                {
                    lastNode.AddChild(nextNode);
                }
                else
                {
                    switch (tagType)
                    {
                        case TagType.Close:                            
                            if (lastNode.Tag != (nextNode as HtmlSyntaxNode).Tag) throw new HtmlParsingException("Invalid formatting!");
                            lastNode = syntaxStack.Pop() as HtmlSyntaxNode; //pop it out.
                            syntaxStack.Peek().AddChild(lastNode);//add it as a child to the one before it.
                            break;
                        case TagType.Open:
                            syntaxStack.Push(nextNode); //push it to the stack
                            break;
                        default: throw new HtmlParsingException("Strange behavior!");
                    }
                }
            }
            Debug.Assert(syntaxStack.Count == 1); //we must have only one element - the root and to unfold from it.
            return syntaxStack.Pop();
        }

        /// <summary>
        /// Gets the next node after the start index
        /// </summary>
        /// <param name="code">The code.</param>
        /// <param name="startIndex">The index from which to start searching.</param>
        /// <param name="tagType">Returns next tag type</param>
        /// <param name="previousTag">The previous tag in the stack.</param>
        /// <param name="isParseContent">If the content of the last opening node should be parsed. In Html all the content is parsed.</param>        
        protected override SyntaxNode GetNextNode(string code, ref int startIndex, out TagType tagType, Tag previousTag, bool isParseContent)
        {
            string currentSubstring = code.Substring(startIndex);
            //in the regex I won't work with h1, h2... and such tags!
            string regexPattern = previousTag == null || isParseContent ? @"</?[a-zA-Z]+[^<>]*/?>" : @"<" + previousTag.CloseTag + @">";
            Match tag = Regex.Match(currentSubstring, regexPattern);
            HtmlTag registeredTag = null;
            tagType = TagType.Open; //we just set it here and when needed will do it later
            while (tag.Success && (registeredTag = (GetTagByValue(tag.Value) as HtmlTag)) == null)//getting tags, but they are not registered.
            {
                tag = tag.NextMatch();
            }
            if (!tag.Success) //didn't find a registered tag until the end - return everything left as a text node
            {
                startIndex += currentSubstring.Length;
                return new TextSyntaxNode(currentSubstring);
            }
            if (tag.Index == 0) //if the string given to the matcher was something like '<strong>...'
            {
                if (tag.Value.StartsWith("</"))
                {
                    tagType = TagType.Close;
                }
                else if (tag.Value.EndsWith("/>"))
                {
                    tagType = TagType.SelfClose;
                }
                startIndex += tag.Value.Length;
                return new HtmlSyntaxNode(registeredTag, tag.Value);
            }
            //there has been some text before the tag that I found...
            startIndex += tag.Index; //move the start index for the other iterations
            return new TextSyntaxNode(currentSubstring.Substring(0, tag.Index));
        }
    }
}
