const { createApp } = Vue

createApp({
    data() {
        return {
            isLoading: true,
            host: new HostModel(),
            scanDialogText: '',
            isScanError: false,
            keyFingerprints: []
        }
    },
    methods: {
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

            axios.post('/api/hosts', this.host)

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
        this.isLoading = false;
    }
}).mount('#app-host')