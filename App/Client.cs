using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace App
{
    internal class Client
    {
        public int Id { get; set; } = GeneratorID.GetClientID();
        public string Name { get; set; } = "Buddy";
        public TcpClient TcpClient { get; set; }
        public NetworkStream Stream { get => TcpClient.GetStream(); }

        public Client()
        {

        }
        
        public Client(int Id, string Name, TcpClient tcpClient)
        {
            this.Id = Id;
            this.Name = Name;
            this.TcpClient = tcpClient;
        }
    }
}
