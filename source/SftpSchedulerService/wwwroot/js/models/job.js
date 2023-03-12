class JobModel {
    constructor() {
        this.name = '';
        this.hostId = 0;
        this.schedule = '';

        // these default to true, as you don't want the screen showing everything is invalid at first pass
        this.isNameValid = true;
        this.isHostValid = true;
        this.isScheduleValid = false;
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
        return (
            this.isNameValid
            && this.isHostValid
            && this.isScheduleValid
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

}