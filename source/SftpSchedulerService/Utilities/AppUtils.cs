using System.Reflection;

namespace SftpSchedulerService.Utilities
{
    public static class AppUtils
    {
        private static string? _version;

        static AppUtils()
        {
        }

        public static string Version
        {
            get 
            { 
                if (String.IsNullOrEmpty(_version)) 
                {
                    _version = Assembly.GetExecutingAssembly()?.GetName()?.Version?.ToString(3);
                }
                return _version ?? String.Empty;
            }    
        }
    }
}
