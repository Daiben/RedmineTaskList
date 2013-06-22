using System.Collections.ObjectModel;

namespace RedmineTaskListPackage.Tree
{
    public class RedmineProjectTreeNodeCollection : Collection<RedmineProjectTreeNode>
    {
        public RedmineProjectTreeNodeCollection()
        {
        }

        public RedmineProjectTreeNodeCollection(params RedmineProjectTreeNode[] items)
            :base(items)
        {

        }
    }
}
