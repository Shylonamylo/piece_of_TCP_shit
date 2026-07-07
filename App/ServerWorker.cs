using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace App
{
    
    internal class ServerWorker
    {
        List<Client> clients = new();
        TcpListener server;
        int port;
        bool working = true;

        List<Message> messagePool = new List<Message>();

        public ServerWorker()
        {
            Console.Clear();

            Console.Write("Введите порт: ");

            while (!int.TryParse(Console.ReadLine(), out port))
            {
                Console.Clear();
                Console.Write("Введите порт: ");
            };
        }
        internal void Start()
        {
            Console.Clear();
            server = new(System.Net.IPAddress.Any, port);
            server.Start();
            Console.WriteLine("Сервер запущен!");

            var connector = new Thread(ClientConnector);
            connector.Start();

            var messagePoolWorker = new Thread(MessagePoolWorker);
            messagePoolWorker.Start();
        }
        private void MessagePoolWorker()
        {
            while (working)
            {
                if(messagePool.Count > 0)
                {
                    Message message = messagePool[0];
                    if (message == null || clients.Count == 0 || string.IsNullOrEmpty(message.Content))
                    {
                        messagePool.RemoveAt(0);
                        continue;
                    }
                    if (message.MessageType == MessageType.Text)
                    {
                        try
                        {
                            string answerContent = $"{clients.FirstOrDefault(a => a.Id == message.SenderId).Name}: {message.Content}";
                            Message answerMessage = new Message(MessageType.Text, 0, answerContent);
                            SendMessageToAllClients(answerMessage);
                            messagePool.RemoveAt(0);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message);
                            messagePool.RemoveAt(0);
                        }
                    }
                }
            }
        }
        internal void SendMessageToAllClients(Message message)
        {
            var data = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));
            foreach (var client in clients)
            {
                client.Stream.Write(data);
            }
        }
        internal void SendMessageToClient(Message message, Client client)
        {
            try
            {
                client.Stream.Write(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message)));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private Message GetMessage(Client client)
        {
            var responseData = new byte[512];
            var response = new StringBuilder();
            try
            {
                int bytes;
                do
                {
                    try
                    {
                        bytes = client.Stream.Read(responseData);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                        return null;
                    }

                    response.Append(Encoding.UTF8.GetString(responseData, 0, bytes));
                }
                while (client.Stream.DataAvailable);

                return JsonSerializer.Deserialize<Message>(response.ToString());
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }

        internal async void ClientListener(Client client)
        {
            while (working)
            {

                try
                {
                    Message message = GetMessage(client);
                    if(message == null)
                    {
                        clients.Remove(client);
                        Message disconnectMessage = new(MessageType.Text, 0, $"{client.Name} отключился");
                        SendMessageToAllClients(disconnectMessage);
                        return;
                    }

                    messagePool.Add(message);
                    Console.WriteLine($"{client.Name} отправил {messagePool[messagePool.Count - 1].Content}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    clients.Remove(client);
                    return;
                }
            }
        }
        private void ClientConnector()
        {
            while (working)
            {
                TcpClient tcpClient = server.AcceptTcpClient();

                Client initClient = new()
                {
                    TcpClient = tcpClient,
                    Name = "Buddy" + (clients.Count + 1)
                };

                Message message = new(MessageType.ClientData, 0, $"{initClient.Id}");

                SendMessageToClient(message, initClient);

                var initMessage = GetMessage(initClient);

                if(initMessage.MessageType == MessageType.ClientData)
                {
                    initClient.Name = initMessage.Content;
                }

                Thread listener = new(() => ClientListener(initClient));
                listener.Start();

                clients.Add(initClient);

                Console.WriteLine($"Подключился клиент: {initClient.TcpClient.Client.RemoteEndPoint}, его имя: {initClient.Name}");
                Message notificationMessage = new(MessageType.Text, 0, $"Присоединился {initClient.Name}");
                SendMessageToAllClients(notificationMessage);
            }
        }
    }
}
