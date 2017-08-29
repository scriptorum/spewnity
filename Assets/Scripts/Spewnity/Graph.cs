using System.Collections;
using System.Collections.Generic;

// Just messing around with some graph theory
// Learning a little from: https://msdn.microsoft.com/en-us/library/ms379570(v=vs.80).aspx
// You probably want to use this instead: http://yaccconstructor.github.io/QuickGraph/
namespace Spewnity
{
    public class Graph<T> : IEnumerable<GraphNode<T>>
    {
        public List<GraphNode<T>> nodeSet;

        public Graph(List<GraphNode<T>> nodeSet = null)
        {
            this.nodeSet = nodeSet == null ? new List<GraphNode<T>>() : nodeSet;
        }

        public Graph<T> Add(GraphNode<T> value)
        {
            nodeSet.Add(value);
            return this;
        }

        public Graph<T> Add(T value)
        {
            return Add(new GraphNode<T>(value));
        }

        // TODO This doesn't check to see if there's already a directed edge
        public void AddDirectedEdge(GraphNode<T> from, GraphNode<T> to, int cost)
        {
            from.neighbors.Add(to);
            from.costs.Add(cost);
        }

        public void AddUndirectedEdge(GraphNode<T> from, GraphNode<T> to, int cost)
        {
            from.neighbors.Add(to);
            from.costs.Add(cost);

            to.neighbors.Add(from);
            to.costs.Add(cost);
        }

        public void SetUnidirectedEdgeCost(GraphNode<T> from, GraphNode<T> to, int cost)
        {
            int index = from.neighbors.FindIndex(neighbor => neighbor == to);
            from.costs[index] = cost;
            index = to.neighbors.FindIndex(neighbor => neighbor == from);
            to.costs[index] = cost;
        }

        public bool Contains(T value)
        {
            return nodeSet.FindIndex((GraphNode<T> node) => node.value.Equals(value)) >= 0;
        }

        public bool Remove(T value)
        {
            // Remove node
            GraphNode<T> nodeToRemove = nodeSet.Find((GraphNode<T> node) => node.value.Equals(value));
            if (nodeToRemove == null) return false;
            nodeSet.Remove(nodeToRemove);

            // Remove connections to this node
            foreach (GraphNode<T> node in nodeSet)
            {
                int index = node.neighbors.IndexOf(nodeToRemove);
                if (index != -1)
                {
                    node.neighbors.RemoveAt(index);
                    node.costs.RemoveAt(index);
                }
            }
            return true;
        }

        public GraphNode<T> this [int index]
        {
            get { return nodeSet[index]; }
            set { nodeSet[index] = value; }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public IEnumerator<GraphNode<T>> GetEnumerator()
        {
            return ((IEnumerable<GraphNode<T>>) nodeSet).GetEnumerator();
        }
    }

    public class GraphNode<T> : Node<T>
    {
        public List<int> costs;

        public GraphNode(T value = default(T), ICollection<Node<T>> neighbors = null) : base(value, neighbors)
        {
            int count = neighbors == null ? 0 : neighbors.Count;
            costs = new List<int>(count);
        }
    }

    ////////////////////////////////////////////////////////////////////////

    public class BinaryTree<T>
    {
        public BinaryNode<T> root;

        public BinaryTree()
        {
            Clear();
        }

        public virtual void Clear()
        {
            root = null;
        }
    }

    public class BinaryNode<T> : Node<T>
    {
        public BinaryNode(T value = default(T), BinaryNode<T> left = null, BinaryNode<T> right = null)
        {
            if (left != null || right != null)
            {
                base.neighbors = new List<Node<T>>();
                base.neighbors.Add(left);
                base.neighbors.Add(right);
            }
            else base.neighbors = null;
        }

        public BinaryNode<T> Left
        {
            get { return base.neighbors == null ? null : (BinaryNode<T>) base.neighbors[0]; }
            set
            {
                if (base.neighbors == null)
                    base.neighbors = new List<Node<T>>(2);
                base.neighbors[0] = value;
            }
        }

        public BinaryNode<T> Right
        {
            get { return base.neighbors == null ? null : (BinaryNode<T>) base.neighbors[1]; }
            set
            {
                if (base.neighbors == null)
                    base.neighbors = new List<Node<T>>(2);
                base.neighbors[1] = value;
            }
        }
    }

    ////////////////////////////////////////////////////////////////////////

    public class Node<T>
    {
        public T value;
        public List<Node<T>> neighbors = null;

        public Node(T value = default(T), ICollection<Node<T>> neighbors = null)
        {
            this.value = value;
            this.neighbors = neighbors == null ? new List<Node<T>>() : new List<Node<T>>(neighbors);
        }
    }
}