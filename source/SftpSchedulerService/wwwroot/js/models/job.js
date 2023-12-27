class JobModel {
    constructor() {
        this.hashId = '';
        this.name = '';
        this.hostId = 0;
        this.schedule = '';
        this.type = 0;
        this.localPath = '';
        this.remotePath = '';
        this.deleteAfterDownload = false;
        this.remoteArchivePath = '';
        this.localCopyPaths = '';
        this.isEnabled = true;
        this.restartOnFailure = false;
        this.isFailing = false;
        this.compressionMode = 0;
        this.fileMask = '';
        this.preserveFilestamp = true;
        this.transferMode = 0;

        // these default to true, as you don't want the screen showing everything is invalid at first pass
        this.isNameValid = true;
        this.isHostValid = true;
        this.isScheduleValid = false;
        this.isTypeValid = true;
        this.isLocalPathValid = true;
        this.isRemotePathValid = true;
        this.isRemoteArchivePathValid = false;
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
        this.validateRemoteArchivePath();
        this.validateRemotePath();
        return (
            this.isNameValid
            && this.isHostValid
            && this.isScheduleValid
            && this.isTypeValid
            && this.isLocalPathValid
            && this.isRemoteArchivePathValid
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
        this.isTypeValid = (this.type >= JobTypes.Download && this.type <= JobTypes.Upload);
        return this.isTypeValid;
    }

    validateLocalPath = function () {
        this.isLocalPathValid = (this.localPath.length > 0);
        return this.isLocalPathValid;
    }

    validateRemoteArchivePath = function () {
        if (this.type == JobTypes.Upload || this.deleteAfterDownload) {
            this.isRemoteArchivePathValid = true;
        }
        else {
            this.isRemoteArchivePathValid = (this.remoteArchivePath.length > 0);
        }
        return this.isRemoteArchivePathValid;
    }

    validateRemotePath = function () {
        this.isRemotePathValid = (this.remotePath.length > 0);
        return this.isRemotePathValid;
    }

}