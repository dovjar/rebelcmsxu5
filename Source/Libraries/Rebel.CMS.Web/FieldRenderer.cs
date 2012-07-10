using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Web;
using System.Web.Mvc;
using Rebel.Cms.Web.Context;
using Rebel.Cms.Web.Macros;
using Rebel.Cms.Web.Model;
using Rebel.Framework;
using Rebel.Framework.Persistence.Model.Constants.AttributeDefinitions;

namespace Rebel.Cms.Web
{
    /// <summary>
    /// A utility class for rendering an rebel content Field/Property
    /// </summary>
    public class FieldRenderer
    {
        public IHtmlString RenderField(IRoutableRequestContext routableRequestContext, ControllerContext controllerContext, Content item,
            string fieldAlias = "", string valueAlias = "", string altFieldAlias = "", string altValueAlias = "", string altText = "", string insertBefore = "", string insertAfter = "",
            bool recursive = false, bool convertLineBreaks = false, bool removeParagraphTags = false,
            RebelRenderItemCaseType casing = RebelRenderItemCaseType.Unchanged,
            RebelRenderItemEncodingType encoding = RebelRenderItemEncodingType.Unchanged,
            string formatString = "")
        {
            var sb = new StringBuilder();

            var valObj = GetFieldValue(item, fieldAlias, valueAlias, recursive);

            if ((valObj == null || valObj.ToString().IsNullOrWhiteSpace()) && !altFieldAlias.IsNullOrWhiteSpace())
            {
                valObj = GetFieldValue(item, altFieldAlias, altValueAlias, recursive);
            }

            if ((valObj == null || valObj.ToString().IsNullOrWhiteSpace()) && !altText.IsNullOrWhiteSpace())
            {
                valObj = altText;
            }

            if(!formatString.IsNullOrWhiteSpace())
                formatString = "{0:" + formatString.Replace("\\", "\\\\").Replace("\"", "\\\"") + "}";
            else
                formatString = "{0}";

            var val = string.Format(formatString, valObj);

            if(!val.IsNullOrWhiteSpace())
            {
                switch (casing)
                {
                    case RebelRenderItemCaseType.Upper:
                        val = val.ToUpper();
                        break;
                    case RebelRenderItemCaseType.Lower:
                        val = val.ToLower();
                        break;
                    case RebelRenderItemCaseType.Title:
                        val = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(val);
                        break;
                    default:
                        break;
                }

                switch (encoding)
                {
                    case RebelRenderItemEncodingType.Url:
                        val = HttpUtility.UrlEncode(val);
                        break;
                    case RebelRenderItemEncodingType.Html:
                        val = HttpUtility.HtmlEncode(val);
                        break;
                    default:
                        break;
                }

                if (convertLineBreaks)
                {
                    val = val.Replace(Environment.NewLine, "<br />");
                }

                if (removeParagraphTags)
                {
                    val = val.Trim().Trim("<p>").Trim("</p>");
                }

                sb.Append(HttpUtility.HtmlDecode(insertBefore));
                sb.Append(val);
                sb.Append(HttpUtility.HtmlDecode(insertAfter));
            }

            //now we need to parse the macro syntax out and replace it with the rendered macro content

            var macroRenderer = new MacroRenderer(routableRequestContext.RegisteredComponents, routableRequestContext);
            var macroParser = new MacroSyntaxParser();
            IEnumerable<MacroParserResult> parseResults;
            var parsed = macroParser.Parse(sb.ToString(),
                                           (macroAlias, macroParams)
                                           => macroRenderer.RenderMacroAsString(macroAlias,
                                                                                macroParams,
                                                                                controllerContext, false,
                                                                                () => item), out parseResults);

            //now we need to parse any internal links and replace with actual URLs
            var linkParse = new LinkSyntaxParser();
            parsed = linkParse.Parse(parsed, x => routableRequestContext.RoutingEngine.GetUrl(x));

            return new MvcHtmlString(parsed);
        }

        private static object GetFieldValue(Content item, string fieldAlias, string valueAlias, bool recursive)
        {
            switch (fieldAlias)
            {
                case "@id":
                    return item.Id;
                case "@parentId":
                    return item.ParentContent().Id;
                case "@name":
                case "Name":
                    return item.Name;
                case "@urlName":
                case "UrlName":
                    return item.UrlName;
                case "@path":
                    return item.GetPath().ToString();
                case "@level":
                    return item.GetPath().Level;
                case "@template":
                    return item.CurrentTemplate.Alias;
                case "@templateId":
                case "CurrentTemplateId":
                    return item.CurrentTemplate.Id;
                case "@templateFileName":
                    return item.CurrentTemplate.Id.Value;
                case "@nodeTypeAlias":
                    return item.ContentType.Alias;
                case "@createDate":
                    return item.UtcCreated;
                case "@updateDate":
                    return item.UtcModified;
                case "@statusChangedDate":
                    return item.UtcStatusChanged;
                default:
                    return item.Field(fieldAlias, valueAlias, recursive);
            }
        }
    }
}