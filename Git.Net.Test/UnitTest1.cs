using Microsoft.VisualStudio.TestTools.UnitTesting;
using FaustVX.Temp;

namespace Git.Net.Test
{
    [TestClass]
    public class UnitTest1
    {
        private static void Prepare(System.Action action)
        {
            using (TemporaryDirectory.CreateTemporaryDirectory(setCurrentDirectory: true))
                action();
        }

        private static void AssertLastCommit(bool isNotNull = true)
        {
            var msg = Git.GetLastCommit();
            System.Console.WriteLine(msg ?? "null");
            if (isNotNull)
                Assert.IsNotNull(msg);
            else
                Assert.IsNull(msg);
        }

        [TestMethod]
        public void GitInit()
            => Prepare(() => Git.Init());

        [TestMethod]
        public void Join()
        {
            var actual1 = new[] { "clone", "-n", "-b test", "http://github.com" }.Join();
            System.Console.WriteLine(actual1);
            Assert.AreEqual("clone -n -b test http://github.com", actual1);
            var actual2 = new[] { null, "-b test" }.Join();
            System.Console.WriteLine(actual2);
            Assert.AreEqual("-b test", actual2);
            var actual3 = new string?[] { null, null }.Join();
            System.Console.WriteLine(actual3);
            Assert.AreEqual("", actual3);
        }

        [TestMethod]
        public void GitClone()
            => Prepare(() =>
            {
                var exit = Git.Clone("");
                System.Console.WriteLine(exit);
                Assert.IsFalse(exit);
                AssertLastCommit(false);
                exit = Git.Clone(@"https://github.com/FaustVX/Git.Net.git", checkout: false, localDirectory: ".");
                AssertLastCommit();
                Assert.IsTrue(exit);
            });

        [TestMethod]
        public void GitCloneAnotherBranch()
            => Prepare(() =>
            {
                Git.Clone(@"https://github.com/FaustVX/FishShell.git", checkout: false, branch: "kodi", localDirectory: ".");
                AssertLastCommit();
            });

        [TestMethod]
        public void GitReset()
            => Prepare(() =>
            {
                Git.Clone(@"https://github.com/FaustVX/Git.Net.git", checkout: false, localDirectory: ".");
                AssertLastCommit();
                Git.Reset(^1, Git.ResetMode.Hard);
                AssertLastCommit();
                Git.Reset(0x1bcacd64e9c5464auL, Git.ResetMode.Hard);
                AssertLastCommit();
                Assert.ThrowsException<System.Runtime.CompilerServices.SwitchExpressionException>(() => Git.Reset(0x1bcacd6, (Git.ResetMode)10));
            });

        [TestMethod]
        public void Ref()
        {
            Assert.AreEqual("HEAD~1", new Git.Ref(^1).ToString());

            Assert.AreEqual("ref", new Git.Ref("ref").ToString());
            Assert.AreEqual("ref", new Git.Ref(^0, "ref").ToString());
            Assert.AreEqual("ref~1", new Git.Ref(^1, "ref").ToString());
            
            Assert.AreEqual("254fde", new Git.Ref(0x254fde).ToString());
            Assert.AreEqual("254fde", new Git.Ref(^0, 0x254fde).ToString());
            Assert.AreEqual("254fde~1", new Git.Ref(^1, 0x254fde).ToString());
            
            Assert.AreEqual("254fde25f", new Git.Ref(0x254fde25fL).ToString());
            Assert.AreEqual("254fde25f", new Git.Ref(^0, 0x254fde25fL).ToString());
            Assert.AreEqual("254fde25f~1", new Git.Ref(^1, 0x254fde25fL).ToString());
        }
    }
}
