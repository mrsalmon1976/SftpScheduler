createApp({
    data() {
        return {
            isLoading: true,
            isSmtpTabSelected: false,
            settings: new SettingsModel(),
            testEmailToAddress: '',
            testEmailSubject: 'SftpScheduler Test Email',
            testEmailBody: 'This is a test email from SftpScheduler.',
        }
    },
    methods: {
        async loadSettings() {
            let result = await axios.get('/api/settings')
                .catch(err => {
                    UiHelpers.showErrorToast('Error', '', err.message);
                    this.isLoading = false;
                });

            this.settings.digestDays = result.data.digestDays;
            this.settings.digestTime = result.data.digestTime;
            this.settings.maxConcurrentJobs = result.data.maxConcurrentJobs;
            this.settings.smtpHost = result.data.smtpHost;
            this.settings.smtpPort = result.data.smtpPort;
            this.settings.smtpUserName = result.data.smtpUserName;
            this.settings.smtpFromName = result.data.smtpFromName;
            this.settings.smtpFromEmail = result.data.smtpFromEmail;
            this.settings.smtpEnableSsl = result.data.smtpEnableSsl;
            this.isLoading = false;
        },
        openTestEmailDialog() {
            $('#modal-test-email').modal();
        },
        async sendTestEmail() {
            var emailData = {
                host: this.settings.smtpHost,
                port: this.settings.smtpPort,
                userName: this.settings.smtpUserName,
                password: this.settings.smtpPassword,
                fromAddress: this.settings.smtpFromEmail,
                enableSsl: this.settings.smtpEnableSsl,
                toAddress: this.testEmailToAddress,
                subject: this.testEmailSubject,
                body: this.testEmailBody
            };
            var url = '/api/settings/email-test';
            await axios.post(url, emailData)
                .then(response => {
                    UiHelpers.showSuccessToast('Test email successfully sent');
                })
                .catch(err => {
                    if (err.response && err.response.data && err.response.data.errorMessages) {
                        var errMessages = err.response.data.errorMessages;
                        for (var i = 0; i < errMessages.length; i++) {
                            UiHelpers.showErrorToast('Validation Error', '', errMessages[i]);
                        }
                    }
                    else {
                        UiHelpers.showErrorToast('Unexpected Error', '', err.message);
                    }
                });
        },
        setLogReloadInterval() {
            var that = this;
            setInterval(function () {
                that.loadLogs();
            }, 60000);
        },
        async submit() {
            this.isLoading = true;
            $('#toastsContainerTopRight > .toast').remove();

            // make sure all is valid
            if (!this.settings.validate()) {
                this.isLoading = false;
                UiHelpers.showErrorToast('Validation Failure', '', 'Settings missing or invalid.', 10000);
                return;
            }

            var url = '/api/settings';
            await axios.post(url, this.settings)

                .then(response => {
                    UiHelpers.showSuccessToast('Global settings successfully updated');
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
                });
            this.isLoading = false;
        }
    },
    mounted: function () {
        this.isLoading = true;
        UiHelpers.setPageHeader('Global Settings');
        this.loadSettings();

        var that = this;
        $(document).on('shown.bs.tab', function (e)
        {
            that.isSmtpTabSelected = (e.target.innerText == 'SMTP');
        });
    }
}).mount('#app-settings')