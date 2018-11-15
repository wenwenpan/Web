using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Net;
using System.Net.Sockets;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientConsoleNew
{
    class Program
    {
        static void Main(string[] args)
        {
            ConsoleKey key;
            ServerClient client = new ServerClient();
            client.SendMessage();

            Console.WriteLine("\n\n 输入\"Q\"键退出。");
            do
            {
                key = Console.ReadKey(true).Key;
            } while (key != ConsoleKey.Q);
        }
    }
    public class ServerClient
    {
        private const int BufferSize = 8192;
        private byte[] buffer;
        private TcpClient client;
        private NetworkStream streamToServer;
        private string msg = "Welcome to TraceFact.Net!";
        private RequestHandler handler;

        public ServerClient()
        {
            try
            {
                client = new TcpClient();
                client.Connect("localhost", 8500); // 与服务器连接
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return;
            }
            buffer = new byte[BufferSize];
            Console.WriteLine("Server Connected！{0} --> {1}", client.Client.LocalEndPoint, client.Client.RemoteEndPoint);
            streamToServer = client.GetStream();
        }
        public void SendMessage(string msg) {
            msg = String.Format("[length={0}]{1}", msg.Length, msg);
            for (int i = 0; i <= 2; i++)
            {
                byte[] temp = Encoding.Unicode.GetBytes(msg); // 获得缓存
                try
                {
                    streamToServer.Write(temp, 0, temp.Length); // 发往服务器
                    Console.WriteLine("Sent: {0}", msg);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    break;
                }
            }
            lock (streamToServer) {
                AsyncCallback callBack = new AsyncCallback(ReadComplete);
                streamToServer.BeginRead(buffer, 0, BufferSize, callBack, null);
            }

        }
        public void SendMessage()
        {
            SendMessage(this.msg);
        }
        private void ReadComplete(IAsyncResult ar) {
            int bytesRead=0;
            handler = new RequestHandler();
            try
            {
                lock (streamToServer)
                {
                    bytesRead = streamToServer.EndRead(ar);
                }
                if (bytesRead == 0) throw new Exception("读取到0 字节");
                string msg = Encoding.Unicode.GetString(buffer, 0, bytesRead);
                Console.WriteLine("Received: {0}", msg);
                Array.Clear(buffer, 0, buffer.Length); // 清空缓存，避免脏读
                //string[] msgArray = handler.GetActualString(msg);
                //foreach (string m in msgArray)
                //{
                //    Console.WriteLine("Received:{0}", m);
                //}
                handler.PrintOutput(msg);
                lock (streamToServer)
                {
                    AsyncCallback callBack = new AsyncCallback(ReadComplete);
                    streamToServer.BeginRead(buffer, 0, BufferSize, callBack, null);
                }
            }
            catch (Exception ex) {
                if (streamToServer != null)
                    streamToServer.Dispose();
                client.Close();
                Console.WriteLine(ex.Message);
            }
        }
    }

    public class RequestHandler {
        private string temp = string.Empty;
        public string[] GetActualString(string input) {
            return GetActualString(input,null);
        }
        private string[] GetActualString(string input,List<string>outputList) {
            if (outputList == null)
                outputList = new List<string>();

            if (!String.IsNullOrEmpty(temp))
                input = temp + input;

            string output = "";
            string pattern = @"(?<=^\[length=)(\d+)(?=\])";//\b(?<word>\w+)\s+(\k<word>)\b
            int length;

            if (Regex.IsMatch(input, pattern))
            {
                Match m = Regex.Match(input, pattern);
                //获取需要截取的位置
                length = Convert.ToInt32(m.Groups[0].Value);
                //获取从此位置开始的字符
                int startIndex = input.IndexOf(']') + 1;
                output = input.Substring(startIndex);

                if (output.Length == length)
                {
                    outputList.Add(output);
                    temp = "";
                }
                else if (output.Length < length)
                {
                    temp = input;
                }
                else if (output.Length > length)
                {
                    output = output.Substring(0, length);
                    outputList.Add(output);
                    temp = "";
                    input = input.Substring(startIndex + length);
                    GetActualString(input, outputList);
                }
            }
            else {
                temp = input;
            }
            return outputList.ToArray();
        }
        public static void Test() {
            RequestHandler handler = new RequestHandler();
            string input;

            input = "[length=13]明天中秋，祝大家节日快乐！";
            handler.PrintOutput(input);

            input = "明天中秋，祝大家节日快乐！";
            input = String.Format("[length=13]{0}",input);
            handler.PrintOutput(input);
        }
        public void PrintOutput(string input)
        {
            Console.WriteLine(input);
            string[] outputArray = GetActualString(input);
            foreach (string output in outputArray)
            {
                Console.WriteLine(output);
            }
            Console.WriteLine();
        }
    }
}
