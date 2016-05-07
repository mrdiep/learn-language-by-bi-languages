using HtmlAgilityPack;

namespace VoiceSubtitle.Helper
{
    public static class WebHelper
    {
        public static string InnerHtmlText(string text)
        {
            var html = new HtmlDocument();
            html.LoadHtml(text);
            return html.DocumentNode.InnerText;
        }
    }
}