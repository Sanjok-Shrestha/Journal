window.QuillEditor = {
    editors: {},

    initialize: function (editorId, content, placeholder, minHeight, dotNetRef) {
        try {
            const toolbarId = `toolbar-${editorId}`;
            const editorElementId = `editor-${editorId}`;

            const quill = new Quill(`#${editorElementId}`, {
                theme: 'snow',
                placeholder: placeholder,
                modules: {
                    toolbar: `#${toolbarId}`
                }
            });

            // Set minimum height
            const editorElement = document.querySelector(`#${editorElementId} .ql-editor`);
            if (editorElement) {
                editorElement.style.minHeight = `${minHeight}px`;
            }

            // Set initial content
            if (content) {
                quill.root.innerHTML = content;
            }

            // Listen for text changes
            quill.on('text-change', function () {
                const html = quill.root.innerHTML;
                dotNetRef.invokeMethodAsync('OnContentChanged', html);
            });

            this.editors[editorId] = quill;
            return true;
        } catch (error) {
            console.error('Error initializing Quill editor:', error);
            return false;
        }
    },

    getContent: function (editorId) {
        const quill = this.editors[editorId];
        if (quill) {
            return quill.root.innerHTML;
        }
        return '';
    },

    setContent: function (editorId, content) {
        const quill = this.editors[editorId];
        if (quill) {
            quill.root.innerHTML = content;
        }
    },

    dispose: function (editorId) {
        if (this.editors[editorId]) {
            delete this.editors[editorId];
        }
    }
};