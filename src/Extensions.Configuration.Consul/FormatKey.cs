using System;
using System.Collections.Generic;
using System.Text;

namespace Extensions.Configuration.Consul
{
    public class FormatKey
    {
        public void Format()
        {
            var dic = new Dictionary<string, string>
            {
                ["AppSettings/Client:Url"] = "http://127.0.0.1",
                ["AppSettings/Client:Port"] = "90"
            };
            foreach (var key in dic.Keys)
            {

            }
        }
    }

    public class KeyNode
    {
        public string KeyName { get; set; }

        public string Key { get; set; }

        public List<KeyNode> Children { get; set; }
    }
}
