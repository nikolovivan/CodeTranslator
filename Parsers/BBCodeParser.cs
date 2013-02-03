using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CodeTranslator.Tags;
using CodeTranslator.Constants;
using CodeTranslator.Nodes;
using CodeTranslator.Exceptions;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace CodeTranslator.Parsers
{
    /// <summary>
    /// A class that provides methods for parsing from bbcode
    /// </summary>
    public class BBCodeParser : ParserBase
    {
        /// <summary>
        /// Creates a parser.
        /// </summary>
        /// <param name="tags">The available tags.</param>
        public BBCodeParser(params BBTag[] tags) : base(tags) { }
        /// <summary>
        /// Returns the html equivalent of a bbcode
        /// </summary>
        /// <param name="bbcode">The bbcode</param>        
        public string ToHtml(string bbcode)
        {
            if (string.IsNullOrEmpty(bbcode)) throw new ArgumentNullException("bbcode");
            return String.Concat(Constants.Constants.OpenHtmlParagraph, ParseSyntaxNode(bbcode).ToHtml(), Constants.Constants.CloseHtmlParagraph)
                .Replace(Constants.Constants.OpenHtmlParagraph+Constants.Constants.CloseHtmlParagraph,"");//finally replace the empty <p></p> that show up.
        }
        /// <summary>
        /// Parses BBCode to a Syntax Node
        /// </summary>
        /// <param name="code">The bbcode</param> 
        protected override SyntaxNode ParseSyntaxNode(string code)
        {
            if (string.IsNullOrEmpty(code)) throw new ArgumentNullException("code");
            Stack<SyntaxNode> syntaxStack = new Stack<SyntaxNode>(); //this is a stack that will hold the bbcode nodes.
            syntaxStack.Push(new BBCodeSyntaxNode(null, null)); //push the main node.
            int i = 0;
            while (i < code.Length)
            {
                TagType tagType;
                BBCodeSyntaxNode lastNode = syntaxStack.Peek() as BBCodeSyntaxNode;
                if (lastNode == null) throw new BBCodeParsingException("Bad node type in nodes stack.");
                if (syntaxStack.Count > 1 && lastNode.Tag == null) throw new BBCodeParsingException("You can't have more than one root node!");
                SyntaxNode nextNode = GetNextNode(code, ref i, out tagType, lastNode.Tag, lastNode.IsParseContent);
                if (nextNode is TextSyntaxNode) //the text is always added as a child.
                {
                    lastNode.AddChild(nextNode);
                    if (lastNode.IsSelfCloserNode()) //tags like [*] need just text and close themselves. Sometimes they can be empty, but they still have '\n\r'
                    {
                        lastNode = syntaxStack.Pop() as BBCodeSyntaxNode; //pop it out.
                        syntaxStack.Peek().AddChild(lastNode); //add it as a child to the one before it
                    }
                }
                else
                {
                    if (tagType == TagType.Close) //if it's closing tag
                    {
                        //this blows when we have tags with the same endings or beginnings...
                        if (lastNode.Tag != (nextNode as BBCodeSyntaxNode).Tag) throw new BBCodeParsingException("Invalid formatting!");
                        lastNode = syntaxStack.Pop() as BBCodeSyntaxNode; //pop it out.
                        syntaxStack.Peek().AddChild(lastNode);//add it as a child to the one before it.
                    }
                    else//opening tag
                    {
                        syntaxStack.Push(nextNode); //push it to the stack
                    }
                }
            }
            Debug.Assert(syntaxStack.Count == 1); //we must have only one element - the root and to unfold from it.
            return syntaxStack.Pop();
        }  

        /// <summary>
        /// Gets the next node after the start index.
        /// </summary>
        /// <param name="code">The code.</param>
        /// <param name="startIndex">The index from which to start searching.</param>
        /// <param name="tagType">Returns the next tag type</param>
        /// <param name="previousTag">The previous tag in the stack.</param>
        /// <param name="isParseContent">If the content of the last opening node should be parsed.</param>
        /// <returns></returns>
        protected override SyntaxNode GetNextNode(string code, ref int startIndex, out TagType tagType, Tag previousTag, bool isParseContent)
        {
            string currentSubstring = code.Substring(startIndex);
            string regexPattern = previousTag == null || isParseContent ? @"\[/?[a-z*][a-z0-9]*(=[^\[\]\n\r\v\f]+)?\]" : @"\[" + previousTag.CloseTag + @"\]"; //the close tag has /
            Match tag = Regex.Match(currentSubstring, regexPattern);
            BBTag registeredTag = null;
            tagType = TagType.Open; //we just set it here and when needed will do it later
            while (tag.Success && (registeredTag = (GetTagByValue(tag.Value) as BBTag)) == null)//getting tags, but they are not registered.
            {
                tag = tag.NextMatch();
            }
            if (!tag.Success) //didn't find a registered tag until the end - return everything left as a text node
            {
                startIndex += currentSubstring.Length;
                return new TextSyntaxNode(currentSubstring);
            }
            if (tag.Index == 0) //if the string given to the matcher was something like '[b]...'
            {
                if (tag.Value.StartsWith("[/")) tagType = TagType.Close;                
                startIndex += tag.Value.Length;
                return new BBCodeSyntaxNode(registeredTag, tag.Value);
            }
            //there has been some text before the tag that I found...
            startIndex += tag.Index; //move the start index for the other iterations
            return new TextSyntaxNode(currentSubstring.Substring(0, tag.Index));
        }
    }
}
