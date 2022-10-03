class UiHelpers
{
    static showErrorToast(title, subtitle, message) {
        $(document).Toasts('create', {
            class: 'bg-danger',
            title: title,
            subtitle: subtitle,
            body: message
        });
    };
}