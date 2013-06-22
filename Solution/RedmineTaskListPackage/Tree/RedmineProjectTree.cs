using Redmine;
using System.Linq;

namespace RedmineTaskListPackage.Tree
{
    public class RedmineProjectTree : RedmineProjectTreeNode
    {
        public static RedmineProjectTree Create(RedmineProject[] projects)
        {
            return Fill(new RedmineProjectTree(), projects);
        }

        private static T Fill<T>(T node, RedmineProject[] projects)
            where T: RedmineProjectTreeNode
        {
            foreach (var project in from x in projects where x.ParentId == node.Id select x)
            {
                node.Nodes.Add(Fill(new RedmineProjectTreeNode() { 
                    Id = project.Id, 
                    Name = project.Name
                }, projects));
            }

            return node;
        }
    }
}
