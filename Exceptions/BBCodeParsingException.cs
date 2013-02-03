using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CodeTranslator.Exceptions
{
    public class BBCodeParsingException : Exception
    {
        /// <summary>
        /// Creates a bbcode parsing exception
        /// </summary>
        public BBCodeParsingException() : base() { }
        /// <summary>
        /// Creates a bbcode parsing exception
        /// </summary>
        /// <param name="message">The message.</param>
        public BBCodeParsingException(string message) : base(message) { }
    }
}
