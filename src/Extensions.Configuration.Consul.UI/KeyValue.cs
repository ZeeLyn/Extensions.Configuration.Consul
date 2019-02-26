using System.ComponentModel.DataAnnotations;

namespace Extensions.Configuration.Consul.UI
{

    public class KeyValue_Key
    {
        [Required(ErrorMessage = "Key cannot be empty")]
        public string Key { get; set; }
    }

    public class KeyValue_Value : KeyValue_Key
    {


        public string Value { get; set; }
    }
}
