using System.Collections.Generic;
using System.Linq;

namespace Extensions.Configuration.Consul.UI
{
    public class FormatKey
    {
        public static List<KeyNode> FormatFolder(Dictionary<string, string> keyValuePairs)
        {
            var data = keyValuePairs.Where(p => !string.IsNullOrWhiteSpace(p.Key) && p.Key.Contains("/"))
                .Select(p => p.Key).Select(p => new
                {
                    Folder = p.Substring(0, p.LastIndexOf('/')),
                    FullKey = p,
                }).ToList();
            #region Folders
            var folders = data.GroupBy(p => p.Folder).Select(p => new NodeInfo { Array = p.Key.Split('/').ToList(), FullName = p.Key }).ToList();
            var items = folders.FindAll(p => p.Array.Count > 0).Select(p => p.Array[0]).Distinct().ToList();
            var nodes = new List<KeyNode>();
            foreach (var item in items)
            {
                var node = new KeyNode
                {
                    Id = item + "/",
                    Name = item,
                    Type = NodeType.Folder
                };
                nodes.Add(node);
                var currentKeys = keyValuePairs.Where(p => p.Key.Contains(node.Id) && p.Key != node.Id && !p.Key.Substring(node.Id.Length, p.Key.Length - node.Id.Length).Contains("/")).Select(p => new NodeInfo
                {
                    FullName = p.Key,
                    Array = p.Key.Substring(node.Id.Length, p.Key.Length - node.Id.Length).Split(':').ToList()
                }).Where(p => p.Array.Count > 0).ToList();
                if (currentKeys.Any())
                    RecurrenceKeys(keyValuePairs, currentKeys, node, 0);
                RecurrenceFolder(keyValuePairs, folders, node, 1);
            }
            #endregion

            #region Keys
            var keys = keyValuePairs.Where(p => !string.IsNullOrWhiteSpace(p.Key) && !p.Key.Contains("/")).Select(p =>
                new NodeInfo
                {
                    Array = p.Key.Split(':').ToList(),
                    FullName = p.Key
                }).ToList();
            var keys0 = keys.FindAll(p => p.Array.Count > 0).Select(p => p.Array[0]).Distinct().ToList();
            foreach (var key in keys0)
            {
                var isLast = keys.Any(p => p.FullName == key);
                var node = new KeyNode
                {
                    Id = key + (isLast ? "" : ":"),
                    Name = key,
                    Type = isLast ? NodeType.FullKey : NodeType.PartKey,
                    Text = isLast ? keyValuePairs[key] : null
                };
                nodes.Add(node);
                RecurrenceKeys(keyValuePairs, keys, node, 1);
            }
            #endregion
            return nodes;
        }

        public static void RecurrenceFolder(Dictionary<string, string> keyValuePairs, List<NodeInfo> folders, KeyNode keyNode, int level = 0)
        {
            var items = folders.FindAll(p => p.Array.Count > level && p.FullName != keyNode.Id && p.FullName.StartsWith(keyNode.Id)).Select(p => p.Array[level]).Distinct().ToList();
            if (items.Any())
            {
                if (keyNode.Nodes == null)
                    keyNode.Nodes = new List<KeyNode>();
            }
            else
            {
                var keys = keyValuePairs.Where(p => p.Key.StartsWith(keyNode.Id) && p.Key != keyNode.Id).Select(p => new NodeInfo
                {
                    FullName = p.Key,
                    Array = p.Key.Substring(keyNode.Id.Length, p.Key.Length - keyNode.Id.Length).Split(':').ToList()
                }).ToList();
                var current = keys.Select(p => p.Array[0]).Distinct().ToList();
                if (current.Any())
                {
                    keyNode.Nodes = new List<KeyNode>();
                    foreach (var key in current)
                    {
                        var fullKey = keyNode.Id;
                        if (keyNode.Type == NodeType.Folder)
                            fullKey += key;
                        else if (keyNode.Type == NodeType.FullKey)
                            fullKey += ":" + key;
                        var isLast = keyValuePairs.Any(p => p.Key == key);
                        var node = new KeyNode
                        {
                            Id = fullKey,
                            Name = key,
                            Type = isLast ? NodeType.FullKey : NodeType.PartKey,
                            Text = isLast ? keyValuePairs[key] : null
                        };
                        keyNode.Nodes.Add(node);
                        RecurrenceKeys(keyValuePairs, keys, node, 1);
                    }
                }
            }

            foreach (var item in items)
            {
                var prefix = keyNode.Id + item + "/";
                var node = new KeyNode
                {
                    Type = NodeType.Folder,
                    Id = prefix,
                    Name = item
                };
                keyNode.Nodes.Add(node);
                RecurrenceFolder(keyValuePairs, folders, node, level + 1);
            }
        }

        public static void RecurrenceKeys(Dictionary<string, string> keyValuePairs, List<NodeInfo> keys,
            KeyNode keyNode, int level = 0)
        {
            var items = keys.FindAll(p => p.Array.Count > level && p.FullName.StartsWith(keyNode.Id)).Select(p => p.Array[level]).Distinct().ToList();
            if (items.Any())
            {
                if (keyNode.Nodes == null)
                    keyNode.Nodes = new List<KeyNode>();
            }
            else
            {
                keyNode.Id = keyNode.Id.TrimEnd(':');
                keyNode.Text = keyValuePairs[keyNode.Id];
                keyNode.Type = NodeType.FullKey;
            }
            foreach (var item in items)
            {
                var key = keyNode.Id + (keyNode.Type == NodeType.FullKey ? ":" : "") + item;
                var isLast = keyValuePairs.Any(p => p.Key == key);
                var node = new KeyNode
                {
                    Type = isLast ? NodeType.FullKey : NodeType.PartKey,
                    Id = key,
                    Name = item,
                    Text = isLast ? keyValuePairs[key] : null
                };
                keyNode.Nodes.Add(node);
                RecurrenceKeys(keyValuePairs, keys, node, level + 1);
            }
        }
    }

    public class NodeInfo
    {
        public string FullName { get; set; }

        public List<string> Array { get; set; }
    }
}
