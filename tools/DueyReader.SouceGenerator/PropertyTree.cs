using System.Collections.Generic;
using System.Linq;

namespace DueyReader.SouceGenerator
{
    public class TreeNode
    {
        public string Name { get; set; }
        public PropertyData Value { get; set; }
        public List<TreeNode> Children { get; set; } = new();
        public int Depth { get; set; }

        public TreeNode(string name, int depth)
        {
            Name = name;
            Depth = depth;
        }

        public static List<TreeNode> BuildTreeCore(Dictionary<string, PropertyData> dict)
        {
            var roots = new List<TreeNode>();

            foreach (var kvp in dict)
            {
                var path = kvp.Key.Split('/');
                var value = kvp.Value;

                List<TreeNode> currentLevel = roots;
                TreeNode currentNode = null;

                for (int depth = 0; depth < path.Length; depth++)
                {
                    var part = path[depth];
                    currentNode = currentLevel.FirstOrDefault(n => n.Name == part);
                    if (currentNode == null)
                    {
                        currentNode = new TreeNode(part, depth + 1);
                        currentLevel.Add(currentNode);
                    }
                    currentLevel = currentNode.Children;
                }

                if (currentNode != null)
                {
                    currentNode.Value = value;
                }
            }

            return roots;
        }

        public static TreeNode BuildTree(Dictionary<string, PropertyData> dict)
        {
            var list = BuildTreeCore(dict);
            return new TreeNode("", -1) { Children = list, Value = new PropertyData(3, "", "", "") };
        }
    }
}
