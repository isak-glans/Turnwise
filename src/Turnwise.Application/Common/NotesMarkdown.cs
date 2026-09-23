using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace Turnwise.Application.Common;

/// <summary>
/// Converts the small subset of Markdown supported by the Notes editor - bold, italic, bullet
/// and numbered lists, headings, links - into HTML for read-only display. Hand-rolled rather
/// than a full CommonMark implementation: deliberately minimal, and safe by construction. Every
/// character of the source text is HTML-encoded before any markdown syntax is turned into tags,
/// so the source can never inject markup, and links are restricted to the http(s)/mailto
/// schemes (anything else is left as literal "[text](url)" text rather than becoming a link).
/// </summary>
public static class NotesMarkdown
{
    private static readonly Regex HeadingPattern = new(@"^(#{1,3})\s+(.*)$", RegexOptions.Compiled);
    private static readonly Regex UnorderedItemPattern = new(@"^-\s+(.*)$", RegexOptions.Compiled);
    private static readonly Regex OrderedItemPattern = new(@"^\d+\.\s+(.*)$", RegexOptions.Compiled);
    private static readonly Regex LinkPattern = new(@"\[([^\]]+)\]\(([^)\s]+)\)", RegexOptions.Compiled);
    private static readonly Regex BoldPattern = new(@"\*\*(.+?)\*\*", RegexOptions.Compiled);
    private static readonly Regex ItalicPattern = new(@"(?<!\*)\*(?!\*)(.+?)(?<!\*)\*(?!\*)", RegexOptions.Compiled);

    public static string ToHtml(string? markdown)
    {
        if (string.IsNullOrWhiteSpace(markdown))
        {
            return "";
        }

        var html = new StringBuilder();
        var lines = markdown.Replace("\r\n", "\n").Split('\n');
        string? openListTag = null;
        var paragraphOpen = false;

        void CloseList()
        {
            if (openListTag is not null)
            {
                html.Append('<').Append('/').Append(openListTag).Append('>');
                openListTag = null;
            }
        }

        void CloseParagraph()
        {
            if (paragraphOpen)
            {
                html.Append("</p>");
                paragraphOpen = false;
            }
        }

        foreach (var rawLine in lines)
        {
            var line = rawLine.TrimEnd();

            if (line.Length == 0)
            {
                CloseList();
                CloseParagraph();
                continue;
            }

            var heading = HeadingPattern.Match(line);
            if (heading.Success)
            {
                CloseList();
                CloseParagraph();
                var level = heading.Groups[1].Value.Length;
                html.Append($"<h{level}>").Append(FormatInline(heading.Groups[2].Value)).Append($"</h{level}>");
                continue;
            }

            var unordered = UnorderedItemPattern.Match(line);
            var ordered = unordered.Success ? null : OrderedItemPattern.Match(line);
            if (unordered.Success || ordered is { Success: true })
            {
                CloseParagraph();
                var wantTag = unordered.Success ? "ul" : "ol";
                if (openListTag != wantTag)
                {
                    CloseList();
                    html.Append('<').Append(wantTag).Append('>');
                    openListTag = wantTag;
                }

                var itemText = unordered.Success ? unordered.Groups[1].Value : ordered!.Groups[1].Value;
                html.Append("<li>").Append(FormatInline(itemText)).Append("</li>");
                continue;
            }

            CloseList();
            if (!paragraphOpen)
            {
                html.Append("<p>");
                paragraphOpen = true;
            }
            else
            {
                html.Append("<br>");
            }

            html.Append(FormatInline(line));
        }

        CloseList();
        CloseParagraph();
        return html.ToString();
    }

    private static string FormatInline(string text)
    {
        var encoded = WebUtility.HtmlEncode(text);

        encoded = LinkPattern.Replace(encoded, m =>
        {
            var label = m.Groups[1].Value;
            var rawUrl = WebUtility.HtmlDecode(m.Groups[2].Value); // undo encoding so the scheme check sees the real URL
            if (!IsSafeUrl(rawUrl))
            {
                return m.Value; // not a scheme we allow - leave the "[text](url)" text as-is
            }

            var safeHref = WebUtility.HtmlEncode(rawUrl);
            return $"<a href=\"{safeHref}\" target=\"_blank\" rel=\"noopener noreferrer\">{label}</a>";
        });

        encoded = BoldPattern.Replace(encoded, "<strong>$1</strong>");
        encoded = ItalicPattern.Replace(encoded, "<em>$1</em>");

        return encoded;
    }

    private static bool IsSafeUrl(string url) =>
        Uri.TryCreate(url, UriKind.Absolute, out var uri) &&
        (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps || uri.Scheme == "mailto");
}
