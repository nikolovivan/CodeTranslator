using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CodeTranslator.Exceptions
{
    public class HtmlParsingException : Exception
    {
        /// <summary>
        /// Creates an html parsing exception
        /// </summary>
        public HtmlParsingException() : base() { }
        /// <summary>
        /// Creates an html parsing exception
        /// </summary>
        /// <param name="message">The message.</param>
        public HtmlParsingException(string message) : base(message) { }
    }
}
