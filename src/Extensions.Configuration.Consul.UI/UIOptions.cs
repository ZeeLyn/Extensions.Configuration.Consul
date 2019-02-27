namespace Extensions.Configuration.Consul.UI
{
    public class UIOptions
    {
        public UIOptions(string ip = "*", int port = 5342)
        {
            IP = ip;
            Port = port;
        }

        public string IP { get; set; } = "*";
        public int Port { get; set; } = 5342;
    }
}
