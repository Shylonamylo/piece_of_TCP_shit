using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection.Metadata;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace App
{
    internal class ClientWorker
    {
        string host = "127.0.0.0";
        int port = 0;
        Client client = new();

        public ClientWorker() 
        {
            Console.Clear();

            do
            {
                Console.Write("Введите IP адрес сервера: ");
                host = Console.ReadLine();
                Console.Clear();
            } while (!IPAddress.TryParse(host, out _));

            Console.Clear();

            Console.Write("Введите порт: ");

            while (!int.TryParse(Console.ReadLine(), out port))
            {
                Console.Clear();
                Console.Write("Введите порт: ");
            };

            Console.Write("Введите ваш ник: ");
            client.Name = Console.ReadLine();

            Console.Clear();
        }

        public async Task StartAsync()
        {
            client.TcpClient = new TcpClient();
            client.TcpClient.Connect(host, port);

            Message IDFromServerMessageObj = new();

            try
            {
                IDFromServerMessageObj = GetMessage();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            if(IDFromServerMessageObj.MessageType == MessageType.ClientData)
            {
                client.Id = int.Parse(IDFromServerMessageObj.Content);
            }

            Message NameToServerMessageObj = new Message(MessageType.ClientData, client.Id, client.Name);
            string NameToServerMessage = JsonSerializer.Serialize(NameToServerMessageObj);

            try
            {
                await SendMessageAsync(NameToServerMessage);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            Thread messageListener = new Thread(GetMessages);
            messageListener.Start();

            Thread TypeWriter = new Thread(() => TypeThread());
            TypeWriter.Start();
        }

        private async void TypeThread()
        {
            while (true)
            {
                string content = Console.ReadLine();
                Message message = new(MessageType.Text, client.Id, content);
                await SendMessageAsync(JsonSerializer.Serialize(message));
            }
        }

        private async Task SendMessageAsync(string message)
        {
            try
            {
                var dataToSend = Encoding.UTF8.GetBytes(message);

                await client.Stream.WriteAsync(dataToSend);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private Message? GetMessage()
        {
            try
            {
                var responseData = new byte[512];
                var response = new StringBuilder();
                int bytes;
                do
                {
                    bytes = client.Stream.Read(responseData);

                    response.Append(Encoding.UTF8.GetString(responseData, 0, bytes));
                }
                while (client.Stream.DataAvailable);

                Message result = JsonSerializer.Deserialize<Message>(response.ToString());

                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }

        public void GetMessages()
        {
            while (true)
            {
                Message message = GetMessage();

                if(message.MessageType == MessageType.Text)
                {
                    Console.WriteLine(message.Content);
                }
            }
        }

        public void Stop()
        {
            client.Stream.Close();
            client.Stream.Dispose();
            client.TcpClient.Close();
            client.TcpClient.Dispose();
        }
    }
}
