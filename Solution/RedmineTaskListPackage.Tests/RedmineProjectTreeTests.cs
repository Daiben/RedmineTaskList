using NUnit.Framework;
using Redmine;

namespace RedmineTaskListPackage.Tree.Tests
{
    [TestFixture]
    public class RedmineProjectTreeTests
    {
        [Test]
        public void Create()
        {
            var projects = new[]
            {
                new RedmineProject { Id = 1 },
                new RedmineProject { Id = 2, ParentId = 1 },
                new RedmineProject { Id = 3, ParentId = 2 }
            };

            var tree = RedmineProjectTree.Create(projects);

            Assert.AreEqual(1, tree.Nodes.Count);
            Assert.AreEqual(1, tree.Nodes[0].Id);
            Assert.AreEqual(1, tree.Nodes[0].Nodes.Count);
            Assert.AreEqual(2, tree.Nodes[0].Nodes[0].Id);
            Assert.AreEqual(1, tree.Nodes[0].Nodes[0].Nodes.Count);
            Assert.AreEqual(3, tree.Nodes[0].Nodes[0].Nodes[0].Id);
        }
    }
}
