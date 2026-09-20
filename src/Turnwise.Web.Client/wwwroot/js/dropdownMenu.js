export function positionMenu(triggerId, menuEl) {
    const trigger = document.getElementById(triggerId);
    if (!trigger || !menuEl) {
        return;
    }

    const triggerRect = trigger.getBoundingClientRect();
    const menuRect = menuEl.getBoundingClientRect();
    const viewportHeight = document.documentElement.clientHeight;
    const viewportWidth = document.documentElement.clientWidth;

    let top = triggerRect.bottom + 4;
    if (top + menuRect.height > viewportHeight && triggerRect.top - menuRect.height - 4 >= 0) {
        top = triggerRect.top - menuRect.height - 4;
    }

    let left = triggerRect.right - menuRect.width;
    if (left < 4) {
        left = 4;
    }
    if (left + menuRect.width > viewportWidth) {
        left = Math.max(4, viewportWidth - menuRect.width - 4);
    }

    menuEl.style.position = "fixed";
    menuEl.style.top = `${top}px`;
    menuEl.style.left = `${left}px`;
    menuEl.style.right = "auto";
}

/// Closes a dropdown menu when the user clicks anywhere outside it (and outside its trigger
/// button, so the trigger's own click handler is the only thing that re-toggles it). Uses
/// "mousedown" rather than "click" so this fires - and can call back into .NET - before the
/// browser's later "click" event reaches the trigger button's own Blazor handler.
export function registerOutsideClick(triggerId, menuEl, dotNetRef) {
    const handler = (event) => {
        const trigger = document.getElementById(triggerId);
        if (menuEl.contains(event.target) || (trigger && trigger.contains(event.target))) {
            return;
        }
        dotNetRef.invokeMethodAsync("OnClickOutsideMenu");
    };

    document.addEventListener("mousedown", handler, true);

    return {
        dispose: () => document.removeEventListener("mousedown", handler, true)
    };
}
