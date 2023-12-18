using NSubstitute;
using SftpScheduler.BLL.Data;
using SftpScheduler.BLL.Models;

namespace SftpScheduler.BLL.Tests.Builders.Models
{
    public class JobEntityBuilder
    {

		private JobEntity _jobEntity = new JobEntity();

        public JobEntityBuilder WithDeleteAfterDownload(bool deleteAfterDownload)
        {
            _jobEntity.DeleteAfterDownload = deleteAfterDownload;
            return this;
        }

        public JobEntityBuilder WithIsEnabled(bool isEnabled)
        {
            _jobEntity.IsEnabled = isEnabled;
            return this;
        }

        public JobEntityBuilder WithHostId(int hostId)
        {
            _jobEntity.HostId = hostId;
            return this;
        }

        public JobEntityBuilder WithId(int id)
        {
            _jobEntity.Id = id;
            return this;
        }

        public JobEntityBuilder WithLocalCopyPaths(string localCopyPaths)
        {
            _jobEntity.LocalCopyPaths = localCopyPaths;
            return this;
        }

        public JobEntityBuilder WithLocalPath(string localPath)
        {
            _jobEntity.LocalPath = localPath;
            return this;
        }

        public JobEntityBuilder WithName(string name)
        {
            _jobEntity.Name = name;
            return this;
        }

        public JobEntityBuilder WithRandomProperties()
		{
			_jobEntity.Id = Faker.RandomNumber.Next(1, Int32.MaxValue);
			_jobEntity.Name = Faker.Lorem.GetFirstWord();
			_jobEntity.HostId = Faker.RandomNumber.Next(1, Int32.MaxValue);
			_jobEntity.Type = Faker.RandomNumber.Next(1, 2) == 1 ? BLL.Data.JobType.Download : BLL.Data.JobType.Upload;
			_jobEntity.Schedule = "0 * 0 ? * * *";
			_jobEntity.ScheduleInWords = "Every minute";
			_jobEntity.LocalPath = "\\\\localshare\\" + Faker.Lorem.GetFirstWord();
			_jobEntity.RemotePath = "/" + Faker.Lorem.GetFirstWord();
            _jobEntity.RemoteArchivePath = _jobEntity.RemotePath + "/" + Faker.Lorem.GetFirstWord();
            _jobEntity.DeleteAfterDownload = Faker.RandomNumber.Next(1, 2) == 1 ? true : false;
			return this;
		}

		public JobEntityBuilder WithRemotePath(string remotePath)
		{
			_jobEntity.RemotePath = remotePath;
			return this;
		}

        public JobEntityBuilder WithRemoteArchivePath(string remoteArchivePath)
        {
            _jobEntity.RemoteArchivePath = remoteArchivePath;
            return this;
        }

        public JobEntityBuilder WithRestartOnFailure(bool restartOnFailure)
        {
            _jobEntity.RestartOnFailure = restartOnFailure;
            return this;
        }

        public JobEntityBuilder WithSchedule(string schedule)
        {
            _jobEntity.Schedule = schedule;
            return this;
        }


        public JobEntityBuilder WithType(JobType jobType)
        {
            _jobEntity.Type = jobType;
            return this;
        }

        public JobEntity Build()
		{
			return _jobEntity;
		}
	}
}
