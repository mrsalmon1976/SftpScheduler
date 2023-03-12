const { createApp } = Vue

createApp({
    data() {
        return {
            jobs: [],
            isLoading: true
        }
    },
    methods: {
        async loadJobs() {
            this.isLoading = true;

            let result = await axios.get('/api/jobs')
                .catch(err => {
                    UiHelpers.showErrorToast('Error', '', err.message);
                    this.isLoading = false;
                });

            this.jobs = result.data;
            this.isLoading = false;

        }
    },
    mounted: function () {
        this.isLoading = false;
        this.loadJobs();
    }
}).mount('#app-job')