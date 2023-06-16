using SftpScheduler.BLL.DataAnnotations;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SftpScheduler.BLL.Models
{
    public class SmtpHost
    {
        [Required]
        [Host]
        public string Host { get; set; } = String.Empty;

        [Required]
        [Range(1, 65535)]
        public int Port { get; set; } = 25;

        public string? UserName { get; set; }

        public string? Password { get; set; }

        public bool EnableSsl { get; set; } = true;
    }
}
