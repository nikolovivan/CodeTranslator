using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace CodeTranslator.Attributes
{
    /// <summary>
    /// Represents a class for an html attribute.
    /// </summary>
    public class HtmlAttribute
    {
        /// <summary>
        /// The attribute name, e. g. style
        /// </summary>
        public string Name { get; private set; }
        /// <summary>
        /// The value for the current attribute, e. g.
        /// for the font-size, width, etc. value of the style attribute.
        /// </summary>
        public AttributeOption Value { get; private set; }
        /// <summary>
        /// Tells if the current attribute has a value (part which we use.)
        /// </summary>
        public bool HasValue { get { return Value != null; } }

        /// <summary>
        /// Creates a new html attribute without a specific option (takes the whole value).
        /// </summary>
        /// <param name="name">The name of the attribute</param>        
        public HtmlAttribute(string name)
        {
            if (string.IsNullOrEmpty(name.Trim())) throw new ArgumentNullException("name");
            Name = name.Trim();
        }

        /// <summary>
        /// Creates a new html attribute with a specific option (takes a part of the value).
        /// </summary>
        /// <param name="name">The name of the attribute.</param>
        /// <param name="value">The value we're interested in.</param>
        public HtmlAttribute(string name, AttributeOption value)
            : this(name)
        {
            if (value == null) throw new ArgumentNullException("value");
            Value = value;
        }

        public override bool Equals(object obj)
        {
            HtmlAttribute attr = obj as HtmlAttribute;
            if (attr == null) return false;
            return this.GetHashCode() == attr.GetHashCode();
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode() ^ (Value != null ? Value.GetHashCode() : -1);
        }

        /// <summary>
        /// Gets the tag representation and returns all attributes that are currently used.
        /// </summary>
        /// <param name="tagRepresentation">The tag representation - usually without &lt;&gt;, but will work with them, too...</param>        
        public static HtmlAttribute[] GetAttributes(string tagRepresentation)
        {
            if (string.IsNullOrEmpty(tagRepresentation)) throw new ArgumentNullException("tagRepresentation");
            List<HtmlAttribute> result = new List<HtmlAttribute>();
            Match attribute = Regex.Match(tagRepresentation, "\\w+=\"[^\"]*\"");
            //make all possible combinations - the whole value and all possible attribute values.
            while (attribute.Success)
            {
                string attributeName = attribute.Value.Substring(0, attribute.Value.IndexOf('='));
                result.Add(new HtmlAttribute(attributeName));
                foreach (Match attributeValue in Regex.Matches(attribute.Value.Substring(attribute.Value.IndexOf('=')),"[\\w\\-]+\\s*" + AttributeOption.ValueSeparator)) //the attributes are usually words or words with '-' ending on ':' - if the value separator is different than ":" there may be a need to change something!!!
                {
                    result.Add(new HtmlAttribute(attributeName, new AttributeOption(attributeValue.Value.Substring(0, attributeValue.Value.IndexOf(AttributeOption.ValueSeparator)).Trim(), ";")));//the option delimiter here is not important
                }
                attribute = attribute.NextMatch();
            }
            return result.ToArray();
        }

        /// <summary>
        /// Returns the value of the given attribute in the given tag representation
        /// </summary>
        /// <param name="tagRepresentation">The tag representation.</param>
        /// <param name="attribute">The attribute which value we need.</param>        
        public static string GetAttributeValue(string tagRepresentation, HtmlAttribute attribute)
        {
            if (string.IsNullOrEmpty(tagRepresentation)) throw new ArgumentNullException("tagRepresentation");
            if (attribute == null) throw new ArgumentNullException("attribute");
            //there can be a problem if the attribute name contains some regex symbols.
            Match attr = Regex.Match(tagRepresentation, attribute.Name + "=\"[^\"]*\"");
            if (!attr.Success) return null; //don't have such attribute.
            if (!attribute.HasValue) return attr.Value.Substring(attr.Value.IndexOf('=') + 1).Trim('"');//if we want to get a whole value attribute - return only the value without the quotation marks...
            string attributeNameWithSeparator = attribute.Value.Name + AttributeOption.ValueSeparator;
            if (!attr.Value.Contains(attributeNameWithSeparator)) return null;
            string attrVal = attr.Value.Substring(attr.Value.IndexOf(attributeNameWithSeparator) + attributeNameWithSeparator.Length).Trim(); //get everything after the name
            if (!attrVal.Contains(attribute.Value.OptionDelimiter)) return attrVal; //if there is no delimiter, return everything else.
            return attrVal.Substring(0, attrVal.IndexOf(attribute.Value.OptionDelimiter)).Trim();                        
        }
    }
}
