using System.ComponentModel.DataAnnotations;

namespace Extensions.Configuration.Consul.UI
{
    public class KeyValue
    {
        [Required(ErrorMessage = "Key cannot be empty")]
        public string Key { get; set; }

        public string Value { get; set; }
    }
}
