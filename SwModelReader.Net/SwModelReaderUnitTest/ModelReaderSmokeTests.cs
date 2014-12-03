using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SwModelReaderCore;
using System.IO;
using System.Diagnostics;
using System.Text;
using System.Linq;


namespace SwModelReaderUnitTest
{
    [TestClass]
    public class ModelReaderSmokeTests
    {
        [TestMethod]
        public void SmokeTestPart0()
        {
            string curdir = Directory.GetCurrentDirectory();
            string testModel = Path.Combine(curdir, "..\\..\\models\\coffee.sldprt");
            Assert.IsTrue(File.Exists(testModel));
            using (FileStream fstream = new FileStream(testModel, FileMode.Open, FileAccess.Read))
            {
                {
                    var reader = new SwModelReaderCore.SwModelReader(fstream);
                    string[] streamNames;
                    reader.GetAvailableStreamNames(out streamNames);
                    Assert.IsNotNull(streamNames);
                    Assert.IsTrue(streamNames.Length > 0);
                    foreach (var item in streamNames)
                    {
                        Debug.WriteLine(item);

                    }

                }

            }
        }
        [TestMethod]
        public void SmokeTestPart1()
        {
            string curdir = Directory.GetCurrentDirectory();
            string testModel = Path.Combine(curdir, "..\\..\\models\\coffeepot.sldprt");
            Assert.IsTrue(File.Exists(testModel));
            using (FileStream fstream = new FileStream(testModel, FileMode.Open, FileAccess.Read))
            {
                {
                    var reader = new SwModelReaderCore.SwModelReader(fstream);
                    string[] streamNames;
                    reader.GetAvailableStreamNames(out streamNames);
                    Assert.IsNotNull(streamNames);
                    Assert.IsTrue(streamNames.Length > 0);
                    foreach (var item in streamNames)
                    {
                        byte[] blob;
                        var ret = reader.GetStream(item, out blob);
                        Assert.AreEqual(ret, SwFileReaderResult.Ok);
                        Assert.IsNotNull(blob);

                    }

                }

            }
        }
        [TestMethod]
        public void SmokeTestPart1Preview()
        {
            string curdir = Directory.GetCurrentDirectory();
            string testModel = Path.Combine(curdir, "..\\..\\models\\coffeepot.sldprt");
            Assert.IsTrue(File.Exists(testModel));
            using (var reader = SwModelReader.Open(testModel))
            {
                {
                   
                    byte[] pngData;
                    reader.GetStream("PreviewPNG", out pngData);
                    var strHeader = Encoding.ASCII.GetString(pngData.Take(4).ToArray());
                    Assert.IsTrue(strHeader.ToLower().EndsWith("png"));

                }

            }
        }
        [TestMethod]
        public void SmokeTestAsm()
        {
            string curdir = Directory.GetCurrentDirectory();
            string testModel = Path.Combine(curdir, "..\\..\\models\\coffeejar.sldasm");
            Assert.IsTrue(File.Exists(testModel));
            using (FileStream fstream = new FileStream(testModel, FileMode.Open, FileAccess.Read))
            {
                {
                    var reader = new SwModelReaderCore.SwModelReader(fstream);
                    string[] streamNames;
                    reader.GetAvailableStreamNames(out streamNames);
                    Assert.IsNotNull(streamNames);
                    Assert.IsTrue(streamNames.Length > 0);
                    foreach (var item in streamNames)
                    {
                        byte[] blob;
                        var ret = reader.GetStream(item, out blob);
                        Assert.AreEqual(ret,SwFileReaderResult.Ok);
                        Assert.IsNotNull(blob);
                    }

                }

            }
        }
        [TestMethod]
        public void SmokeTestDrw()
        {
            string curdir = Directory.GetCurrentDirectory();
            string testModel = Path.Combine(curdir, "..\\..\\models\\box.slddrw");
            Assert.IsTrue(File.Exists(testModel));
            using (FileStream fstream = new FileStream(testModel, FileMode.Open, FileAccess.Read))
            {
                {
                    var reader = new SwModelReaderCore.SwModelReader(fstream);
                    string[] streamNames;
                    reader.GetAvailableStreamNames(out streamNames);
                    foreach (var item in streamNames)
                    {
                        byte[] blob;
                        var ret = reader.GetStream(item, out blob);
                        Assert.AreEqual(ret, SwFileReaderResult.Ok);
                        Assert.IsNotNull(blob);
                    }

                }

            }
        }
    }
}
