﻿createApp({
    data() {
        return {
            isLoading: true,
            jobTotalCount: 'Loading...',
            jobFailingCount: 'Loading...',
            jobFailingPercentage: 0,
            jobFailingClass: 'bg-info'
        }
    },
    methods: {
        calculatePercentage(divident, divisor) {
            var dividentNum = parseInt(divident);
            var divisorNum = parseInt(divisor);
            var quotient = dividentNum / divisorNum * 100;
            return parseInt(quotient);
        },
        async loadStats() {
            this.isLoading = true;

            let result = await axios.get('/api/dashboard/stats')
                .catch(err => {
                    UiHelpers.showErrorToast('Error', '', err.message);
                    this.isLoading = false;
                });

            this.jobTotalCount = result.data.jobTotalCount;
            this.jobFailingCount = result.data.jobFailingCount;
            this.jobFailingPercentage = this.calculatePercentage(this.jobFailingCount, this.jobTotalCount);
            this.jobFailingClass = (this.jobFailingPercentage == 0 ? 'bg-success' : 'bg-danger');
            this.isLoading = false;

        }
    },
    mounted: function () {
        UiHelpers.setPageHeader('Dashboard')
        this.isLoading = false;
        this.loadStats();
    }
}).mount('#app-dashboard')