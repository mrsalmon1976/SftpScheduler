class SettingsModel {
    constructor() {
        this.maxConcurrentJobs = 1;

        this.smtpHost = '';
        this.smtpPort = 25;
        this.smtpUserName = '';
        this.smtpPassword = '';
        this.smtpEnableSsl = true;
        this.smtpPasswordIgnored = true;
        this.smtpFromName = 'SftpScheduler';
        this.smtpFromEmail = '';

        this.isSmtpHostValid = true;
        this.isSmtpPortValid = true;
        this.isSmtpFromEmailValid = true;
    }

    validate = function () {
        return (
            this.validateSmtpFromEmail()
            && this.validateSmtpHost()
            && this.validateSmtpPort()
        );
    }

    validateSmtpFromEmail = function () {
        this.isSmtpFromEmailValid = (this.smtpFromEmail == null || this.smtpFromEmail.length < 1 || ValidationUtils.isEmailValid(this.smtpFromEmail));
        return this.isSmtpFromEmailValid;
    }

    validateSmtpHost = function () {
        this.isSmtpHostValid = (this.smtpHost.length < 1 || this.smtpHost.length >= 5);
        return this.isSmtpHostValid;
    }

    validateSmtpPort = function () {
        this.isSmtpPortValid = (this.smtpPort >= 1 && this.smtpPort <= 65536);
        return this.isSmtpPortValid;
    }

}