// A minimal WYSIWYG editor for the character Notes field. Uses the browser's built-in
// execCommand for formatting (still supported everywhere that matters here, and far
// simpler than reimplementing rich-text editing from scratch for a hobby project).
//
// Notes is stored as HTML. Two different trust levels apply to that HTML:
//   - While the user is actively typing/formatting in THIS session, the contenteditable
//     element is the source of truth and is never rewritten mid-edit (that would reset the
//     cursor). Typing and execCommand can only ever produce the safe formatting tags below,
//     and paste/drop are intercepted to insert plain text only, so the live DOM stays safe
//     without per-keystroke sanitizing.
//   - Whenever stored Notes HTML is (re)installed into the DOM - initial load, or switching
//     back to a character - it may have come from a loaded file (possibly shared, possibly
//     hand-edited), so it's run through sanitizeHtml() first.

const ALLOWED_TAGS = new Set(["B", "STRONG", "I", "EM", "U", "UL", "OL", "LI", "BR", "DIV", "P", "SPAN"]);

// Their content is code, not prose - dropped along with the tag, not left behind as visible text.
const REMOVE_ENTIRELY = new Set(["SCRIPT", "STYLE"]);

function stripDisallowed(root) {
    const walker = document.createTreeWalker(root, NodeFilter.SHOW_ELEMENT);
    const toUnwrap = [];
    const toRemove = [];
    for (let node = walker.nextNode(); node; node = walker.nextNode()) {
        if (REMOVE_ENTIRELY.has(node.tagName)) {
            toRemove.push(node);
        } else if (!ALLOWED_TAGS.has(node.tagName)) {
            toUnwrap.push(node);
        } else {
            for (const attr of [...node.attributes]) {
                node.removeAttribute(attr.name);
            }
        }
    }
    for (const el of toRemove) {
        el.parentNode?.removeChild(el);
    }
    for (const el of toUnwrap) {
        while (el.firstChild) {
            el.parentNode.insertBefore(el.firstChild, el);
        }
        el.parentNode.removeChild(el);
    }
}

export function sanitizeHtml(html) {
    const temp = document.createElement("div");
    temp.innerHTML = html ?? "";
    stripDisallowed(temp);
    return temp.innerHTML;
}

export function sanitizeAndSetHtml(editorEl, html) {
    editorEl.innerHTML = sanitizeHtml(html);
}

export function getHtml(editorEl) {
    return editorEl.innerHTML;
}

function insertPlainText(e) {
    e.preventDefault();
    const text = (e.clipboardData || e.dataTransfer)?.getData("text/plain") ?? "";
    document.execCommand("insertText", false, text);
}

export function init(editorEl, toolbarEl) {
    const buttons = [...toolbarEl.querySelectorAll("[data-command]")];

    const updateToolbarState = () => {
        for (const btn of buttons) {
            let active = false;
            try {
                active = document.queryCommandState(btn.dataset.command);
            } catch {
                // Some commands don't support queryCommandState in every browser; leave inactive.
            }
            btn.classList.toggle("active", active);
        }
    };

    const onButtonMouseDown = (e) => e.preventDefault(); // keep the editor's selection focused
    const onButtonClick = (e) => {
        editorEl.focus();
        document.execCommand(e.currentTarget.dataset.command, false, null);
        updateToolbarState();
    };

    editorEl.addEventListener("keyup", updateToolbarState);
    editorEl.addEventListener("mouseup", updateToolbarState);
    editorEl.addEventListener("focus", updateToolbarState);
    editorEl.addEventListener("paste", insertPlainText);
    editorEl.addEventListener("drop", insertPlainText);

    for (const btn of buttons) {
        btn.addEventListener("mousedown", onButtonMouseDown);
        btn.addEventListener("click", onButtonClick);
    }

    return {
        dispose: () => {
            editorEl.removeEventListener("keyup", updateToolbarState);
            editorEl.removeEventListener("mouseup", updateToolbarState);
            editorEl.removeEventListener("focus", updateToolbarState);
            editorEl.removeEventListener("paste", insertPlainText);
            editorEl.removeEventListener("drop", insertPlainText);
            for (const btn of buttons) {
                btn.removeEventListener("mousedown", onButtonMouseDown);
                btn.removeEventListener("click", onButtonClick);
            }
        }
    };
}
