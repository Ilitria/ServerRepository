using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.IO;

namespace Server
{
    public class ClientObject
    {
        public string Id { get; private set; }
        public NetworkStream Stream { get; private set; }
        string userName;
        TcpClient client;
        ServerObject server;

        public ClientObject(TcpClient tcpClient, ServerObject serverObject)
        {
            Id = Guid.NewGuid().ToString();
            client = tcpClient;
            server = serverObject;
            serverObject.AddConnection(this);
        }

        //Процесс взаимодействия с клиентов
        public void Process()
        {
            try
            {
                Stream = client.GetStream();
                string message = GetMessage();
                userName = message;

                message = $"{userName} вошел в чат";
                server.BroadcastMessage(message, this.Id);
                Console.WriteLine(message);
                SaveLog(message);
                //Получение сообщений
                while (true)
                {
                    try
                    {
                        message = GetMessage();
                        if (message == "")
                        {
                            server.CheckMessage(message, this.Id);
                            message = $"{userName} покинул чат";
                            Console.WriteLine(message);
                            SaveLog(message);
                            server.BroadcastMessage(message, this.Id);
                            break;
                        }
                        
                        message = $"{userName}: {message}";
                        Console.WriteLine(message);
                        SaveLog(message);
                        server.BroadcastMessage(message, this.Id);
                    }
                    catch
                    {
                        message = $"{userName} покинул чат";
                        Console.WriteLine(message);
                        SaveLog(message);
                        server.BroadcastMessage(message, this.Id);
                        break;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            finally
            {
                server.RemoveConnection(this.Id);
                Close();
            }
        }
        //Запись сообщений в файл 
        public void SaveLog(string message)
        {
            StreamWriter sw = new StreamWriter("server.txt", true);
            string str = DateTime.Now + " " + message;
            sw.WriteLine(str);
            sw.Close();
        }

        //Получение сообщения от пользователя
        public string GetMessage()
        {
            byte[] data = new byte[64]; 
            StringBuilder builder = new StringBuilder();
            int bytes = 0;
            do
            {
                bytes = Stream.Read(data, 0, data.Length);
                builder.Append(Encoding.Unicode.GetString(data, 0, bytes));
            }
            while (Stream.DataAvailable);

            return XorDe_En_cryption(builder.ToString());
        }
        //Шифровка/расшифровка
        static public string XorDe_En_cryption(string message)
        {
            int key = DateTime.Now.Year * DateTime.Now.Hour / DateTime.Now.Day / DateTime.Now.Month;
            string encMessage = "";
            for (int i = 0; i < message.Length; i++)
            {
                encMessage += (char)(message[i] ^ key);
            }
            return encMessage;
        }

        //Закрытие подключения
        public void Close()
        {
            if (Stream != null)
                Stream.Close();
            if (client != null)
                client.Close();
        }
    }
}
