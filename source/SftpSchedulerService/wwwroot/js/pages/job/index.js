﻿createApp({
    data() {
        return {
            filter: '',
            filteredJobs: [],
            jobs: [],
            isLoading: true,
            selectedJob: new JobModel(),
            JOBTYPE_DOWNLOAD: JobTypes.Download,
            JOBTYPE_UPLOAD: JobTypes.Upload
        }
    },
    watch: {
        filter: UiHelpers.debounce(function (text) {
            this.updateFilter();
        }, 100)
    },
    methods: {
        async deleteJob(jobIdHash) {
            var that = this;
            this.isLoading = true;

            if (this.selectedJob != null && jobIdHash.length > 0) {
                $('#modal-delete-job').modal('toggle');
                let result = await axios.delete('/api/jobs/' + jobIdHash)
                    .then(response => {
                        that.loadJobs();
                        UiHelpers.showSuccessToast('Delete Job', '', 'Job successfully deleted');
                    })
                    .catch(err => {
                        UiHelpers.showErrorToast('Error', '', 'Failed to delete job - please check error logs for details.');
                        that.isLoading = false;
                    });
            }

        },
        async executeJob(job) {

            await axios.post('/api/jobs/' + job.hashId + '/run', job)
                .then(response => {
                    UiHelpers.showSuccessToast('Run Job', '', 'Job ' + job.name + ' has been scheduled for execution');
                })
                .catch(err => {
                    if (err.response && err.response.status == 400) {
                        UiHelpers.showWarningToast('Error', '', err.response.data)
                    }
                    else {
                        UiHelpers.showErrorToast('Error', '', err.message);
                    }
                });

        },
        formatDateTime(dt) {
            return UiHelpers.formatDateTime(dt);
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
            this.updateFilter();
            this.isLoading = false;

        },
        showDeleteDialog(job) {
            this.selectedJob = job;
            $('#modal-delete-job').modal();

        },
        async toggleJob(job, isEnabled) {
            var that = this;
            job.isEnabled = isEnabled;
            this.isLoading = true;
            await axios.post('/api/jobs/' + job.hashId, job)
                .then(response => {
                    that.loadJobs();
                    var msg = '\'' + job.name + '\' successfully ' + (isEnabled ? 'enabled' : 'disabled');
                    if (isEnabled) {
                        UiHelpers.showSuccessToast('Toggle Job', '', msg);
                    }
                    else {
                        UiHelpers.showWarningToast('Toggle Job', '', msg);
                    }
                })
                .catch(err => {
                    UiHelpers.showErrorToast('Error', '', err.message);
                    that.isLoading = false;
                });

        },
        updateFilter() {

            if (this.filter.length == 0) {
                this.filteredJobs = this.jobs;
            }
            else {
                var that = this;
                this.filteredJobs = this.jobs.filter((el) => el.name.toLowerCase().includes(that.filter.toLowerCase()));
            }
        }
    },
    mounted: function () {
        this.isLoading = false;
        UiHelpers.setPageHeader('Jobs');
        this.loadJobs();
    }
}).mount('#app-job')