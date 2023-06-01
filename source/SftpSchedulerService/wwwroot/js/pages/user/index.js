createApp({
    data() {
        return {
            currentUserName: '',
            users: [],
            isLoading: true,
            selectedUser: new UserModel()
        }
    },
    methods: {
        async disableUser(user) {
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

            if (user.userName == this.currentUserName) {
                alert('You cannot disable the current user');
                return;
            }

            this.selectedUser = user;

            $('#modal-disable-user').modal();

        }
    },
    mounted: function () {
        this.isLoading = false;
        this.currentUserName = this.$el.parentElement.getAttribute('data-current-user-name');
        UiHelpers.setPageHeader('Users');
        this.loadUsers();
    }
}).mount('#app-user')