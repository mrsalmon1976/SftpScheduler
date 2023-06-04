createApp({
    data() {
        return {
            isLoading: true,
            isEdit: false,
            userId: '',
            user: new UserModel(),
            submitButtonText: 'Create User'
        }
    },
    methods: {
        async loadUser() {

            let result = await axios.get('/api/users/' + this.user.id)
                .catch(err => {
                    this.isLoading = false;
                    UiHelpers.showErrorToast('Error', '', err.message);
                    return;
                });


            var userData = result.data;
            UiHelpers.setPageHeader('Edit User / ' + userData.userName);
            this.user.userName = userData.userName;
            this.user.email = userData.email;
            this.user.password = '';
            this.user.role = userData.role;
            this.user.isEnabled = userData.isEnabled;
            this.user.validate();
            this.isLoading = false;
        },
        async submit() {
            this.isLoading = true;
            $('#toastsContainerTopRight > .toast').remove();

            // make sure all is valid
            if (!this.user.validate()) {
                this.isLoading = false;
                UiHelpers.showErrorToast('Validation Failure', '', 'User properties missing or incorrect.', 10000);
                return;
            }

            var url = (this.isEdit ? '/api/users/' + this.user.id : '/api/users');
            axios.post(url, this.user)

                .then(response => {
                    window.location.href = '/users';
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
        this.user.id = this.$el.parentElement.getAttribute('data-user-id');
        if (this.user.id != '') {
            this.isEdit = true;
            this.submitButtonText = 'Update User';
            this.loadUser();
        }
        else {
            this.isLoading = false;
            UiHelpers.setPageHeader('Create New User');
        }
    }
}).mount('#app-user')