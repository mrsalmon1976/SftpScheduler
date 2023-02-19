const { createApp } = Vue

createApp({
    data() {
        return {
            isLoading: true,
            host: new HostModel()
        }
    },
    methods: {
        async submit() {
            this.isLoading = true;
            $('#toastsContainerTopRight > .toast').remove();

            // make sure all is valid
            if (!this.host.validate()) {
                this.isLoading = false;
                return;
            }

            axios.post('/api/host', this.host)

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