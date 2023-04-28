createApp({
    data() {
        return {
            isLoading: true
        }
    },
    methods: {
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
    }
}).mount('#app-dashboard')