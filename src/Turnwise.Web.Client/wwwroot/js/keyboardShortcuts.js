export function registerKeyboardShortcuts(dotNetRef) {
    const handler = (e) => {
        if (e.key === "Escape") {
            dotNetRef.invokeMethodAsync("OnEscapePressed");
            return;
        }

        const isUndoCombo = (e.ctrlKey || e.metaKey) && !e.shiftKey && e.key.toLowerCase() === "z";
        if (isUndoCombo) {
            // Let a focused text field handle its own native undo instead of stomping on it.
            const tag = document.activeElement?.tagName;
            if (tag !== "INPUT" && tag !== "TEXTAREA" && tag !== "SELECT") {
                e.preventDefault();
                dotNetRef.invokeMethodAsync("OnUndoShortcut");
            }
        }
    };

    document.addEventListener("keydown", handler);

    return {
        dispose: () => document.removeEventListener("keydown", handler)
    };
}
