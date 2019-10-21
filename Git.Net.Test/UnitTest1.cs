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
        public void GitClone()
            => Prepare(() =>
            {
                Git.Clone(@"https://github.com/FaustVX/Git.Net.git", checkout: false);
                System.Environment.CurrentDirectory = @"./Git.Net";
                System.Console.Out.WriteLine(Git.GetLastCommit() ?? "null");
                Assert.IsNotNull(Git.GetLastCommit());
                Git.AddTag("test");
            });
    }
}
