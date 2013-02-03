using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CodeTranslator.Options
{
    /// <summary>
    /// Represents a base abstract class for all options
    /// </summary>
    public abstract class TagOption
    {
        /// <summary>
        /// The opening tag for the given option.
        /// </summary>
        public string OpenTag { get; private set; }
        /// <summary>
        /// The closing tag for the given option.
        /// </summary>
        public string CloseTag { get; private set; }

        /// <summary>
        /// Creates a tag option class
        /// </summary>
        /// <param name="openTag">The open tag</param>
        /// <param name="closeTag">The close tag</param>
        public TagOption(string openTag, string closeTag)
        {
            if (string.IsNullOrEmpty(openTag.Trim())) throw new ArgumentNullException("openTag");
            OpenTag = openTag.Trim();
            CloseTag = closeTag.Trim();
        }        
    }
}
