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

    static showSuccessToast(title, subtitle, message) {
        $(document).Toasts('create', {
            class: 'bg-success',
            title: title,
            subtitle: subtitle,
            body: message,
            autohide: true,
            delay: 2000
        });
    };

    static showWarningToast(title, subtitle, message) {
        $(document).Toasts('create', {
            class: 'bg-warning',
            title: title,
            subtitle: subtitle,
            body: message,
            autohide: true,
            delay: 2000
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

// header island
createApp({
    data() {
        return {
            pageHeader: 'Loading...'
        }
    },
    methods: {
    },
    mounted: function () {
        emitter.on(BusMessages.PageHeader, headerText => {
            this.pageHeader = headerText;
        });
    }
}).mount('#app-header')


// menu island
createApp({
    data() {
        return {
            jobNotificationBadgeClass: 'badge-success',
            jobNotifications: [],
        }
    },
    methods: {
        async loadJobNotifications(forceReload) {
            var url = '/api/notifications/jobs';
            if (forceReload) {
                url = url + '?forceReload=true'
            }

            let result = await axios.get(url)
                .catch(err => {
                    UiHelpers.showErrorToast('Error', '', err.message);
                    return;
                });

            this.jobNotifications = this.resetJobNotificationStyles(result.data);
        },
        resetJobNotificationStyles(notifications) {
            var badgeClass = '';
            for (var i = 0; i < notifications.length; i++) {

                var jn = notifications[i];

                if (jn.notificationType == 'Error') {
                    if (badgeClass.length == 0) badgeClass = 'badge-danger';
                    jn.textClass = 'text-danger';
                    jn.text = '[' + jn.jobName + '] failed on it\'s last execution';
                }
                else if (jn.notificationType == 'Warning') {
                    if (badgeClass.length == 0) badgeClass = 'badge-warning';
                    jn.textClass = 'text-warning';
                    jn.text = '[' + jn.jobName + '] failed in the last 24 hours';
                }
            }
            if (badgeClass.length == 0) badgeClass = 'badge-success';
            this.jobNotificationBadgeClass = badgeClass;
            return notifications;
        }
    },
    mounted: function () {
        this.loadJobNotifications(false);
    }
}).mount('#app-menu')

