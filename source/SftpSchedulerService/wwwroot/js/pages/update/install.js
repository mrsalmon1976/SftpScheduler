﻿createApp({
    data() {
        return {
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

        }
    },
    mounted: function () {
        UiHelpers.setPageHeader('Updating SftpScheduler')
        this.checkVersion();
    }
}).mount('#app-update')