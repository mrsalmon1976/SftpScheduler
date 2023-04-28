createApp({
    data() {
        return {
            hosts: [],
            isLoading: true,
            selectedHost: new HostModel()
        }
    },
    methods: {
        async deleteHost(hostIdHash) {
            var that = this;
            this.isLoading = true;

            if (this.selectedHost != null && hostIdHash.length > 0) {

                let result = await axios.delete('/api/hosts/' + hostIdHash)
                    .then(response => {
                        that.loadHosts();

                    })
                    .catch(err => {
                        UiHelpers.showErrorToast('Error', '', err.message);
                        that.isLoading = false;
                    })
                    .then(response => {
                        $('#modal-delete-host').modal('toggle');
                    });
            }
        },
        async loadHosts() {
            this.isLoading = true;

            let result = await axios.get('/api/hosts')
                .catch(err => {
                    UiHelpers.showErrorToast('Error', '', err.message);
                    this.isLoading = false;
                });

            this.hosts = result.data;
            this.isLoading = false;

        },
        showDeleteDialog(host) {

            if (host.jobCount > 0) {
                UiHelpers.showErrorToast('Error', '', 'You cannot a delete a host that has attached jobs');
                return;
            }

            this.selectedHost = host;
            $('#modal-delete-host').modal();

        }
    },
    mounted: function () {
        UiHelpers.setPageHeader('Hosts')
        this.isLoading = false;
        this.loadHosts();
    }
}).mount('#app-host')