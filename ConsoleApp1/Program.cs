using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace ConsoleApp1
{
    class Program
    {
        static void Main(string[] args)
        {
            var dic = new Dictionary<string, string>
            {
                ["AppSettings/App1/S1/Client:Url"] = "http://127.0.0.1",
                ["AppSettings/App1/S1/Client:SSL:PWD"] = "123",
                ["AppSettings/App1/S2/Client:Port"] = "100",
                ["AppSettings/App2/S3/Client:Port"] = "90",
                ["A:B:C"] = "1"
            };

            var keyNodes = FormatKey.FormatFolder(dic);


            Console.WriteLine(JsonConvert.SerializeObject(keyNodes));


            //var hasFolderKeys = dic.Where(p => !string.IsNullOrWhiteSpace(p.Key) && p.Key.Contains("/")).Select(p => new
            //{
            //    Folder = p.Key.Substring(0, p.Key.LastIndexOf('/')),
            //    Key = p.Key.Substring(p.Key.LastIndexOf('/') + 1, p.Key.Length - p.Key.LastIndexOf('/') - 1),
            //    p.Value
            //}).GroupBy(p => p.Folder).Select(p => new
            //{
            //    Folder = p.Key,
            //    FolderArray = p.Key.Split('/'),
            //    Children = p.ToDictionary(key => key.Key, val => val.Value)
            //}).Select(p => BuildFolder(p.Folder, p.Children)).ToList();
            //foreach (var key in hasFolderKeys)
            //{

            //}
            //Console.WriteLine(JsonConvert.SerializeObject(hasFolderKeys));

            //Console.WriteLine(JsonConvert.SerializeObject(BuildKey("Client:Port:A", "1234")));

            //var configuration = new ConfigurationBuilder().AddInMemoryCollection(dic).Build();
            //Console.WriteLine(JsonConvert.SerializeObject(configuration.GetSection("AppSettings/App2/Client:Port")));
            //configuration.GetSection("A:B:C")["D"] = "999";
            //Console.WriteLine(JsonConvert.SerializeObject(configuration.GetSection("A:B:C:D")));

            //Console.WriteLine(JsonConvert.SerializeObject("AppSettings/Client:Url".Split(new[] { '/', ':' }, StringSplitOptions.RemoveEmptyEntries)));
            //Format(dic.Keys.ToList());
            Console.ReadKey();
        }


        private static List<KeyNode> RecurrenceFolder(IEnumerable<IConfigurationSection> sections, List<KeyNode> keyNodes)
        {
            if (keyNodes == null)
                keyNodes = new List<KeyNode>();
            foreach (var section in sections)
            {

            }

            return keyNodes;
        }

        private static KeyNode BuildFolder(string folderString, Dictionary<string, string> keyValue)
        {
            using (var enumerator = folderString.Split('/').AsEnumerable().GetEnumerator())
            {
                KeyNode parentNode = null;
                KeyNode currentNode = null;
                var key = "";
                while (enumerator.MoveNext())
                {
                    key += $"{enumerator.Current}/";

                    var node = new KeyNode
                    {
                        Type = NodeType.Folder,
                        Key = key
                    };

                    if (parentNode == null)
                    {
                        parentNode = node;
                        currentNode = node;
                    }
                    else
                    {
                        if (currentNode.Children == null)
                            currentNode.Children = new List<KeyNode>();
                        currentNode.Children.Add(node);
                        currentNode = node;
                    }

                }
                if (currentNode != null)
                {
                    currentNode.Children = new List<KeyNode>();
                    foreach (var (s, value) in keyValue)
                    {
                        currentNode.Children.Add(BuildKey(s, value));
                    }
                }

                return parentNode;
            }
        }

        private static KeyNode BuildKey(string keyString, string value)
        {
            using (var enumerator = keyString.Split(':').AsEnumerable().GetEnumerator())
            {
                KeyNode parentNode = null;
                KeyNode currentNode = null;
                var key = "";
                while (enumerator.MoveNext())
                {
                    key += $"{enumerator.Current}:";

                    var node = new KeyNode
                    {
                        Type = NodeType.PartKey,
                        Key = key.TrimEnd(':')
                    };

                    if (parentNode == null)
                    {
                        parentNode = node;
                        currentNode = node;
                    }
                    else
                    {
                        if (currentNode.Children == null)
                            currentNode.Children = new List<KeyNode> { node };
                        else
                            currentNode.Children.Add(node);
                        currentNode = node;
                    }

                }

                if (currentNode != null)
                {
                    currentNode.Type = NodeType.FullKey;
                    currentNode.Value = value;
                }

                return parentNode;
            }
        }

        private static void Format(List<string> data)
        {
            var hasFolderKeys = data.Where(p => !string.IsNullOrWhiteSpace(p) && p.Contains("/")).ToList();
            var nonFolderKeys = data.Where(p => !string.IsNullOrWhiteSpace(p)).Except(hasFolderKeys).ToList();
            var r = hasFolderKeys.Select(p => new
            {
                Folder = p.Substring(0, p.IndexOf('/')),
                Key = p.Substring(p.IndexOf('/') + 1, p.Length - p.IndexOf('/') - 1)
            });

            Console.WriteLine(JsonConvert.SerializeObject(r));

            //Console.WriteLine(JsonConvert.SerializeObject(hasFolderKeys));

            //Console.WriteLine(JsonConvert.SerializeObject(nonFolderKeys));
        }
    }
    public class KeyNode
    {
        public string KeyName { get; set; }

        public string Key { get; set; }

        public NodeType Type { get; set; }

        public override string ToString()
        {
            switch (Type)
            {
                case NodeType.Folder:
                    return Key + "/";
                case NodeType.PartKey:
                    return Key + ":";
                default:
                    return Key;
            }
        }

        public string Value { get; set; }
        public List<KeyNode> Children { get; set; }
    }

    public enum NodeType
    {
        Folder,
        PartKey,
        FullKey
    }
}
