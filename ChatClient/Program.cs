using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ChatClient
{
    class Program
    {
        static string userName;
        // static string host = "192.168.0.108";
        //private const int port = 8888;
        static string host;
        static int port;
        static TcpClient client;
        static NetworkStream stream;

        static void Main(string[] args)
        {
            Console.Write("Введите свое имя: ");
            userName = Console.ReadLine();
            //Вообще, вводить необходимо следующий хост: 127.0.0.1
            // это значит что подключаться будет на локальной машине,
            // Если же необходимо запустить на каком-то устройстве, а с другого получать доступ,
            // необходимо просто вводить IP машины, на которой запущен сервер.
            Console.Write("Введите IP адрес (Пример: 192.168.0.108): ");
            host = Console.ReadLine();

            if (!RightIp(host))
            {
                Console.ReadLine();
                return;
            }

            Console.Write("Введите номер порта: ");
            port = int.Parse(Console.ReadLine());

            if (!PortEnabled(host, port))
            {
                Console.ReadLine();
                return;
            }

            client = new TcpClient();
            try
            {
                client.Connect(host, port); //подключение клиента
                stream = client.GetStream(); // получаем поток

                string message = userName;
                byte[] data = Encoding.Unicode.GetBytes(message);
                stream.Write(data, 0, data.Length);

                // запускаем новый поток для получения данных
                Thread receiveThread = new Thread(new ThreadStart(ReceiveMessage));
                receiveThread.Start(); //старт потока
                Console.WriteLine($"Добро пожаловать, {userName}");
                SendMessage();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Возникла ошибка: "+ex.Message);
            }
            finally
            {
                Disconnect();
            }
        }
        // отправка сообщений
        static void SendMessage()
        {
            Console.WriteLine("Введите сообщение: ");

            while (true)
            {
                string message = Console.ReadLine();
                byte[] data = Encoding.Unicode.GetBytes(message);
                stream.Write(data, 0, data.Length);
            }
        }
        // получение сообщений
        static void ReceiveMessage()
        {
            while (true)
            {
                try
                {
                    byte[] data = new byte[64]; // буфер для получаемых данных
                    StringBuilder builder = new StringBuilder();
                    int bytes = 0;
                    do
                    {
                        bytes = stream.Read(data, 0, data.Length);
                        builder.Append(Encoding.Unicode.GetString(data, 0, bytes));
                    }
                    while (stream.DataAvailable);

                    string message = builder.ToString();
                    //Console.Beep();
                    if (Console.CursorLeft > 0)
                    {
                        Console.WriteLine(" <- незаконченое предложение помещено в буфер,\n дополнительно набирать не надо!");
                    }
                    Console.WriteLine(message);//вывод сообщения
                }
                catch
                {
                    Console.WriteLine("Подключение прервано!"); //соединение было прервано
                    Console.ReadLine();
                    Disconnect();
                }
            }
        }

        static void Disconnect()
        {
            if (stream != null)
                stream.Close();//отключение потока
            if (client != null)
                client.Close();//отключение клиента
            Environment.Exit(0); //завершение процесса
        }

        private static bool RightIp(string ip)
        {
            try
            {
                Console.WriteLine("Загрузка...");
                IPAddress testIP = IPAddress.Parse(ip);
                Ping pingSender = new Ping();
                PingReply reply = pingSender.Send(testIP);

                if (reply.Status == IPStatus.Success)
                {
                    Console.WriteLine("Соединение установлено.");
                    return true;
                }
                else
                {
                    Console.WriteLine("Не удалось установить соединение, проверьте введённые данные.");
                    return false;
                }
            }
            catch (Exception)
            {
                Console.Write("При попытке установить соединение что-то пошло не так. Проверьте введённые данные.");
                return false;
            }
        }

        private static bool PortEnabled(string ip, int port)
        {
            try
            {
                Console.WriteLine("Обработка...");
                IPAddress testIP = IPAddress.Parse(ip);
                Ping pingSender = new Ping();
                PingReply reply = pingSender.Send(testIP);

                using (TcpClient Scan = new TcpClient())
                {
                    try
                    {
                        Scan.Connect(reply.Address.ToString(), port);
                        Console.WriteLine($"[{port}] | PORT IS OPEN");
                        return true;
                    }
                    catch
                    {
                        Console.WriteLine($"[{port}] | PORT IS CLOSED\nProgram is over.");
                        return false;
                    }
                }
            }
            catch (Exception)
            {
                Console.Write("Возникла ошибка при подкючении к порту. Проверьте введённые данные.");
                return false;
            }
        }
    }
}
