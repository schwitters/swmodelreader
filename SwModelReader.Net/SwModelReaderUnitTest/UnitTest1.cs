using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SwModelReaderCore;
using System.IO;
using System.Diagnostics;


namespace SwModelReaderUnitTest
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            string curdir = Directory.GetCurrentDirectory();
            string testModel = Path.Combine(curdir, "..\\..\\Models\\_in_ec_valve.sldprt");
            Assert.IsTrue(File.Exists(testModel));
            using (FileStream fstream = new FileStream(testModel, FileMode.Open, FileAccess.Read))
            {
                {
                    SwModelReaderCore.SwModelReader reader = new SwModelReaderCore.SwModelReader(fstream);
                    string[] streamNames;
                    reader.GetAvailableStreamNames(out streamNames);
                    foreach (var item in streamNames)
                    {
                        Debug.WriteLine(item);

                    }

                }

            }
        }
    }
}
