const { createApp } = Vue

const emitter = mitt();

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

var BusMessages = {};
Object.defineProperty(BusMessages, "PageHeader", {
    value: "PageHeader",
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

    static setPageHeader(headerText) {
        emitter.emit(BusMessages.PageHeader, headerText);
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
};

createApp({
    data() {
        return {
            pageHeader: 'Loading...'
        }
    },
    methods: {
    },
    mounted: function () {
        //emitter.emit(BusMessages.PageHeader, this.pad)
        //bus.$on(BusMessages.PageHeader, headerText => this.pageHeader = headerText);
        emitter.on(BusMessages.PageHeader, headerText => {
            this.pageHeader = headerText;
        });
    }
}).mount('#app-header')


