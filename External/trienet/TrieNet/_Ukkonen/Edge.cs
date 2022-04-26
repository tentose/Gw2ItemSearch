using System.Runtime.Serialization;

namespace Gma.DataStructures.StringSearch
{
    [DataContract]
    internal class Edge<T>
    {
        public Edge(string label, Node<T> target)
        {
            this.Label = label;
            this.Target = target;
        }

        [DataMember()]
        public string Label { get; set; }

        [DataMember()]
        public Node<T> Target { get; private set; }
    }
}