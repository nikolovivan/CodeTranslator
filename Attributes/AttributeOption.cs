using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CodeTranslator.Attributes
{
    /// <summary>
    /// A class that represents an attribute option:
    /// if we have style="font-size: 100%;", this class represents
    /// font-size: 100%;    
    /// </summary>
    public class AttributeOption
    {
        /// <summary>
        /// Separates the name from the value
        /// </summary>
        public const string ValueSeparator = ":";
        /// <summary>
        /// Delimits different attributevalues
        /// </summary>        
        public string OptionDelimiter { get; private set; }
        /// <summary>
        /// The name of the attribute value, e. g. 'font-style'. (No :)
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Creates an attribute option.
        /// </summary>
        /// <param name="name">The name,  e. g. 'font-style'. (No :)</param>
        /// <param name="optionDelimiter">The delimiter, until which the value is taken. We can use space.</param>        
        public AttributeOption(string name, string optionDelimiter)
        {
            if (string.IsNullOrEmpty(name.Trim())) throw new ArgumentNullException("name");
            if (string.IsNullOrEmpty(optionDelimiter)) throw new ArgumentNullException("optionDelimiter");
            Name = name.Trim();
            OptionDelimiter = optionDelimiter;
        }

        public override bool Equals(object obj)
        {
            AttributeOption av = obj as AttributeOption;
            if (av == null) return false;
            return this.GetHashCode() == av.GetHashCode();
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode(); //we don't care about the option delimiter!
        }
    }
}
