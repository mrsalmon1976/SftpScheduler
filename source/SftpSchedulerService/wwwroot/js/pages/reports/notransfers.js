createApp({
    data() {
        return {
            jobs: [],
            isLoading: true,
            JOBTYPE_DOWNLOAD: JobTypes.Download,
            JOBTYPE_UPLOAD: JobTypes.Upload,
            endDate: null,
            startDate: null
        }
    },
    methods: {

        initialiseDataRangePicker() {
            var that = this;
            this.startDate = moment().add(-3, 'month');
            this.endDate = moment();
            $('input[name="daterange"]').daterangepicker({
                opens: 'left',
                startDate: this.startDate,
                endDate: this.endDate,
                locale: {
                    format: 'YYYY/MM/DD'
                }
            }, function (start, end, label) {
                that.startDate = start;
                that.endDate = end;
                that.loadJobs();
            });
        },
        async loadJobs() {
            var that = this;
            this.isLoading = true;
            var s = this.startDate.format('YYYY-MM-DD');
            var e = this.endDate.format('YYYY-MM-DD');

            let result = await axios.get('/api/reports/notransfers?startDate=' + s + '&endDate=' + e)
                .catch(err => {
                    UiHelpers.showErrorToast('Error', '', err.message);
                    that.isLoading = false;
                });

            this.jobs = result.data;
            this.isLoading = false;

        },
    },
    mounted: function () {
        this.isLoading = false;
        UiHelpers.setPageHeader('Reports / Jobs without transfers');
        this.initialiseDataRangePicker();
        this.loadJobs();
    }
}).mount('#app-reports-notransfers')