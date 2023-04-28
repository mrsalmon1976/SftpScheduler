createApp({
    data() {
        return {
            isLoading: true,
            isEdit: false,
            hostId: '',
            host: new HostModel(),
            scanDialogText: '',
            isScanError: false,
            keyFingerprints: [],
            submitButtonText: 'Create Host'
        }
    },
    methods: {
        async loadHost() {

            let result = await axios.get('/api/hosts/' + this.host.hashId)
                .catch(err => {
                    this.isLoading = false;
                    UiHelpers.showErrorToast('Error', '', err.message);
                    return;
                });


            var hostData = result.data;
            UiHelpers.setPageHeader('Edit Host / ' + hostData.name + ' [' + hostData.host + ']');
            this.host.name = hostData.name;
            this.host.host = hostData.host;
            this.host.port = hostData.port;
            this.host.userName = hostData.userName;
            this.host.password = '';
            this.host.keyFingerprint = hostData.keyFingerprint;
            this.host.validate();
            this.isLoading = false;
        },
        async showScanDialog() {

            this.isScanError = false;
            this.keyFingerprints = [];
            this.scanDialogText = 'Scanning host for fingerprint values using SHA-256 and MD5 algorithms...';
            $('#modal-scan-fingerprint').modal();

            if (!this.host.validate())
            { 
                this.isScanError = true;
                this.scanDialogText = 'The captured host details are invalid - unable to scan for fingerprint.';
                return;
            }


            axios.post('/api/hosts/scanfingerprint', this.host)

                .then(response => {
                    this.scanDialogText = '';
                    this.keyFingerprints = response.data;
                })
                .catch(err => {
                    this.isScanError = true;
                    if (err.response && err.response.data && err.response.data.errorMessages) {
                        var errMessages = err.response.data.errorMessages;
                        var text = '';
                        for (var i = 0; i < errMessages.length; i++) {
                            text = text + errMessages[i] + '<br />';
                        }
                        this.scanDialogText = text;
                    }
                    else {
                        this.scanDialogText = err.message;
                    }
                });
        },
        setKeyFingerPrint(keyFingerprint) {
            this.host.keyFingerprint = keyFingerprint;
            $('#modal-scan-fingerprint').modal('toggle');
        },
        async submit() {
            this.isLoading = true;
            $('#toastsContainerTopRight > .toast').remove();

            // make sure all is valid
            if (!this.host.validate()) {
                this.isLoading = false;
                return;
            }

            var url = (this.isEdit ? '/api/hosts/' + this.host.hashId : '/api/hosts');
            axios.post(url, this.host)

                .then(response => {
                    window.location.href = '/hosts';
                })
                .catch(err => {
                    if (err.response && err.response.data && err.response.data.errorMessages) {
                        var errMessages = err.response.data.errorMessages;
                        for (var i = 0; i < errMessages.length; i++) {
                            UiHelpers.showErrorToast('Validation Error', '', errMessages[i]);
                        }
                    }
                    else {
                        UiHelpers.showErrorToast('Validation Error', '', err.message);
                    }
                    this.isLoading = false;
                });

        }
    },
    mounted: function () {
        this.host.hashId = this.$el.parentElement.getAttribute('data-host-id');
        if (this.host.hashId != '') {
            this.isEdit = true;
            this.submitButtonText = 'Update Host';
            this.loadHost();
        }
        else {
            this.isLoading = false;
            UiHelpers.setPageHeader('Create New Host');
        }
    }
}).mount('#app-host')