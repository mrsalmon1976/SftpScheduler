﻿namespace SftpSchedulerService.Models
{
    public class HostViewModel
    {
        public int Id { get; set; }

        public string? Name { get; set; }

        public string? Host { get; set; }

        public int Port { get; set; }

        public string? UserName { get; set; }

        public string? Password { get; set; }
    }
}
