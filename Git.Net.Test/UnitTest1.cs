using Microsoft.VisualStudio.TestTools.UnitTesting;
using FaustVX.Temp;

namespace Git.Net.Test
{
    [TestClass]
    public class UnitTest1
    {
        private static void Prepare(System.Action action)
        {
            using var dir = TemporaryDirectory.CreateTemporaryDirectory();
            System.Console.WriteLine(dir);
            System.Environment.CurrentDirectory = dir;
            action();
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
                Git.Clone(@"https://github.com/FaustVX/Git.Net.git", checkout: false, localDirectory: "Repo");
                System.Environment.CurrentDirectory = new System.IO.DirectoryInfo("Repo").FullName;
                System.Console.WriteLine(Git.GetLastCommit() ?? "null");
                Assert.IsNotNull(Git.GetLastCommit());
                Git.AddTag("test");
            });
    }
}
