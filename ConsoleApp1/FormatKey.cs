using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ConsoleApp1
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
                });
            var folders = data.GroupBy(p => p.Folder).Select(p => new NodeInfo { FolderArray = p.Key.Split('/').ToList(), FullName = p.Key }).ToList();
            var items = folders.FindAll(p => p.FolderArray.Count >= 0).Select(p => p.FolderArray[0]).Distinct().ToList();
            var nodes = new List<KeyNode>();
            foreach (var item in items)
            {
                var node = new KeyNode
                {
                    Key = item + "/",
                    KeyName = item,
                    Type = NodeType.Folder
                };
                nodes.Add(node);
                RecurrenceFolder(keyValuePairs, folders, node, 1);
            }
            return nodes;
        }

        public static void RecurrenceFolder(Dictionary<string, string> keyValuePairs, List<NodeInfo> folders, KeyNode keyNode, int level = 0)
        {
            var items = folders.FindAll(p => p.FolderArray.Count > level && p.FullName.StartsWith(keyNode.Key)).Select(p => p.FolderArray[level]).Distinct().ToList();
            if (items.Any())
                keyNode.Children = new List<KeyNode>();
            else
            {
                var keys = keyValuePairs.Where(p => p.Key.StartsWith(keyNode.Key)).Select(p => new NodeInfo
                {
                    FullName = p.Key,
                    FolderArray = p.Key.Substring(keyNode.Key.Length, p.Key.Length - keyNode.Key.Length).Split(':').ToList()
                }).ToList();
                var current = keys.Select(p => p.FolderArray[0]).Distinct().ToList();
                if (current.Any())
                {
                    keyNode.Children = new List<KeyNode>();
                    foreach (var key in current)
                    {
                        var node = new KeyNode
                        {
                            Key = keyNode.Key + key + ":",
                            KeyName = key,
                            Type = NodeType.PartKey
                        };
                        keyNode.Children.Add(node);
                        RecurrenceKeys(keyValuePairs, keys, node, 1);
                    }
                }
            }

            foreach (var item in items)
            {
                var prefix = keyNode.Key + item + "/";
                var node = new KeyNode
                {
                    Type = NodeType.Folder,
                    Key = prefix,
                    KeyName = item
                };
                keyNode.Children.Add(node);
                RecurrenceFolder(keyValuePairs, folders, node, level + 1);
            }
        }

        public static void RecurrenceKeys(Dictionary<string, string> keyValuePairs, List<NodeInfo> folders,
            KeyNode keyNode, int level = 0)
        {
            var items = folders.FindAll(p => p.FolderArray.Count > level && p.FullName.StartsWith(keyNode.Key)).Select(p => p.FolderArray[level]).Distinct().ToList();
            if (items.Any())
                keyNode.Children = new List<KeyNode>();
            else
            {
                keyNode.Key = keyNode.Key.TrimEnd(':');
                keyNode.Value = keyValuePairs[keyNode.Key];
                keyNode.Type = NodeType.FullKey;
            }
            foreach (var item in items)
            {
                var prefix = keyNode.Key + item + ":";
                var node = new KeyNode
                {
                    Type = NodeType.PartKey,
                    Key = prefix,
                    KeyName = item
                };
                keyNode.Children.Add(node);
                RecurrenceKeys(keyValuePairs, folders, node, level + 1);
            }
        }
    }

    public class NodeInfo
    {
        public string FullName { get; set; }

        public List<string> FolderArray { get; set; }
    }
}
