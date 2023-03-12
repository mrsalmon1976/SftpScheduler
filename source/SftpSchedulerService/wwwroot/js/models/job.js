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

    validate = function () {
        return (
            this.validateName()
            && this.validateHost()
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