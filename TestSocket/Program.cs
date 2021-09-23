using System;
using System.Net;
using System.Text;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Linq;


namespace TestSocket
{
    class Program
    {           
        static void Main(string[] args)
        {
            byte[] rawData = StaticData.CreateTestPackage_246();

            var from = new IPEndPoint(0, 0);

            foreach(ushort port in StaticData.ports)
            {
                /*Task.Run(() =>
                {
                    try
                    {
                        UdpClient udpClient = StaticData.CreateUdpClient(port);
                        while (true)
                        {
                            byte[] recvBuffer = udpClient.Receive(ref from);
                            byte TypePackage = StaticData.TaskProcessEasy(recvBuffer);
                            Console.WriteLine($"TypePackage:{TypePackage}  from:{udpClient.Client.LocalEndPoint} длинна пакета: {recvBuffer.Length - 12}");
                        }
                    }
                    catch (Exception  ex) {
                        Console.WriteLine(ex.Message);
                    }
                });*/

                Task.Run(() =>
                {
                    UdpClient udpClient = StaticData.CreateUdpClient(port);
                    while (true)
                    {
                        byte[] recvBuffer = udpClient.Receive(ref from);
                        StaticData.TaskProcess_241_243(recvBuffer);
                    }
                });

                /*Task.Run(() =>
                {
                    UdpClient udpClient = StaticData.CreateUdpClient(port);

                    while (true)
                    {
                        byte[] recvBuffer = udpClient.Receive(ref from);
                        StaticData.TaskProcess_246(recvBuffer);

                    }
                });*/
            }

            Console.WriteLine("Starting test socket capture...\n");

            /*UdpClient udpClient = new UdpClient();
            Thread.Sleep(1000);
            udpClient.Send(rawData, rawData.Length, "255.255.255.255", 27681);*/

            Console.ReadLine();
        }
    }

}


//public static Encoding encoding = Encoding.GetEncoding("windows-1251");
//Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
