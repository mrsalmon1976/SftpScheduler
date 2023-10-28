namespace SftpSchedulerService
{
    public static class AppConstants
    {
        public static string DefaultAdminUserName = "admin";
        
        public static string DefaultAdminUserPassword = "admin";

        public static class NotificationType
        {
            public const string Error = "Error";

            public const string Warning = "Warning";

            public const string Information = "Information";

            public static string[] All = new string[] 
            { 
                NotificationType.Error, 
                NotificationType.Warning, 
                NotificationType.Information 
            };
        }
    }
}
