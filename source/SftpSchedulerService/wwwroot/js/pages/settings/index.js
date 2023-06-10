createApp({
    data() {
        return {
            isLoading: true,
            settings: new SettingsModel(),
        }
    },
    methods: {
        async loadSettings() {
            let result = await axios.get('/api/settings')
                .catch(err => {
                    UiHelpers.showErrorToast('Error', '', err.message);
                    this.isLoading = false;
                });

            this.settings.maxConcurrentJobs = result.data.maxConcurrentJobs;
            this.isLoading = false;
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
    }
}).mount('#app-settings')