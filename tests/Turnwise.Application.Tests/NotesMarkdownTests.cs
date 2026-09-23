using Turnwise.Application.Common;
using Xunit;

namespace Turnwise.Application.Tests;

public class NotesMarkdownTests
{
    [Fact]
    public void ToHtml_EmptyOrWhitespace_ReturnsEmptyString()
    {
        Assert.Equal("", NotesMarkdown.ToHtml(null));
        Assert.Equal("", NotesMarkdown.ToHtml(""));
        Assert.Equal("", NotesMarkdown.ToHtml("   "));
    }

    [Fact]
    public void ToHtml_PlainTextBecomesAParagraph()
    {
        Assert.Equal("<p>Hiding in the bushes.</p>", NotesMarkdown.ToHtml("Hiding in the bushes."));
    }

    [Fact]
    public void ToHtml_SingleNewlineWithinAParagraphBecomesLineBreak()
    {
        Assert.Equal("<p>Line one<br>Line two</p>", NotesMarkdown.ToHtml("Line one\nLine two"));
    }

    [Fact]
    public void ToHtml_BlankLineSeparatesParagraphs()
    {
        Assert.Equal("<p>First.</p><p>Second.</p>", NotesMarkdown.ToHtml("First.\n\nSecond."));
    }

    [Theory]
    [InlineData("**bold**", "<p><strong>bold</strong></p>")]
    [InlineData("*italic*", "<p><em>italic</em></p>")]
    [InlineData("**bold** and *italic*", "<p><strong>bold</strong> and <em>italic</em></p>")]
    public void ToHtml_SupportsBoldAndItalic(string markdown, string expected)
    {
        Assert.Equal(expected, NotesMarkdown.ToHtml(markdown));
    }

    [Theory]
    [InlineData("# Heading 1", "<h1>Heading 1</h1>")]
    [InlineData("## Heading 2", "<h2>Heading 2</h2>")]
    [InlineData("### Heading 3", "<h3>Heading 3</h3>")]
    public void ToHtml_SupportsHeadings(string markdown, string expected)
    {
        Assert.Equal(expected, NotesMarkdown.ToHtml(markdown));
    }

    [Fact]
    public void ToHtml_BulletListLinesBecomeAnUnorderedList()
    {
        Assert.Equal("<ul><li>Ammo</li><li>Rations</li></ul>", NotesMarkdown.ToHtml("- Ammo\n- Rations"));
    }

    [Fact]
    public void ToHtml_NumberedListLinesBecomeAnOrderedList()
    {
        Assert.Equal("<ol><li>First</li><li>Second</li></ol>", NotesMarkdown.ToHtml("1. First\n2. Second"));
    }

    [Fact]
    public void ToHtml_SwitchingListTypeMidStreamClosesTheFirstList()
    {
        Assert.Equal("<ul><li>A</li></ul><ol><li>B</li></ol>", NotesMarkdown.ToHtml("- A\n1. B"));
    }

    [Fact]
    public void ToHtml_HttpsLinkBecomesAnAnchorTag()
    {
        var result = NotesMarkdown.ToHtml("See [my character](https://example.com/rp)");

        Assert.Equal(
            "<p>See <a href=\"https://example.com/rp\" target=\"_blank\" rel=\"noopener noreferrer\">my character</a></p>",
            result);
    }

    [Fact]
    public void ToHtml_HttpAndMailtoLinksAreAlsoAllowed()
    {
        Assert.Contains("href=\"http://example.com\"", NotesMarkdown.ToHtml("[link](http://example.com)"));
        Assert.Contains("href=\"mailto:gm@example.com\"", NotesMarkdown.ToHtml("[email](mailto:gm@example.com)"));
    }

    [Fact]
    public void ToHtml_JavascriptSchemeLinkIsNotTurnedIntoAnAnchor()
    {
        var result = NotesMarkdown.ToHtml("[click me](javascript:alert(1))");

        Assert.DoesNotContain("<a ", result);
        Assert.Contains("[click me](javascript:alert(1))", result);
    }

    [Fact]
    public void ToHtml_DataSchemeLinkIsNotTurnedIntoAnAnchor()
    {
        var result = NotesMarkdown.ToHtml("[x](data:text/html,<script>alert(1)</script>)");

        Assert.DoesNotContain("<a ", result);
    }

    [Theory]
    [InlineData("<script>alert(1)</script>", "<p>&lt;script&gt;alert(1)&lt;/script&gt;</p>")]
    [InlineData("<img src=x onerror=alert(1)>", "<p>&lt;img src=x onerror=alert(1)&gt;</p>")]
    [InlineData("5 < 10 & 10 > 5", "<p>5 &lt; 10 &amp; 10 &gt; 5</p>")]
    public void ToHtml_HtmlSpecialCharactersAreAlwaysEncoded(string markdown, string expected)
    {
        Assert.Equal(expected, NotesMarkdown.ToHtml(markdown));
    }

    [Fact]
    public void ToHtml_LinkTextItselfIsHtmlEncoded()
    {
        var result = NotesMarkdown.ToHtml("[<b>click</b>](https://example.com)");

        Assert.Contains("&lt;b&gt;click&lt;/b&gt;", result);
        Assert.DoesNotContain("<b>click</b>", result);
    }

    [Fact]
    public void ToHtml_LinkUrlWithAmpersandRoundTripsCorrectly()
    {
        var result = NotesMarkdown.ToHtml("[search](https://example.com/page?a=1&b=2)");

        Assert.Contains("href=\"https://example.com/page?a=1&amp;b=2\"", result);
    }

    [Fact]
    public void ToHtml_RealisticCharacterNotesRenderAsExpected()
    {
        var markdown = "## Background\n\nSecretly a **good** goblin.\n\nInventory:\n- Rusty dagger\n- 3 gold\n\nRP thread: [forum post](https://example.com/rp/123)";

        var result = NotesMarkdown.ToHtml(markdown);

        Assert.Equal(
            "<h2>Background</h2><p>Secretly a <strong>good</strong> goblin.</p><p>Inventory:</p>" +
            "<ul><li>Rusty dagger</li><li>3 gold</li></ul>" +
            "<p>RP thread: <a href=\"https://example.com/rp/123\" target=\"_blank\" rel=\"noopener noreferrer\">forum post</a></p>",
            result);
    }
}
