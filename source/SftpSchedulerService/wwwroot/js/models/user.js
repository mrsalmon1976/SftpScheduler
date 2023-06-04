class UserModel {
    constructor() {
        this.id = '';
        this.userName = '';
        this.email = '';
        this.password = '';
        this.role = UserRoles.User;
        this.isEnabled = true;

        // these default to true, as you don't want the screen showing everything is invalid at first pass
        this.isUserNameValid = true;
        this.isEmailValid = true;
        this.isPasswordValid = true;
    }

    validate = function (validatePassword) {
        this.validateUserName();
        this.validateEmail();
        this.validatePassword();
        return (
            this.isUserNameValid
            && this.isEmailValid
            && this.isPasswordValid
            );
    }

    validateEmail = function () {
        this.isEmailValid = (this.email.length >= 5);
        return this.isEmailValid;
    }

    validatePassword = function ()
    {
        // if we are editing, it is valid to supply no password (it won't get updated)
        if (this.id.length > 0 && this.password.length == 0) {
            this.isPasswordValid = true;

        }
        else
        {
            this.isPasswordValid = (this.password.length >= 5);
        }
        return this.isPasswordValid;
    }

    validateUserName = function () {
        this.isUserNameValid = (this.userName.length >= 3);
        return this.isUserNameValid;
    }
}