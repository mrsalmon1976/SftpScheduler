createApp({
    data() {
        return {
            allHosts: [],
            host: '',
            isEdit: false,
            isLoading: true,
            localPrefix: '',
            localPrefixInWords: 'No local prefix entered.',
            schedule: '',
            scheduleInWords: 'No schedule entered',
            job: new JobModel(),
            auditLogs: [],
            logs: [],
            fileLogs: [],
            submitButtonText: 'Create Job'
        }
    },
    watch: {
        localPrefix: UiHelpers.debounce(function (text) {
            this.updateLocalPrefixText();
        }, 500),
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
        async executeJob() {

            await axios.post('/api/jobs/' + this.job.hashId + '/run', this.job)
                .then(response => {
                    UiHelpers.showSuccessToast('Run Job', '', 'Job ' + this.job.name + ' has been scheduled for execution');
                })
                .catch(err => {
                    if (err.response && err.response.status == 400) {
                        UiHelpers.showWarningToast('Error', '', err.response.data)
                    }
                    else {
                        UiHelpers.showErrorToast('Error', '', err.message);
                    }
                });

        },
        formatDateTime(dt) {
            return UiHelpers.formatDateTime(dt);
        },
        getLogClass(log) {
            if (log.status == 'Failed') {
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
        isUploadVisible() {
            return (this.job.type == JobTypes.Upload);
        },
        async loadAuditLogs() {

            let result = await axios.get('/api/jobs/' + this.job.hashId + '/auditlogs')
                .catch(err => {
                    UiHelpers.showErrorToast('Error', '', err.message);
                });

            this.auditLogs = result.data;
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
            UiHelpers.setPageHeader('Edit Job / ' + jobData.name);
            this.job.name = jobData.name;
            this.job.hostId = jobData.hostId;
            this.job.type = jobData.type;
            this.job.localPath = jobData.localPath;
            this.job.localPrefix = jobData.localPrefix;
            this.localPrefix = jobData.localPrefix;
            this.job.localArchivePath = jobData.localArchivePath;
            this.job.remotePath = jobData.remotePath;
            this.job.deleteAfterDownload = jobData.deleteAfterDownload;
            this.job.remoteArchivePath = jobData.remoteArchivePath;
            this.job.localCopyPaths = jobData.localCopyPaths;
            this.job.isEnabled = jobData.isEnabled;
            this.job.restartOnFailure = jobData.restartOnFailure;
            this.job.compressionMode = jobData.compressionMode;
            this.job.fileMask = jobData.fileMask;
            this.job.preserveTimestamp = jobData.preserveTimestamp;
            this.job.transferMode = jobData.transferMode;

            this.job.validate();

            this.schedule = jobData.schedule;
            Vue.nextTick(function () {
                $('[data-toggle="tooltip"]').tooltip();
            })

        },
        async loadFileLogs() {

            let result = await axios.get('/api/jobs/' + this.job.hashId + '/filelogs')
                .catch(err => {
                    UiHelpers.showErrorToast('Error', '', err.message);
                });

            this.fileLogs = result.data;
        },
        async loadLogs() {

            let result = await axios.get('/api/jobs/' + this.job.hashId + '/logs')
                .catch(err => {
                    UiHelpers.showErrorToast('Error', '', err.message);
                });

            this.logs = result.data;
        },
        onTypeChange() {
            $('[data-toggle="tooltip"]').tooltip();
        },
        setLogReloadInterval() {
            var that = this;
            setInterval(function () {
                that.loadLogs();
                that.loadFileLogs();
            }, 60000);
        },
        async submit() {
            this.isLoading = true;
            $('#toastsContainerTopRight > .toast').remove();

            // make sure all is valid
            if (!this.job.validate()) {
                this.isLoading = false;
                UiHelpers.showErrorToast('Validation Failure', '', 'Job properties missing or incorrect.', 10000);
                return;
            }

            var url = (this.isEdit ? '/api/jobs/' + this.job.hashId : '/api/jobs');
            await axios.post(url, this.job)

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
        },
        async updateLocalPrefixText() {
            this.job.localPrefix = this.localPrefix;
            if (this.localPrefix.trim() == '') {
                this.localPrefixInWords = 'No local prefix entered';
                return;
            }
            let prefixWords = this.localPrefix;
            const currentYear = moment().format('YYYY');
            const currentMonth = moment().format('MM');
            const currentDay = moment().format('DD');
            const currentHour = moment().format('HH');
            const currentMinute = moment().format('mm');
            const currentSecond = moment().format('ss');
            prefixWords = prefixWords.replace(/\{year\}/gi, currentYear);
            prefixWords = prefixWords.replace(/\{month\}/gi, currentMonth);
            prefixWords = prefixWords.replace(/\{day\}/gi, currentDay);
            prefixWords = prefixWords.replace(/\{hour\}/gi, currentHour);
            prefixWords = prefixWords.replace(/\{minute\}/gi, currentMinute);
            prefixWords = prefixWords.replace(/\{second\}/gi, currentSecond);
            this.localPrefixInWords = 'Files will be prefixed with the value \'' + prefixWords + '\'';
        }
    },
    mounted: function () {
        this.isLoading = true;
        this.loadHosts();
        this.job.hashId = this.$el.parentElement.getAttribute('data-job-id');
        if (this.job.hashId == '') {
            UiHelpers.setPageHeader('Create New Job');
            Vue.nextTick(function () {
                $('[data-toggle="tooltip"]').tooltip();
            })

        }
        else { 
            this.isEdit = true;
            this.submitButtonText = 'Update Job';
            this.loadJobDetail();
            this.loadLogs();
            this.loadFileLogs();
            this.loadAuditLogs();
            this.setLogReloadInterval();
        }
        this.updateLocalPrefixText();
        this.isLoading = false;

    }
}).mount('#app-job')