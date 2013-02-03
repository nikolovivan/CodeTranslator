using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CodeTranslator.Tags;
using CodeTranslator.Nodes;

namespace CodeTranslator.Parsers
{
    /// <summary>
    /// Represents an abstract class for all parsers.
    /// </summary>
    public abstract class ParserBase
    {
        /// <summary>
        /// The list of available tags.
        /// </summary>
        public Tag[] Tags { get; private set; }
        /// <summary>
        /// Creates a parser.
        /// </summary>
        /// <param name="tags">The available tags.</param>
        public ParserBase(params Tag[] tags)
        {
            if (tags == null || tags.Length == 0) throw new ArgumentNullException("tags");
            if (!IsTagsDistinct(tags)) throw new ArgumentException("The tags set contains duplicates!");
            Tags = tags;
        }

        /// <summary>
        /// Checks if the tags are distinct.
        /// </summary>
        /// <param name="tags">List of tags from which to check</param>        
        private bool IsTagsDistinct(Tag[] tags)
        {
            HashSet<string> openTags = new HashSet<string>();
            HashSet<string> closeTags = new HashSet<string>();
            foreach (Tag tag in tags)
            {
                openTags.Add(tag.OpenTag);
                closeTags.Add(tag.CloseTag);
            }
            return openTags.Count == tags.Length && closeTags.Count == tags.Length;
        }

        /// <summary>
        /// Parses code to a Syntax Node
        /// </summary>
        /// <param name="code">The code (html or bbcode)</param> 
        protected abstract SyntaxNode ParseSyntaxNode(string code);

        /// <summary>
        /// Gets the next node after the start index.
        /// </summary>
        /// <param name="code">The code.</param>
        /// <param name="startIndex">The index from which to start searching.</param>
        /// <param name="tagType">Returns the next tag type</param>
        /// <param name="previousTag">The previous tag in the stack.</param>
        /// <param name="isParseContent">If the content of the last opening node should be parsed.</param>
        protected abstract SyntaxNode GetNextNode(string code, ref int startIndex, out TagType tagType, Tag previousTag, bool isParseContent);

        /// <summary>
        /// Gets a registered tag by its value.
        /// </summary>
        /// <param name="tagValue">The value of the tag.</param>
        protected Tag GetTagByValue(string tagValue)
        {
            return Tags.SingleOrDefault(t => t.IsValid(tagValue));
        }
    }
}
