namespace App
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Выберите кем хотите быть:");

            Console.WriteLine("1: Клиент");

            Console.WriteLine("2: Сервер");

            int.TryParse(Console.ReadLine(), out int selected);

            switch (selected)
            {
                case 1: 
                    
                    ClientWorker client = new ClientWorker();
                    client.StartAsync();
                    
                break;

                case 2: 
                    
                    ServerWorker server = new ServerWorker();
                    server.Start();
                    
                break;

            }
        }
    }
}
