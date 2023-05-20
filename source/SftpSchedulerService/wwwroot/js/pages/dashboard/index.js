createApp({
    data() {
        return {
            isLoading: true,
            isNewVersionAvailable: false,
            latestVersion: '0.0'
        }
    },
    methods: {
        async checkVersion() {
            let result = await axios.get('/api/update/check')
                .catch(err => {
                    console.log(err.message);
                });

            this.isNewVersionAvailable = result.data.isNewVersionAvailable;
            this.latestVersion = result.data.latestReleaseVersionNumber;
        },
        async loadStats() {
            this.isLoading = true;

            let result = await axios.get('/api/hosts')
                .catch(err => {
                    UiHelpers.showErrorToast('Error', '', err.message);
                    this.isLoading = false;
                });

            this.isLoading = false;

        }
    },
    mounted: function () {
        UiHelpers.setPageHeader('Dashboard')
        this.isLoading = false;
        this.loadStats();
        this.checkVersion();
    }
}).mount('#app-dashboard')