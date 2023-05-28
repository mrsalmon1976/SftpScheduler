createApp({
    data() {
        return {
            userName: '',
            password: '',
            isLoading: true
        }
    },
    methods: {
        async login() {

            if (this.userName.length == 0 || this.password.length == 0) {
                UiHelpers.showErrorToast('Validation Failure', '', 'Missing user name and/or password');
                return;
            }

            var that = this;
            this.isLoading = true;
            let payload = { userName: this.userName, password: this.password };

            let res = await axios.post('/api/auth/login', payload)
                .then(function (response) {
                    window.location.href = '/dashboard';
                })
                .catch(function (error) {
                    if (error.request.status == 401) {
                        UiHelpers.showErrorToast('Login Failure', '', 'Invalid user name/password');
                    }
                    else {
                        UiHelpers.showErrorToast('Error', '', 'An unexpected error occurred trying to log you in');
                    }
                    that.isLoading = false;
                });
        }
    },
    mounted: function () {
        this.isLoading = false;
        document.getElementById("login-username").focus();
    }
}).mount('#app-login')