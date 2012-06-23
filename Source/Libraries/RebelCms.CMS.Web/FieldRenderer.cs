using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Web;
using System.Web.Mvc;
using RebelCms.Cms.Web.Context;
using RebelCms.Cms.Web.Macros;
using RebelCms.Cms.Web.Model;
using RebelCms.Framework;
using RebelCms.Framework.Persistence.Model.Constants.AttributeDefinitions;

namespace RebelCms.Cms.Web
{
    /// <summary>
    /// A utility class for rendering an RebelCms content Field/Property
    /// </summary>
    public class FieldRenderer
    {
        public IHtmlString RenderField(IRoutableRequestContext routableRequestContext, ControllerContext controllerContext, Content item,
            string fieldAlias = "", string valueAlias = "", string altFieldAlias = "", string altValueAlias = "", string altText = "", string insertBefore = "", string insertAfter = "",
            bool recursive = false, bool convertLineBreaks = false, bool removeParagraphTags = false,
            RebelCmsRenderItemCaseType casing = RebelCmsRenderItemCaseType.Unchanged,
            RebelCmsRenderItemEncodingType encoding = RebelCmsRenderItemEncodingType.Unchanged)
        {
            var sb = new StringBuilder();

            // Handle hard coded "friendly" system keys
            if (fieldAlias == "Name" || fieldAlias == "UrlName")
            {
                valueAlias = fieldAlias;
                fieldAlias = NodeNameAttributeDefinition.AliasValue;
            }

            if (fieldAlias == "CurrentTemplateId")
            {
                fieldAlias = SelectedTemplateAttributeDefinition.AliasValue;
                valueAlias = "TemplateId";
            }

            if (altFieldAlias == "Name" || altFieldAlias == "UrlName")
            {
                altValueAlias = altFieldAlias;
                altFieldAlias = NodeNameAttributeDefinition.AliasValue;
            }

            if (altFieldAlias == "CurrentTemplateId")
            {
                altFieldAlias = SelectedTemplateAttributeDefinition.AliasValue;
                altValueAlias = "TemplateId";
            }

            var val = item.Field<string>(fieldAlias, valueAlias, recursive);

            if (val.IsNullOrWhiteSpace() && !altFieldAlias.IsNullOrWhiteSpace())
            {
                val = item.Field<string>(altFieldAlias, altValueAlias, recursive);
            }

            if (val.IsNullOrWhiteSpace() && !altText.IsNullOrWhiteSpace())
            {
                val = altText;
            }

            if(!val.IsNullOrWhiteSpace())
            {
                switch (casing)
                {
                    case RebelCmsRenderItemCaseType.Upper:
                        val = val.ToUpper();
                        break;
                    case RebelCmsRenderItemCaseType.Lower:
                        val = val.ToLower();
                        break;
                    case RebelCmsRenderItemCaseType.Title:
                        val = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(val);
                        break;
                    default:
                        break;
                }

                switch (encoding)
                {
                    case RebelCmsRenderItemEncodingType.Url:
                        val = HttpUtility.UrlEncode(val);
                        break;
                    case RebelCmsRenderItemEncodingType.Html:
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
    }
}