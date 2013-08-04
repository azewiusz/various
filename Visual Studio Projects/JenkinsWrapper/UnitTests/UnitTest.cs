using System;
using JenkinsWrapper;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests
{
    [TestClass]
    public class UnitTest
    {
        [TestMethod]
        public void Test_getPID()
        {
            string param = @"PsExec v1.98 - Execute processes remotely" +
                            @"Copyright (C) 2001-2010 Mark Russinovich" +
                            @"Sysinternals - www.sysinternals.com" +
                            @"Starting C:\Program Files (x86)\Jenkins\start.bat on localhost..." +
                            @"C:\Program Files (x86)\Jenkins\start.bat started on localhost with process ID 8744.";
            string returned = Service1.getPID(param);
            Console.WriteLine("[" + returned + "]");
            Assert.AreEqual("8744", returned);
        }
    }
}