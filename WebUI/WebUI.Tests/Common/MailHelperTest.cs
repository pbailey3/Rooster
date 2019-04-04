using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WebUI.Common;

namespace WebUI.Tests.Common
{
    [TestClass]
    public class MailHelperTest
    {
        [TestMethod]
        public void TestMethod1()
        {
            try
            {
                MailHelper.SendConfirmationMail("paulbaileyoz@gmail.com", "firstName", "http://ConfirmatoinURL");
            }
            catch
            {
                Assert.Fail();
            }
        }
    }
}
