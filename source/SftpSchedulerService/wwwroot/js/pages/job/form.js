const { createApp } = Vue

createApp({
    data() {
        return {
            allHosts: [],
            host: '',
            isLoading: true,
            schedule: '',
            scheduleInWords: 'No schedule entered',
            job: new JobModel(),
            logs: []
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
            this.scheduleInWords = 'Loading....';
            this.job.convertScheduleToWords(text, function (result) {
                that.scheduleInWords = result;
            });
            
        }, 500)
    },
    methods: {
        formatDateTime(dt) {
            return UiHelpers.formatDateTime(dt);
        },
        getLogClass(log) {
            if (log.status == 'Failure') {
                return 'bg-danger';
            }
            else if (log.status == 'Success') {
                return 'bg-success';
            }
            return 'bg-primary';
        },
        isDownloadVisible() {
            return (this.job.type == JobTypes.Download);
        },
        async loadHosts() {
            let result = await axios.get('/api/hosts')
                .catch(err => {
                    UiHelpers.showErrorToast('Error', '', err.message);
                    this.isLoading = false;
                });

            this.allHosts = result.data;
        },
        async loadJobDetail() {

            let result = await axios.get('/api/jobs/' + this.job.hashId)
                .catch(err => {
                    UiHelpers.showErrorToast('Error', '', err.message);
                    return;
                });

            var jobData = result.data;
            this.job.name = jobData.name;
            this.job.hostId = jobData.hostId;
            this.job.type = jobData.type;
            this.job.localPath = jobData.localPath;
            this.job.remotePath = jobData.remotePath;
            this.job.deleteAfterDownload = jobData.deleteAfterDownload;
            this.job.remoteArchivePath = jobData.remoteArchivePath;
            this.job.localCopyPaths = jobData.localCopyPaths;
            this.job.validate();

            this.schedule = jobData.schedule;

        },
        async loadLogs() {
            let result = await axios.get('/api/jobs/' + this.job.hashId + '/logs')
                .catch(err => {
                    UiHelpers.showErrorToast('Error', '', err.message);
                });

            this.logs = result.data;
        },
        async submit() {
            this.isLoading = true;
            $('#toastsContainerTopRight > .toast').remove();

            // make sure all is valid
            if (!this.job.validate()) {
                this.isLoading = false;
                return;
            }

            await axios.post('/api/jobs', this.job)

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
        this.isLoading = true;
        this.loadHosts();

        this.job.hashId = this.$el.parentElement.getAttribute('data-job-id');
        if (this.job.hashId != '') {
            this.loadJobDetail();
            this.loadLogs();
        }
        this.isLoading = false;
    }
}).mount('#app-job')