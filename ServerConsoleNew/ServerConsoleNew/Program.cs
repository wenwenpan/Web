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
            Console.WriteLine("Server is running ... ");
            IPAddress ip = new IPAddress(new byte[] { 0, 0, 0, 0 });
            TcpListener listener = new TcpListener(ip, 8500);
            listener.Start();
            Console.WriteLine("Start Listening ...");
            while (true) {
                TcpClient client = listener.AcceptTcpClient();
                RemoteClient wapper = new RemoteClient(client);
            }
        }
    }
    public class RemoteClient
    {
        private TcpClient client;
        private NetworkStream streamToClient;
        private const int BufferSize = 8192;
        private byte[] buffer;
        private RequestHandler handler;

        public RemoteClient(TcpClient client)
        {
            this.client = client;
            Console.WriteLine("\nClient Connected! {0}-->{1}", client.Client.LocalEndPoint, client.Client.RemoteEndPoint);
            streamToClient = client.GetStream();
            buffer = new byte[BufferSize];

            handler = new RequestHandler();
            AsyncCallback callBack = new AsyncCallback(ReadComplete);
            streamToClient.BeginRead(buffer, 0, BufferSize, callBack, null);
        }
        /*huidao*/
        private void ReadComplete(IAsyncResult ar)
        {
            int bytesRead = 0;
            try
            {
                lock (streamToClient)
                {
                    bytesRead = streamToClient.EndRead(ar);
                    Console.WriteLine("Reading data, {0} bytes ...", bytesRead);
                }
                if (bytesRead == 0) throw new Exception("读取到0字节");
                string msg = Encoding.Unicode.GetString(buffer, 0, bytesRead);
                Array.Clear(buffer, 0, buffer.Length);
                string[] msgArray = handler.GetActualString(msg);
                /*读取字节*/
                foreach (string m in msgArray)
                {
                    Console.WriteLine("Received:{0}", m);
                    string back = m.ToUpper();
                    byte[] temp = Encoding.Unicode.GetBytes(back);
                    streamToClient.Write(temp, 0, temp.Length);
                    streamToClient.Flush();
                    Console.WriteLine("Sent: {0}", back);
                }
                lock (streamToClient)
                {
                    AsyncCallback callBack = new AsyncCallback(ReadComplete);
                    streamToClient.BeginRead(buffer, 0, BufferSize, callBack, null);
                }
            }
            catch (Exception ex) {
                if (streamToClient != null)
                    streamToClient.Dispose();
                client.Close();
                Console.WriteLine(ex.Message);
            }
        }
    }

    public class RequestHandler
    {
        private string temp = string.Empty;
        public string[] GetActualString(string input)
        {
            return GetActualString(input, null);
        }
        private string[] GetActualString(string input, List<string> outputList)
        {
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
            else
            {
                temp = input;
            }
            return outputList.ToArray();
        }
        public static void Test()
        {
            RequestHandler handler = new RequestHandler();
            string input;

            input = "[length=13]明天中秋，祝大家节日快乐！";
            handler.PrintOutput(input);

            input = "明天中秋，祝大家节日快乐！";
            input = String.Format("[length=13]{0}", input);
            handler.PrintOutput(input);
        }
        private void PrintOutput(string input)
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
