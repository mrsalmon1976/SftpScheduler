using System.Reflection;

namespace SftpScheduler.BLL.Utility
{
    public class ResourceUtils
    {
        public virtual string ReadResource(string qualifiedName)
        {
            Assembly? assembly = Assembly.GetAssembly(typeof(ResourceUtils));
            string? result = null;
            if (assembly != null)
            {

                Stream? resourceStream = assembly?.GetManifestResourceStream(qualifiedName);
                if (resourceStream != null)
                {
                    using (resourceStream)
                    {
                        using (StreamReader reader = new StreamReader(resourceStream))
                        {
                            result = reader.ReadToEnd();
                        }
                    }
                }
            }

            if (result == null)
            {
                throw new ArgumentException($"Resource {qualifiedName} does not exist in the current assembly");
            }
            return result;
        }
    }
}
