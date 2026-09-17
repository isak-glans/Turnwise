export function registerEscapeHandler(dotNetRef) {
    const handler = (e) => {
        if (e.key === "Escape") {
            dotNetRef.invokeMethodAsync("OnEscapePressed");
        }
    };

    document.addEventListener("keydown", handler);

    return {
        dispose: () => document.removeEventListener("keydown", handler)
    };
}
