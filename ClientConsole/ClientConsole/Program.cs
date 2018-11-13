using System;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Client Running");
            TcpClient client;
            const int BufferSize = 8192;
                try
                {
                    client = new TcpClient();
                    client.Connect("localhost", 8500);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    return;
                }
                Console.WriteLine("Server Connected! {0}-->{1}", client.Client.LocalEndPoint, client.Client.RemoteEndPoint);

               // string msg = "\"Welcome\"";
            NetworkStream streamToServer = client.GetStream();
            ConsoleKey key1;
            Console.WriteLine("Menu:S-Sent X-Exit");
            do {
                key1 = Console.ReadKey(true).Key;
                if (key1 == ConsoleKey.S)
                {
                    Console.WriteLine("Input msg:");
                    string msg = Console.ReadLine();
                    byte[] buffer = Encoding.Unicode.GetBytes(msg);
                    try
                    {
                        lock (streamToServer)
                        {
                            streamToServer.Write(buffer, 0, buffer.Length);
                        }

                        Console.WriteLine("Send:{0}", msg);
                        int bytesRead;
                        buffer = new byte[BufferSize];
                        lock (streamToServer)
                        {
                            bytesRead = streamToServer.Read(buffer, 0, buffer.Length);
                        }
                        msg = Encoding.Unicode.GetString(buffer, 0, bytesRead);
                        Console.WriteLine("Received:{0}", msg);
                    }
                    catch (Exception ex){
                        Console.WriteLine(ex.Message);
                        break;
                    }
                }
            } while (key1 != ConsoleKey.X);
            ConsoleKey key;
            Console.WriteLine("\ninput \"Q\" to quit");
            do
            {
                key = Console.ReadKey(true).Key;
            } while (key != ConsoleKey.Q);
        }
    }
}
