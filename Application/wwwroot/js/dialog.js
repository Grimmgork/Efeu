htmx.defineExtension('dialog', {
    onEvent: function (name, evt) {

        if (name === "htmx:beforeSwap") {
            let elt = evt.target.closest("[hx-dialog]");
            if (!elt) return;
            evt.preventDefault();
        }

        if (name === "htmx:beforeSwap") {
            let elt = evt.target;

            // Only care about responses that might contain dialogs
            let xhr = evt.detail.xhr;
            let html = xhr.responseText;

            if (!html || !html.includes("<dialog")) return;

            // Parse response into a document fragment
            let parser = new DOMParser();
            let doc = parser.parseFromString(html, "text/html");

            let dialogs = doc.querySelectorAll("dialog[hx-dialog]");

            dialogs.forEach(dialog => {
                // Remove from response
                dialog.remove();

                // Move to body
                document.body.appendChild(dialog);
                htmx.process(dialog);

                // Open it
                dialog.showModal();

                // Optional: cleanup on close
                dialog.addEventListener("close", () => {
                    dialog.remove();
                });
            });

            // Replace response content WITHOUT dialogs
            evt.detail.serverResponse = doc.body.innerHTML;
        }
    }
});

htmx.defineExtension("dialog-close", {
    onEvent: function (name, evt) {

        // Handle clicks on buttons with hx-dialog-close
        if (name === "htmx:load" || name === "htmx:afterSwap") {
            // Attach delegated listener once per page load or swap
            if (!document.body._dialogCloseAttached) {
                document.body.addEventListener("click", function (clickEvt) {
                    let button = clickEvt.target.closest("[hx-dialog-close]");
                    if (!button) return;

                    // Trigger the existing close-dialog event on the button
                    htmx.trigger(button, "close-dialog");
                });
                document.body._dialogCloseAttached = true;
            }
        }

        // Optional: handle server-triggered close-dialog events (no id = nearest dialog)
        if (name === "close-dialog") {
            let dialog;
            if (evt.detail && evt.detail.id) {
                dialog = document.getElementById(evt.detail.id);
            } else {
                dialog = evt.target.closest("dialog");
            }

            if (dialog) {
                dialog.close();
            }
        }
    }
});
