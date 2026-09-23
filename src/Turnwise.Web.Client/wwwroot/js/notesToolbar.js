// Small helpers for the Notes markdown toolbar. All plain-text manipulation of a <textarea> -
// no HTML/DOM content involved, so there's nothing here to sanitize (that's NotesMarkdown.cs's
// job, applied only when the stored markdown is rendered for read-only display).

function notifyInput(textareaEl) {
    textareaEl.dispatchEvent(new Event("input", { bubbles: true }));
}

/// Wraps the current selection in `before`/`after` (e.g. "**"/"**" for bold). With no
/// selection, inserts the markers with the cursor left between them.
export function wrapSelection(textareaEl, before, after) {
    const start = textareaEl.selectionStart;
    const end = textareaEl.selectionEnd;
    const value = textareaEl.value;
    const selected = value.slice(start, end);

    textareaEl.value = value.slice(0, start) + before + selected + after + value.slice(end);
    textareaEl.focus();
    textareaEl.selectionStart = start + before.length;
    textareaEl.selectionEnd = start + before.length + selected.length;
    notifyInput(textareaEl);
}

/// Inserts `prefix` at the start of the line the cursor is on (e.g. "- " for a bullet).
export function prefixLine(textareaEl, prefix) {
    const start = textareaEl.selectionStart;
    const value = textareaEl.value;
    const lineStart = value.lastIndexOf("\n", start - 1) + 1;

    textareaEl.value = value.slice(0, lineStart) + prefix + value.slice(lineStart);
    textareaEl.focus();
    const newPos = start + prefix.length;
    textareaEl.selectionStart = textareaEl.selectionEnd = newPos;
    notifyInput(textareaEl);
}

/// Prompts for a URL and inserts a markdown link, using the current selection (or a
/// placeholder) as the link text. A native prompt() is a deliberately simple choice here
/// over building a custom dialog for one field.
export function insertLink(textareaEl) {
    const start = textareaEl.selectionStart;
    const end = textareaEl.selectionEnd;
    const value = textareaEl.value;
    const selected = value.slice(start, end) || "link text";

    const url = window.prompt("Link URL", "https://");
    if (!url) {
        return;
    }

    const markdown = `[${selected}](${url})`;
    textareaEl.value = value.slice(0, start) + markdown + value.slice(end);
    textareaEl.focus();
    const newPos = start + markdown.length;
    textareaEl.selectionStart = textareaEl.selectionEnd = newPos;
    notifyInput(textareaEl);
}
