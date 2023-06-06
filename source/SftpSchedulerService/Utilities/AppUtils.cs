using System.Reflection;

namespace SftpSchedulerService.Utilities
{
    public static class AppUtils
    {
        private static string? _version;
        private static bool? _isUnitTestContext;

        static AppUtils()
        {
        }

        public static bool IsUnitTestContext
        {
            get
            {
                if (_isUnitTestContext == null)
                {
                    _isUnitTestContext = AppDomain.CurrentDomain.GetAssemblies().Any(
                        a => (a.FullName ?? String.Empty).ToLowerInvariant().StartsWith("nunit.framework"));
                }
                return _isUnitTestContext.Value;
            }
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
