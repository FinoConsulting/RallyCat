using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using HtmlAgilityPack;


namespace RallyCat.Core.Services
{
    public class HtmlConvertService
    {
        private const String c_RallyHostUrl = @"https://rally1.rallydev.com";

        private Int32 _TLevel;
        private Boolean _BTag;

        public String ConvertFromString(String str)
        {
            var doc = new HtmlDocument();
            doc.Load(str.ToStream(), Encoding.UTF8);

            var sw = new StringWriter();

            ConvertTo(doc.DocumentNode, sw);
            sw.Flush();
            return sw.ToString();
        }

        public List<String> GetAllImageSrcs(String str)
        {
            var doc = new HtmlDocument();
            doc.Load(str.ToStream());

            var imgs = doc.DocumentNode.SelectNodes("//img");
            if ((imgs == null) || !imgs.Any()) { return null; }

            var result = imgs.Select(i => c_RallyHostUrl + i.Attributes[@"src"].Value).ToList();
            return result;
        }

        public void ConvertTo(HtmlNode node, TextWriter outText)
        {
            switch (node.NodeType)
            {
                case HtmlNodeType.Comment:
                    // don't output comments
                    break;

                case HtmlNodeType.Document:
                    ConvertContentTo(node, outText);
                    break;

                case HtmlNodeType.Text:
                    // script and style must not be output
                    var parentName = node.ParentNode.Name;
                    if ((parentName == "script") || (parentName == "style"))
                    {
                        break;
                    }

                    // get text
                    var html = ((HtmlTextNode)node).Text;

                    // is it in fact a special closing node output as text?
                    if (HtmlNode.IsOverlappedClosingElement(html))
                    {
                        break;
                    }

                    // check the text is meaningful and not a bunch of whitespaces
                    if (html.Trim().Length > 0)
                    {
                        if (_BTag)
                        {
                            outText.Write("*");
                        }
                        outText.Write(HtmlEntity.DeEntitize(html));
                        if (_BTag)
                        {
                            outText.Write("*");
                        }
                    }
                    break;

                case HtmlNodeType.Element:
                    switch (node.Name)
                    {
                        case "b":
                            _BTag = true;
                            // treat paragraphs as crlf
                            break;
                        case "p":
                            // treat paragraphs as crlf
                            outText.Write("\r\n");
                            break;
                        case "div":
                            // treat paragraphs as crlf
                            outText.Write("\r\n");
                            break;
                        case "span":
                            // treat paragraphs as crlf
                            outText.Write("\r\n");
                            break;
                        case "ul":
                            // treat paragraphs as crlf
                            outText.Write("\r\n");
                            _TLevel += 1;
                            break;
                        case "li":
                            // treat paragraphs as crlf

                            var ts = String.Join("", Enumerable.Repeat("\t", _TLevel));
                            outText.Write("\r\n");
                            outText.Write(">");
                            outText.Write(ts);
                            break;
                    }

                    if (node.HasChildNodes)
                    {
                        ConvertContentTo(node, outText);
                    }

                    switch (node.Name)
                    {
                        case "b":
                            _BTag = false;
                            break;
                        case "ul":
                            _TLevel -= 1;
                            break;
                    }

                    break;
            }
        }

        private void ConvertContentTo(HtmlNode node, TextWriter outText)
        {
            foreach (var subnode in node.ChildNodes)
            {
                ConvertTo(subnode, outText);
            }
        }
    }
}