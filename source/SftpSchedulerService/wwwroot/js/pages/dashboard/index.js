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
        this.isLoading = false;
        this.loadStats();
    }
}).mount('#app-dashboard')