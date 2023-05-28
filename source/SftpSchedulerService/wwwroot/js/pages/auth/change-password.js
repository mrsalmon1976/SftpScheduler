createApp({
    data() {
        return {
            passwordCurrent: '',
            passwordNew: '',
            passwordConfirm: '',
            isPasswordNewValid: false,
            isPasswordConfirmValid: true,
            isLoading: true,
            isValid: false
        }
    },
    methods: {
        async updatePassword() {

            if (!this.validate()) {
                return;
            }

            var that = this;
            this.isLoading = true;
            let payload = { newPassword: this.passwordNew, currentPassword: this.passwordCurrent };

            let res = await axios.post('/api/auth/change-password', payload)
                .then(function (response) {
                    that.passwordCurrent = '';
                    that.passwordNew = '';
                    that.passwordConfirm = '';
                    that.validate();
                    UiHelpers.showSuccessToast('Password Changed!', '', 'Your password has been successfully changed');
                    that.isLoading = false;
                })
                .catch(function (err) {
                    if (err.response && err.response.data && err.response.data.errorMessages) {
                        var errMessages = err.response.data.errorMessages;
                        for (var i = 0; i < errMessages.length; i++) {
                            UiHelpers.showErrorToast('Validation Error', '', errMessages[i]);
                        }
                    }
                    else {
                        UiHelpers.showErrorToast('Validation Error', '', err.message);
                    }
                    that.isLoading = false;
                });
        },
        validate() {
            this.isPasswordNewValid = (this.passwordNew.length >= 5);
            this.isPasswordConfirmValid = (this.passwordNew == this.passwordConfirm);
            this.isValid = (this.isPasswordNewValid && this.isPasswordConfirmValid);
            return this.isValid;
        }
    },
    mounted: function () {
        UiHelpers.setPageHeader('Change Password');
        this.isLoading = false;
        document.getElementById("passwordNew").focus();
    }
}).mount('#app-change-password')