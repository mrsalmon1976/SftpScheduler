class JobModel {
    constructor() {
        this.hashId = '';
        this.name = '';
        this.hostId = 0;
        this.schedule = '';
        this.type = 0;
        this.localPath = '';
        this.remotePath = '';

        // these default to true, as you don't want the screen showing everything is invalid at first pass
        this.isNameValid = true;
        this.isHostValid = true;
        this.isScheduleValid = false;
        this.isTypeValid = true;
        this.isLocalPathValid = true;
        this.isRemotePathValid = true;
    }

    convertScheduleToWords = function (text, callback) {
        var that = this;
        var result = '';
        axios.get('/api/cron', { params: { schedule: text } })
            .then(response => {
                that.isScheduleValid = response.data.isValid;
                if (that.isScheduleValid) {
                    result = response.data.scheduleInWords;
                }
                else {
                    result = response.data.errorMessage;
                }
            })
            .catch(err => {
                that.isScheduleValid = false;
                result = 'An error occurred retrieving the cron description - check application logs for details';
            })
            .then(() => {
                callback(result);
            });
    }

    validate = function () {
        this.validateName();
        this.validateHost();
        this.validateType();
        this.validateLocalPath();
        this.validateRemotePath();
        return (
            this.isNameValid
            && this.isHostValid
            && this.isScheduleValid
            && this.isTypeValid
            && this.isLocalPathValid
            && this.isRemotePathValid
            );
    }

    validateHost = function () {
        this.isHostValid = (this.hostId > 0);
        return this.isHostValid;
    }

    validateName = function () {
        this.isNameValid = (this.name.length >= 3);
        return this.isNameValid;
    }

    validateType = function () {
        this.isTypeValid = (this.type >= 1 && this.type <= 2);
        return this.isTypeValid;
    }

    validateLocalPath = function () {
        this.isLocalPathValid = (this.localPath.length > 0);
        return this.isLocalPathValid;
    }

    validateRemotePath = function () {
        this.isRemotePathValid = (this.remotePath.length > 0);
        return this.isRemotePathValid;
    }

}