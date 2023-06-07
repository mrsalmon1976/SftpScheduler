class SettingsModel {
    constructor() {
        this.maxConcurrentJobs = 1;
    }

    validate = function () {
    //    this.validateName();
    //    this.validateHost();
    //    this.validateKeyFingerprint();
    //    return (this.isHostValid && this.isNameValid);
        return true;
    }
}