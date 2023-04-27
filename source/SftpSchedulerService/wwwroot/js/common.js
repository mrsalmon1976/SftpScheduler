var JobTypes = {};
Object.defineProperty(JobTypes, "Download", {
    value: "1",
    writable: false,
    enumerable: true,
    configurable: false 
});
Object.defineProperty(JobTypes, "Upload", {
    value: "2",
    writable: false,
    enumerable: true,
    configurable: false
});

class UiHelpers
{
    static formatDateTime(dt) {
        var dt = moment(dt);
        if (dt.isValid()) {
            return dt.format('YYYY-MM-DD HH:mm:SS')
        }
        return '';
    };

    static showErrorToast(title, subtitle, message) {
        $(document).Toasts('create', {
            class: 'bg-danger',
            title: title,
            subtitle: subtitle,
            body: message
        });
    };

    static debounce(fn, delay) {
        var timeoutID = null
        return function () {
            clearTimeout(timeoutID)
            var args = arguments
            var that = this
            timeoutID = setTimeout(function () {
                fn.apply(that, args)
            }, delay)
        }
    };
}


