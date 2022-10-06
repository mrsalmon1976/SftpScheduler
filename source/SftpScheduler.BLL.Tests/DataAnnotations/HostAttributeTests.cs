using NUnit.Framework;
using SftpScheduler.BLL.DataAnnotations;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SftpScheduler.BLL.Tests.DataAnnotations
{
    [TestFixture]
    public class HostAttributeTests
    {
        [TestCase("1.2")]
        [TestCase("1.2.3")]
        [TestCase("0.0.0")]
        public void IsValid_InvalidIpAddress_ReturnsFalse(string ipAddress)
        {
            HostAttribute hostAttribute = new HostAttribute();
            bool result = hostAttribute.IsValid(ipAddress);
            Assert.IsFalse(result);

        }

        [TestCase("192.168.10.12")]
        [TestCase("10.0.0.1")]
        public void IsValid_ValidIpAddress_ReturnsTrue(string ipAddress)
        {
            HostAttribute hostAttribute = new HostAttribute();
            bool result = hostAttribute.IsValid(ipAddress);
            Assert.IsTrue(result);

        }

        [TestCase("@dsd.com")]
        public void IsValid_InvalidDns_ReturnsFalse(string hostName)
        {
            HostAttribute hostAttribute = new HostAttribute();
            bool result = hostAttribute.IsValid(hostName);
            Assert.IsFalse(result);

        }

        [TestCase("dsd.com")]
        [TestCase("localhost")]
        [TestCase("www.test.com")]
        public void IsValid_ValidDns_Returnstrue(string hostName)
        {
            HostAttribute hostAttribute = new HostAttribute();
            bool result = hostAttribute.IsValid(hostName);
            Assert.IsTrue(result);

        }

    }
}
