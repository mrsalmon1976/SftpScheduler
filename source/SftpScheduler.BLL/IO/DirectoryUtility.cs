namespace SftpScheduler.BLL.IO
{
    // should be using SystemWrapper library but that targets framework,  not .NET 6
    public interface IDirectoryUtility
    {

        void CopyRecursive(string sourceDirectory, string targetDirectory, IEnumerable<string>? exclusions = null);

        void CopyRecursive(DirectoryInfo source, DirectoryInfo target, IEnumerable<string>? exclusions = null);

        void Create(string path);

        void Delete(string directory, int maxRetryCount = 10);

        void Delete(DirectoryInfo directory, int maxRetryCount = 10);

        void DeleteContents(string sourceDirectory, IEnumerable<string>? exclusions = null, int maxRetryCount = 10);

        void DeleteContents(DirectoryInfo source, IEnumerable<string>? exclusions = null, int maxRetryCount = 10);

        IEnumerable<string> EnumerateFiles(string path);

        bool Exists(string path);

        string[] GetFiles(string sourceDirectory, SearchOption searchOption, string searchPattern = "*.*");

    }

    public class DirectoryUtility : IDirectoryUtility
    {

        public void CopyRecursive(string sourceDirectory, string targetDirectory, IEnumerable<string>? exclusions = null)
        {
            DirectoryInfo diSource = new DirectoryInfo(sourceDirectory);
            DirectoryInfo diTarget = new DirectoryInfo(targetDirectory);

            CopyRecursive(diSource, diTarget, exclusions);
        }

        public void CopyRecursive(DirectoryInfo source, DirectoryInfo target, IEnumerable<string>? exclusions = null)
        {
            Directory.CreateDirectory(target.FullName);

            // Copy each file into the new directory.
            foreach (FileInfo fi in source.GetFiles())
            {
                if (exclusions != null && exclusions.Contains(fi.FullName)) continue;

                // Console.WriteLine(@"Copying {0}\{1}", target.FullName, fi.Name);
                fi.CopyTo(Path.Combine(target.FullName, fi.Name), true);
            }

            // Copy each subdirectory using recursion.
            foreach (DirectoryInfo diSourceSubDir in source.GetDirectories())
            {
                if (exclusions != null && exclusions.Contains(diSourceSubDir.FullName)) continue;

                DirectoryInfo nextTargetSubDir = target.CreateSubdirectory(diSourceSubDir.Name);
                CopyRecursive(diSourceSubDir, nextTargetSubDir, exclusions);
            }
        }

        public void Create(string path)
        {
            Directory.CreateDirectory(path);
        }

        public void Delete(string directory, int maxRetryCount = 10)
        {
            Delete(new DirectoryInfo(directory), maxRetryCount);
        }

        public void Delete(DirectoryInfo directory, int maxRetryCount = 10)
        {
            if (directory.Exists)
            {
                Action deleteAction = () => directory.Delete(true);
                TryDelete(deleteAction, directory.FullName, maxRetryCount);
            }

        }

        public void DeleteContents(string sourceDirectory, IEnumerable<string>? exclusions = null, int maxRetryCount = 0)
        {
            DirectoryInfo diSource = new DirectoryInfo(sourceDirectory);

            DeleteContents(diSource, exclusions, maxRetryCount);
        }

        public void DeleteContents(DirectoryInfo source, IEnumerable<string>? exclusions = null, int maxRetryCount = 0)
        {
            // Copy each file into the new directory.
            foreach (FileInfo fi in source.GetFiles())
            {
                if (exclusions != null)
                {
                    // we need to check the folder AND it's directories
                    if (exclusions.Contains(fi.FullName)) continue;
                }


                // Console.WriteLine(@"Copying {0}\{1}", target.FullName, fi.Name);
                if (fi.Exists)
                {
                    Action deleteAction = () => fi.Delete();
                    TryDelete(deleteAction, fi.FullName, maxRetryCount);
                }
            }

            // Copy each subdirectory using recursion.
            foreach (DirectoryInfo diSourceSubDir in source.GetDirectories())
            {
                if (exclusions != null && exclusions.Contains(diSourceSubDir.FullName)) continue;

                if (diSourceSubDir.Exists)
                {
                    Action deleteAction = () => diSourceSubDir.Delete(true);
                    TryDelete(deleteAction, diSourceSubDir.FullName, maxRetryCount);
                }
            }
        }


        public IEnumerable<string> EnumerateFiles(string path)
        {
            return Directory.EnumerateFiles(path);
        }

        public bool Exists(string path)
        {
            return Directory.Exists(path);
        }

        public string[] GetFiles(string sourceDirectory, SearchOption searchOption, string searchPattern = "*.*")
        {
            return Directory.GetFiles(sourceDirectory, searchPattern, searchOption);
        }

        private void TryDelete(Action deleteAction, string fullPath, int maxRetryCount)
        {
            int retryCount = 0;
            const int SleepTime = 500;
            bool isTryingToDelete = true;
            while (isTryingToDelete)
            {
                try
                {
                    deleteAction();
                    isTryingToDelete = false;
                }
                catch (Exception ex)
                {
                    retryCount++;
                    if (retryCount >= maxRetryCount)
                    {
                        string msg = $"Failed to delete object {fullPath}";
                        throw new IOException(msg, ex);
                    }
                    else
                    {
                        Thread.Sleep(SleepTime);
                    }
                }
            }
        }


    }
}
