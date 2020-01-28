using System.Text;

namespace KernelManagementJam.DebugUtils
{
    using System.Collections.Generic;

    public class AscIITreeDiagram<T>
    {
        // Constants for drawing lines and spaces
        private const string _cross = " ├──";
        private const string _corner = " └──";
        private const string _vertical = " │  ";
        private const string _space = "    ";

        public static void PopulateAscII(IEnumerable<Node<T>> topLevelNodes)
        {
            foreach (var topLevelNode in topLevelNodes)
            {
                PrintNode(topLevelNode, string.Empty);    
            }
        }

        public static void PopulateAscII(Node<T> topLevelNode)
        {
            PrintNode(topLevelNode, string.Empty);
        }

        static void PrintNode(Node<T> node, string indent)
        {
            // Console.WriteLine(node.Name);
            if (indent.Length > 0) node.AscIIBuilder.Append(' ');
            node.AscIIBuilder.Append(node.Name);

            // Loop through the children recursively, passing in the
            // indent, and the isLast parameter
            var numberOfChildren = node.Children.Count;
            for (var i = 0; i < numberOfChildren; i++)
            {
                var child = node.Children[i];
                var isLast = (i == (numberOfChildren - 1));
                PrintChildNode(child, indent, isLast);
            }
        }

        static void PrintChildNode(Node<T> node, string indent, bool isLast)
        {
            // Print the provided pipes/spaces indent
            // Console.Write(indent);
            node.AscIIBuilder.Append(indent);
            

            // Depending if this node is a last child, print the
            // corner or cross, and calculate the indent that will
            // be passed to its children
            if (isLast)
            {
                node.AscIIBuilder.Append(_corner);
                indent += _space;
            }
            else
            {
                node.AscIIBuilder.Append(_cross);
                indent += _vertical;
            }

            PrintNode(node, indent);
        }

    }
    public class Node<T>
    {
        public T State { get; set; }
        public string Name { get; set; }
        public List<Node<T>> Children { get; } = new List<Node<T>>();
        
        internal StringBuilder AscIIBuilder { get; } = new StringBuilder();
        public string AscII => AscIIBuilder.ToString();
    }
}