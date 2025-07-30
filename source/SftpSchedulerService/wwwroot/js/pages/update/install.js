createApp({
    data() {
        return {
            isUpdating: false,
            pageHeader: 'A new version of SftpScheduler is available for download.'
        }
    },
    methods: {
        checkVersion() {
            var isNewVersionAvailable = true;
            setInterval(function () {
                console.log('Checking for update...');
                axios.get('/api/update/check')
                    .then(response => {
                        isNewVersionAvailable = response.data.isNewVersionAvailable;
                        if (isNewVersionAvailable) {
                            console.log('Update still in progress, waiting 10 seconds....')
                        }
                        else {
                            window.location.href = '/';
                        }

                    })
                    .catch(err => {
                        console.log('SftpScheduler not available, waiting 10 seconds....')
                    });
                
            }, 5000);

        },
        initiateInstall() {
            axios.post('/api/update/install')
                .catch(err => {
                    UiHelpers.showErrorToast('Error', '', err.message);
                });
        },
        update() {
            this.isUpdating = true;
            this.pageHeader = 'Update in Progress';
            this.initiateInstall();
            this.checkVersion();
        }
    },
    mounted: function () {
        UiHelpers.setPageHeader('Updating SftpScheduler')
    }
}).mount('#app-update')