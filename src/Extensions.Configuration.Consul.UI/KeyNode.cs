using System;
using System.Collections.Generic;
using System.Text;

namespace Extensions.Configuration.Consul.UI
{
    public class KeyNode
    {
        public string Name { get; set; }

        public string Id { get; set; }

        public NodeType Type { get; set; }

        public override string ToString()
        {
            return Id;
        }

        public string Text { get; set; }
        public List<KeyNode> Nodes { get; set; }

        public object State { get; set; } = new
        {
            expanded = true
        };
    }

    public enum NodeType
    {
        Folder,
        PartKey,
        FullKey
    }
}
