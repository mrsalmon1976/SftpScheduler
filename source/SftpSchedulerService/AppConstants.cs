namespace SftpSchedulerService
{
    public static class AppConstants
    {
        public static string DefaultAdminUserName = "admin";
        
        public static string DefaultAdminUserPassword = "admin";

        public static string SecretKeyContainer = "SftpSchedulerService";

        public static class NotificationType
        {
            public const string Error = "Error";

            public const string Warning = "Warning";

            public const string Information = "Information";
        }
    }
}
