using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CodeTranslator.Parsers;
using CodeTranslator.Tags;
using CodeTranslator.Options;
using CodeTranslator.Attributes;

namespace CodeTranslator
{
    public class BBCode
    {
        /// <summary>
        /// The default bbcode parser with default rules.
        /// </summary>
        private static readonly BBCodeParser DefaultBBCodeParser = GetDefaultBBCodeParser();
        /// <summary>
        /// The default html parser with default rules.
        /// </summary>
        private static readonly HtmlParser DefaultHtmlParser = GetDefaultHtmlParser();
        /// <summary>
        /// Gets a default bbcode parser with default rules.
        /// </summary>
        private static BBCodeParser GetDefaultBBCodeParser()
        {
            //This is a [b]test[/b] if the new classes. Let's hope they will work fine!!!
            //Another paragraph.
            //Another one...
            //[list=1]
            //[*]Item 1
            //[*]Item 2
            //[*]Item 3
            //[/list]
            //[list]
            //[*]Item 1
            //[*]Item 2
            //[*]Item 3
            //[/list]
            //OK, let's see now!!!
            return new BBCodeParser( 
                new BBTag("b", "/b", new BBCodeOption(null, "<strong>", "</strong>", "<br />", true)),
                new BBTag("i", "/i", new BBCodeOption(null, "<em>", "</em>", "<br />", true)),
                new BBTag("u", "/u", new BBCodeOption(null, "<u>", "</u>", "<br />", true)),
                new BBTag("size", "/size", new BBCodeOption("", "<span style=\"font-size:" + BBTag.OptionPlaceholder + "%;\">", "</span>", "<br />", true)),
                new BBTag("color", "/color", new BBCodeOption("", "<span style=\"color:" + BBTag.OptionPlaceholder + ";\">", "</span>", "<br />", true)),
                new BBTag("list", "/list", new BBCodeOption("1", "<ol>", "</ol>", "", true, true), new BBCodeOption(null, "<ul>", "</ul>", "", true, true)),
                new BBTag("*", "", new BBCodeOption(null, "<li>", "</li>", "", true)),
                //changed the new line replacer to work correctly with the code viewer... It was <br />, but the code viewer escapes it and ruins everything. Now is OK.
                //see what to do with block type tags... Like the pre here...
                new BBTag("code", "/code", new BBCodeOption("", "</p><pre class=\"" + BBTag.OptionPlaceholder + "\">", "</pre><p>", "", false)),
                new BBTag("img", "/img", new BBCodeOption(null, "<img src=\"" + BBTag.ContentPlaceholder + "\" alt=\"" + BBTag.ContentPlaceholder + "\" />", "", "", false)),
                new BBTag("url", "/url", new BBCodeOption("", "<a href=\"" + BBTag.OptionPlaceholder + "\">", "</a>", "<br />", true), new BBCodeOption(null, "<a href=\"" + BBTag.ContentPlaceholder + "\">", "</a>", "", false)),
                new BBTag("email", "/email", new BBCodeOption(null, "<a href=\"" + BBTag.ContentPlaceholder + "\">", "</a>", "", false))//,
                //new BBTag("quote", "/quote", new OptionValues(null, "<blockquote><div>", "</div></blockquote>", "<br />", true))
            );
        }

        /// <summary>
        /// Gets a default html parser with default rules.
        /// </summary>
        private static HtmlParser GetDefaultHtmlParser()
        {
            //See what to do with the self closing html tags!!!
            return new HtmlParser(             
                new HtmlTag("strong", "/strong", "<br />" ,new HtmlOption("b","/b", null, null)),
                new HtmlTag("em", "/em", "<br />", new HtmlOption("i", "/i", null, null)),
                new HtmlTag("u", "/u", "<br />", new HtmlOption("u", "/u", null, null)),
                new HtmlTag("span", "/span", "<br />", new HtmlOption("size","/size", null, new HtmlAttribute("style", new AttributeOption("font-size", "%;"))),
                                                       new HtmlOption("color", "/color", null, new HtmlAttribute("style", new AttributeOption("color", ";")))),
                new HtmlTag("ol", "/ol", "", new HtmlOption("list=1","/list", null, null)),
                new HtmlTag("ul", "/ul", "", new HtmlOption("list","/list", null, null)),
                new HtmlTag("li", "/li", "", new HtmlOption("*","", null, null)),
                new HtmlTag("pre", "/pre", "<br />", new HtmlOption("code","/code", null, new HtmlAttribute("class"))),//here is okay, because it looks for explicit <br />, which we don't have (escaped). \n are just flushed.
                new HtmlTag("img", "/img", "", new HtmlOption("img","/img", new HtmlAttribute("src"), null)),
                new HtmlTag("a", "/a", "", new HtmlOption("url","/url", new HtmlAttribute("href"), new HtmlAttribute("href")))//don't have description for mail, because it's the same and the ckeditor will fix it...
            );
        }

        /// <summary>
        /// Converts the BBCode to Html SAFELY!
        /// </summary>
        /// <param name="bbcode">The BBCode</param>        
        public static string ToHtml(string bbcode)
        {
            if (string.IsNullOrEmpty(bbcode)) throw new ArgumentNullException("bbcode");
            return DefaultBBCodeParser.ToHtml(bbcode);
        }

        /// <summary>
        /// Converts Html to BBCode
        /// </summary>
        /// <param name="html">The Html</param>
        public static string FromHtml(string html)
        {
            if (string.IsNullOrEmpty(html)) throw new ArgumentNullException("html");
            return DefaultHtmlParser.ToBBCode(html);
        }
    }
}
