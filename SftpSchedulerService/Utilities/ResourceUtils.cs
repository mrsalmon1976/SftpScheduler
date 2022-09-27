using System.Reflection;

namespace SftpSchedulerService.Utilities
{
    public class ResourceUtils
    {
        public virtual string ReadResource(string qualifiedName)
        {
            var assembly = Assembly.GetExecutingAssembly();
            string result = null;

            using (Stream stream = assembly.GetManifestResourceStream(qualifiedName))
            {
                using (StreamReader reader = new StreamReader(stream))
                {
                    result = reader.ReadToEnd();
                }
            }

            return result;
        }
    }
}
