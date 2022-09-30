﻿using System.Reflection;

namespace SftpSchedulerService.Utilities
{
    public class ResourceUtils
    {
        public virtual string ReadResource(string qualifiedName)
        {
            var assembly = Assembly.GetExecutingAssembly();
            string? result = null;

            Stream? resourceStream = assembly.GetManifestResourceStream(qualifiedName);
            if (resourceStream == null)
            {
                throw new ArgumentException($"Resource {qualifiedName} does not exist in the current assembly");
            }

            using (resourceStream)
            {
                using (StreamReader reader = new StreamReader(resourceStream))
                {
                    result = reader.ReadToEnd();
                }
            }

            return result;
        }
    }
}
