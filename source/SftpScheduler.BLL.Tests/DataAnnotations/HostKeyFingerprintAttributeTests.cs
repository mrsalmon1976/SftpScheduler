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
    public class HostKeyFingerprintAttributeTests
    {
        [TestCase("")]
        [TestCase(null)]
        public void IsValid_NullOrEmpty_ReturnsTrue(string keyFingerPrint)
        {
            HostKeyFingerprintAttribute attribute = new HostKeyFingerprintAttribute();
            bool result = attribute.IsValid(keyFingerPrint);
            Assert.IsTrue(result);

        }

        [TestCase("51d8f04f-dc55-4ee4-ab7d-6ad43008fc52")]
        [TestCase("aaa")]
        public void IsValid_InvalidHostKeyFingerPrint_ReturnsFalse(string keyFingerPrint)
        {
            HostKeyFingerprintAttribute attribute = new HostKeyFingerprintAttribute();
            bool result = attribute.IsValid(keyFingerPrint);
            Assert.IsFalse(result);

        }

        [TestCase("ssh-rsa 2048 e4:9b:47:5a:fc:09:0e:41:a9:7d:0d:f9:cc:c0:4d:e0")]
        [TestCase("ssh-rsa 2048 d4:24:af:f4:75:b7:80:1e:4f:d8:bd:96:46:04:26:43")]
        [TestCase("ssh-dss 1024 e2:88:8b:d0:c0:b0:97:ca:20:93:a1:b0:4f:ce:02:48")]
        public void IsValid_ValidHostKeyFingerPrint_ReturnsTrue(string keyFingerPrint)
        {
            HostKeyFingerprintAttribute attribute = new HostKeyFingerprintAttribute();
            bool result = attribute.IsValid(keyFingerPrint);
            Assert.IsTrue(result);

        }

    }
}
