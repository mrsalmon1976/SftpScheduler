const { createApp } = Vue

createApp({
    data() {
        return {
            allHosts: [],
            host: '',
            isLoading: true,
            schedule: '',
            scheduleInWords: 'No schedule entered',
            job: new JobModel()
        }
    },
    watch: {
        schedule: UiHelpers.debounce(function (text) {
            var that = this;
            this.job.schedule = this.schedule;
            if (this.schedule.trim() == '') {
                that.scheduleInWords = 'No schedule entered';
                return;
            }
            this.isLoading = true;
            this.scheduleInWords = 'Loading....';
            this.job.convertScheduleToWords(text, function (result) {
                that.scheduleInWords = result;
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

        },
        async submit() {
            this.isLoading = true;
            $('#toastsContainerTopRight > .toast').remove();

            // make sure all is valid
            if (!this.job.validate()) {
                this.isLoading = false;
                return;
            }

            axios.post('/api/jobs', this.job)

                .then(response => {
                    window.location.href = '/jobs';
                })
                .catch(err => {
                    if (err.response && err.response.data && err.response.data.errorMessages) {
                        var errMessages = err.response.data.errorMessages;
                        for (var i = 0; i < errMessages.length; i++) {
                            UiHelpers.showErrorToast('Validation Error', '', errMessages[i]);
                        }
                    }
                    else {
                        UiHelpers.showErrorToast('Validation Error', '', err.message);
                    }
                    this.isLoading = false;
                });
        }
    },
    mounted: function () {
        this.isLoading = false;
        this.loadHosts();
    }
}).mount('#app-job')