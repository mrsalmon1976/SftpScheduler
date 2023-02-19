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
        this.isNameValid = (this.name.length >= 4);
        return this.isNameValid;
    }

}