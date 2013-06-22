namespace RedmineTaskListPackage.Tree
{
    public class RedmineProjectTreeNode
    {
        public int Id { get; set; }
     
        public string Name { get; set; }

        public RedmineProjectTreeNodeCollection Nodes { get; private set; }


        public RedmineProjectTreeNode()
        {
            Nodes = new RedmineProjectTreeNodeCollection();
        }
    }
}
