class UserModel {
    constructor() {
        this.id = '';
        this.userName = '';
        this.email = '';
        this.isEnabled = true;

        // these default to true, as you don't want the screen showing everything is invalid at first pass
        this.isUserNameValid = true;
        this.isEmailValid = true;
    }

    validate = function () {
        this.validateUserName();
        this.validateEmail();
        return (
            this.isUserNameValid
            && this.isEmailValid
            );
    }

    validateEmail = function () {
        this.isEmailValid = (this.email.length > 0);
        return this.isEmailValid;
    }

    validateUserName = function () {
        this.isUserNameValid = (this.userName.length >= 4);
        return this.isUserNameValid;
    }
}