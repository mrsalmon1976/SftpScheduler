using Dapper;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using WinSCP;

namespace SftpScheduler.BLL.DataAnnotations
{
    public class HostKeyFingerprintAttribute : ValidationAttribute
    {
        public HostKeyFingerprintAttribute() : base("Please provide a valid host key fingerprint")
        {

        }

        public override bool IsValid(object? value)
        {
            if (value == null)
            {
                return true;
            }
            var hostKeyFingerPrint = value.ToString();
            if (String.IsNullOrEmpty(hostKeyFingerPrint)) 
            {
                return true;
            }

            try
            {
                SessionOptions sessionOptions = new SessionOptions();
                sessionOptions.SshHostKeyFingerprint = hostKeyFingerPrint;
                return true;
            }
            catch (System.ArgumentException)
            {
                return false;
            }
        }

    }

}
