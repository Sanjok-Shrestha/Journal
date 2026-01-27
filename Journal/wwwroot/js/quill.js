window.initQuillEditor = function (elementId, dotNetRef) {
    try {
        const element = document.getElementById(elementId);
        if (!element) {
            console.error('Element not found:', elementId);
            return false;
        }

        // Check if Quill is loaded
        if (typeof Quill === 'undefined') {
            console.error('Quill is not loaded');
            return false;
        }

        // Initialize Quill with comprehensive toolbar
        window.quillEditor = new Quill('#' + elementId, {
            theme: 'snow',
            placeholder: 'Write your thoughts here...',
            modules: {
                toolbar: [
                    [{ 'header': [1, 2, 3, false] }],
                    ['bold', 'italic', 'underline', 'strike'],
                    [{ 'color': [] }, { 'background': [] }],
                    [{ 'list': 'ordered' }, { 'list': 'bullet' }],
                    [{ 'align': [] }],
                    ['blockquote', 'code-block'],
                    ['link', 'image'],
                    ['clean']
                ]
            }
        });

        // If DotNet reference provided, register change event to notify Blazor
        try {
            if (dotNetRef && typeof dotNetRef.invokeMethodAsync === 'function') {
                window.quillEditor.on('text-change', function () {
                    try {
                        dotNetRef.invokeMethodAsync('OnQuillTextChanged');
                    } catch (err) {
                        console.error('Error invoking .NET method from Quill text-change:', err);
                    }
                });
            }
        } catch (err) {
            console.warn('Failed to register text-change handler:', err);
        }

        console.log('Quill editor initialized successfully');
        return true;
    } catch (error) {
        console.error('Error initializing Quill:', error);
        return false;
    }
};

// Get HTML content from Quill editor
window.getQuillContent = function () {
    if (window.quillEditor) {
        return window.quillEditor.root.innerHTML;
    }
    return '';
};

// Set content in Quill editor
window.setQuillContent = function (content) {
    if (window.quillEditor && content) {
        window.quillEditor.root.innerHTML = content;
    }
};

// Get plain text from Quill editor
window.getQuillText = function () {
    if (window.quillEditor) {
        return window.quillEditor.getText();
    }
    return '';
};

// Get word count
window.getQuillWordCount = function () {
    if (window.quillEditor) {
        const text = window.quillEditor.getText().trim();
        if (!text) return 0;
        return text.split(/\s+/).filter(word => word.length > 0).length;
    }
    return 0;
};

// Download a file (used for export). filename: string, content: string, mimeType: string
window.downloadFile = function (filename, content, mimeType) {
    try {
        mimeType = mimeType || 'text/html';
        const blob = new Blob([content], { type: mimeType + ';charset=utf-8' });
        if (window.navigator && window.navigator.msSaveOrOpenBlob) {
            // IE/Edge
            window.navigator.msSaveOrOpenBlob(blob, filename);
            return;
        }
        const url = URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = filename;
        document.body.appendChild(a);
        a.click();
        setTimeout(function () {
            document.body.removeChild(a);
            window.URL.revokeObjectURL(url);
        }, 0);
    } catch (err) {
        console.error('downloadFile error:', err);
    }
};