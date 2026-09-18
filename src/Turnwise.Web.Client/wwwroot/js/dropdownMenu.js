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
