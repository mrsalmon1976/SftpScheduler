const { createApp } = Vue

createApp({
    data() {
        return {
            allHosts: [],
            host: '',
            isLoading: true,
            scheduleInWords: 'No schedule entered',
            schedule: '',
            isScheduleValid: false
        }
    },
    watch: {
        schedule: UiHelpers.debounce(function (text) {
            var that = this;
            this.isLoading = true;
            this.scheduleInWords = 'Loading....';
            if (this.schedule.trim() == '') {
                that.scheduleInWords = 'No schedule entered';
                return;
            }
            axios.get('/api/cron', { params: { schedule: text } })
                .then(response => {
                    that.isScheduleValid = response.data.isValid;
                    if (that.isScheduleValid) {
                        that.scheduleInWords = response.data.scheduleInWords;
                    }
                    else {
                        that.scheduleInWords = response.data.errorMessage;
                    }
                })
                .catch(err => {
                    that.isScheduleValid = false;
                    that.scheduleInWords = 'An error occurred retrieving the cron description - check application logs for details';
                })
                .then(() => {
                    that.isLoading = false;
                });
        }, 500)
    },
    methods: {
        async loadHosts() {
            this.isLoading = true;

            let result = await axios.get('/api/hosts')
                .catch(err => {
                    UiHelpers.showErrorToast('Error', '', err.message);
                    this.isLoading = false;
                });

            this.allHosts = result.data;
            this.isLoading = false;

        }
    },
    mounted: function () {
        this.isLoading = false;
        this.loadHosts();
    }
}).mount('#app-job')