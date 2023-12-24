class HostModel {
    constructor() {
        this.hashId = '';
        this.name = '';
        this.host = '';
        this.port = 0;
        this.userName = '';
        this.password = '';
        this.keyFingerprint = '';
        this.protocol = 0;
        this.protocolName = '';
        this.ftpsMode = 0;

        // these default to true, as you don't want the screen showing everything is invalid at first pass
        this.isHostValid = true;
        this.isNameValid = true;
        this.isKeyFingerprintEmpty = true;
    }

    validate = function () {
        this.validateName();
        this.validateHost();
        this.validateKeyFingerprint();
        return (this.isHostValid && this.isNameValid);
    }

    validateHost = function () {
        this.isHostValid = (this.host.length >= 5);
        return this.isHostValid;
    }

    validateKeyFingerprint = function () {
        this.isKeyFingerprintEmpty = (this.keyFingerprint.length == 0);
        return this.isKeyFingerprintEmpty;
    }

    validateName = function () {
        this.isNameValid = (this.name.length >= 3);
        return this.isNameValid;
    }

}