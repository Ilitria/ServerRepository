using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net.Sockets;
using System.Net;

namespace Server
{
    public class ServerObject
    {
        static TcpListener tcpListener; 
        List<ClientObject> clients = new List<ClientObject>(); 

        public void AddConnection(ClientObject clientObject)
        {
            clients.Add(clientObject);
        }
        //Отключение клиента
        public void RemoveConnection(string id)
        {
            ClientObject client = clients.FirstOrDefault(c => c.Id == id);
            if (client != null)
                clients.Remove(client);
        }
        //Прослушивание входящих подключений
        public void Listen()
        {
            try
            {
                tcpListener = new TcpListener(IPAddress.Any, 5050);
                tcpListener.Start();
                Console.WriteLine("Сервер запущен. Ожидание подключений...");

                while (true)
                {
                    TcpClient tcpClient = tcpListener.AcceptTcpClient();

                    ClientObject clientObject = new ClientObject(tcpClient, this);
                    Thread clientThread = new Thread(new ThreadStart(clientObject.Process));
                    clientThread.Start();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Disconnect();
            }
        }
        //Проверка сообщения на пустоту
        public void CheckMessage(string message, string id)
        {
            if (message == "")
            {
                for (int i = 0; i <= clients.Count; i++)
                {
                    if (clients[i].Id == id)
                    {
                        RemoveConnection(id);
                    }
                }
            }
            
        }
        // Отправка сообщений клиентам
        public void BroadcastMessage(string message, string id)
        {
            message = ClientObject.XorDe_En_cryption(DateTime.Now.ToShortTimeString() + " " + message);//шифрование сообщения
            byte[] data = Encoding.Unicode.GetBytes(message);
            for (int i = 0; i < clients.Count; i++)
            {
                if (clients[i].Id != id)
                {
                    clients[i].Stream.Write(data, 0, data.Length);
                }
            }
        }

        //Отключение клиентов
        public void Disconnect()
        {
            tcpListener.Stop();

            for (int i = 0; i < clients.Count; i++)
            {
                clients[i].Close();
            }
            Environment.Exit(0);
        }
    }
}
