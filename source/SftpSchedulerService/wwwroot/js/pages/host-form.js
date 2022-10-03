const { createApp } = Vue

class HostModel {
    constructor() {
        this.name = '';
        this.host = '';
        this.port = 22;
        this.userName = '';
        this.password = '';

        // these default to true, as you don't want the screen showing everything is invalid at first pass
        this.isHostValid = true;
        this.isNameValid = true;
    }

    validate = function () {
        this.validateName();
        this.validateHost();
        return (this.isHostValid && this.isNameValid);
    }

    validateHost = function () {
        this.isHostValid = (this.host.length >= 5);
        return this.isHostValid;
    }

    validateName = function () {
        this.isNameValid = (this.name.length >= 5);
        return this.isNameValid;
    }

}

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