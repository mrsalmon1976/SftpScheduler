const { createApp } = Vue

createApp({
    data() {
        return {
            hosts: [],
            isLoading: true
        }
    },
    methods: {
        async loadHosts() {
            this.isLoading = true;

            let result = await axios.get('/api/hosts')
                .catch(err => {
                    UiHelpers.showErrorToast('Error', '', err.message);
                    this.isLoading = false;
                });

            this.hosts = result.data;
            this.isLoading = false;

        }
    },
    mounted: function () {
        this.isLoading = false;
        this.loadHosts();
    }
}).mount('#app-host')