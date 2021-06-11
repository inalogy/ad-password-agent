using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using ADPasswordSecureCache;
using ADPasswordSecureCache.Policies;
using System.Security;
using System.IO;

namespace ADPasswordSecureCacheTests
{
    [TestClass]
    public class ADPasswordSecureCacheTests
    {
        private DiskCache<string> DiscCacheInstance = null;
        private const string testString = "ABCDefg123";
        private string testEmptyString = String.Empty;
        private const string testKey = "test";

        [TestInitialize] 
        public void InitTests()
        {
            DiscCacheInstance = new DiskCache<string>(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"Midpoint.ADPassword.Agent"),
            new FifoCachePolicy<string>(),
            2 * 1024 * 1024);
        }

        [TestMethod]
        public void TestSecureStringCaching()
        {
            Assert.IsTrue(DiscCacheInstance.TrySetSecureString(testKey, testString.ToSecureString()));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException), "The given secure string is empty.")]
        public void TestEmptySecureStringCaching()
        {
            DiscCacheInstance.TrySetSecureString(testKey, testEmptyString.ToSecureString());
        }

        [TestMethod]
        public void TestSecureStringCacheRead()
        {
            Assert.IsTrue(DiscCacheInstance.TrySetSecureString(testKey, testString.ToSecureString()));
            Assert.IsTrue(DiscCacheInstance.TryGetSecureString(testKey, out SecureString testStringCopy));
            Assert.AreEqual<string>(testString, testStringCopy.ConvertToString());
        }

    }
}
