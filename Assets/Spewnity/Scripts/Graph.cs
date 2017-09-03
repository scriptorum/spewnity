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

        // Creates a directional edge between two nodes, if one doesn't already exist.
        // Otherwise, updates the edge cost.
        public void ConnectTo(GraphNode<T> from, GraphNode<T> to, int cost)
        {
            int fromIndex = from.neighbors.FindIndex(neighbor => neighbor == to);
            if (fromIndex == -1)
            {
                from.neighbors.Add(to);
                from.costs.Add(cost);
            }
            else from.costs[fromIndex] = cost;
        }

        // Creates a unidirectional edge between two nodes, if one doesn't already exist
        // Otherwise, updates the edge cost.
        public void Connect(GraphNode<T> from, GraphNode<T> to, int cost)
        {
            ConnectTo(from, to, cost);
            ConnectTo(to, from, cost);
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

    public class GraphNode<T>
    {
        public T value;
        public List<GraphNode<T>> neighbors = null;
        public List<int> costs;

        public GraphNode(T value = default(T), ICollection<GraphNode<T>> neighbors = null)
        {
            this.value = value;
            this.neighbors = neighbors == null ? new List<GraphNode<T>>() : new List<GraphNode<T>>(neighbors);
            costs = new List<int>(this.neighbors.Count);
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

    public class BinaryNode<T>
    {
        public T value;
        public List<BinaryNode<T>> neighbors = null;

        public BinaryNode(T value = default(T), BinaryNode<T> left = null, BinaryNode<T> right = null)
        {
            if (left != null || right != null)
            {
                neighbors = new List<BinaryNode<T>>();
                neighbors.Add(left);
                neighbors.Add(right);
            }
            else neighbors = null;
        }

        public BinaryNode<T> Left
        {
            get { return neighbors == null ? null : neighbors[0]; }
            set
            {
                if (neighbors == null)
                    neighbors = new List<BinaryNode<T>>(2);
                neighbors[0] = value;
            }
        }

        public BinaryNode<T> Right
        {
            get { return neighbors == null ? null : neighbors[1]; }
            set
            {
                if (neighbors == null)
                    neighbors = new List<BinaryNode<T>>(2);
                neighbors[1] = value;
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