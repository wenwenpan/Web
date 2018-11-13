using System;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServeConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Server is running");
            const int BufferSize = 8192;
           try
           {
                // IPAddress ip = Dns.GetHostEntry(Dns.GetHostName()).AddressList[0];
                // IPAddress ip = new IPAddress(new byte[] { 127,0,0,1 });
                
                    IPAddress ip = IPAddress.Parse("0.0.0.0");//127.0.0.1失败
                    TcpListener listener = new TcpListener(ip, 8500);
                    listener.Start();
                    Console.WriteLine("Start Listering");
                
                    TcpClient remoteClient = listener.AcceptTcpClient();
                    Console.WriteLine("Client Connented! {0}<--{1}", remoteClient.Client.LocalEndPoint, remoteClient.Client.RemoteEndPoint);
                    NetworkStream streamToClient = remoteClient.GetStream();//huoqu liu 
                do { 
                    byte[] buffer = new byte[BufferSize];
                    int bytesRead;
                    try
                    {
                        lock (streamToClient)
                        {
                            bytesRead = streamToClient.Read(buffer, 0, BufferSize);//duqu shuju 返回数据的length
                        }
                        if (bytesRead == 0) throw new Exception("读取到0字节");
                        Console.WriteLine("Reading data,{0}bytes....", bytesRead);
                        string msg = Encoding.Unicode.GetString(buffer, 0, bytesRead);
                        Console.WriteLine("Receive:{0}", msg);
                        msg = msg.ToUpper();
                        buffer = Encoding.Unicode.GetBytes(msg);
                        lock (streamToClient)
                        {
                            streamToClient.Write(buffer, 0, buffer.Length);
                        }
                        Console.WriteLine("Sent:{0}", msg);
                    }
                    catch (Exception ex) {
                        Console.WriteLine(ex.Message);
                        break;
                    }
                } while (true);
            }
            catch (Exception ex){
                Console.WriteLine(ex.Message);
                return;
            }

            Console.WriteLine("\ninput \"Q\" to quit");
            ConsoleKey key;
            do {
                key = Console.ReadKey(true).Key;
            } while (key!=ConsoleKey.Q);
        }
    }
}


// 大文件 获取字符串
//byte[] buffer = new byte[BufferSize];
//int bytesRead; // 读取的字节数
//MemoryStream msStream = new MemoryStream();
//do {
//bytesRead = streamToClient.Read(buffer, 0, BufferSize);
//msStream.Write(buffer, 0, bytesRead);
//} while (bytesRead > 0);
//buffer = msStream.GetBuffer();
//string msg = Encoding.Unicode.GetString(buffer);
