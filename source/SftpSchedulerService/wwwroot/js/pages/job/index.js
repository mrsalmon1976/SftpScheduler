const { createApp } = Vue

createApp({
    data() {
        return {
            jobs: [],
            isLoading: true,
            selectedJob: new JobModel()
        }
    },
    methods: {
        async deleteJob(jobIdHash) {
            var that = this;
            this.isLoading = true;

            if (this.selectedJob != null && jobIdHash.length > 0) {
                let result = await axios.delete('/api/jobs/' + jobIdHash)
                    .then(response => {
                        that.loadJobs();

                    })
                    .catch(err => {
                        UiHelpers.showErrorToast('Error', '', err.message);
                        that.isLoading = false;
                    })
                    .then(response => {
                        $('#modal-delete-job').modal('toggle');
                    });
            }

        },
        async loadJobs() {
            var that = this;
            this.isLoading = true;
            let result = await axios.get('/api/jobs')
                .catch(err => {
                    UiHelpers.showErrorToast('Error', '', err.message);
                    that.isLoading = false;
                });

            this.jobs = result.data;
            this.isLoading = false;

        },
        showDeleteDialog(job) {
            this.selectedJob = job;
            $('#modal-delete-job').modal();

        }
    },
    mounted: function () {
        this.isLoading = false;
        this.loadJobs();
    }
}).mount('#app-job')