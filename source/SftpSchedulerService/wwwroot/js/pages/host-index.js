const { createApp } = Vue

createApp({
    data() {
        return {
            isLoading: true
        }
    },
    methods: {
        login() {
        }
    },
    mounted: function () {
        this.isLoading = false;
    }
}).mount('#app-host')