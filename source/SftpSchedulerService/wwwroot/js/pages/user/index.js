createApp({
    data() {
        return {
            users: [],
            isLoading: true,
            selectedUser: new UserModel()
        }
    },
    methods: {
        async disableUser(userId) {
            alert('Sorry, this functionality is still missing #sadface')
            //var that = this;
            //this.isLoading = true;

            //if (this.selectedJob != null && jobIdHash.length > 0) {
            //    let result = await axios.delete('/api/jobs/' + jobIdHash)
            //        .then(response => {
            //            that.loadJobs();
            //            UiHelpers.showSuccessToast('Delete Job', '', 'Job successfully deleted');
            //        })
            //        .catch(err => {
            //            UiHelpers.showErrorToast('Error', '', err.message);
            //            that.isLoading = false;
            //        })
            //        .then(response => {
            //            $('#modal-delete-job').modal('toggle');
            //        });
            //}

        },
        loadUsers() {
            var that = this;
            this.isLoading = true;
            axios.get('/api/users')
                .then(function (response) {
                    that.users = response.data;
                    that.isLoading = false;
                })
                .catch(err => {
                    UiHelpers.showErrorToast('Error', '', err.message);
                    that.isLoading = false;
                });
        },
        showDisableDialog(user) {
            this.selectedUser = user;
            $('#modal-disable-user').modal();

        }
    },
    mounted: function () {
        this.isLoading = false;
        UiHelpers.setPageHeader('Users');
        this.loadUsers();
    }
}).mount('#app-user')