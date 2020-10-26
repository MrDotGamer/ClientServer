using Newtonsoft.Json;
using System;
using System.Net.Sockets;
using System.Text;
using XProtocol;


namespace Client
{
    class Program
    {
        static void Main()
        {
            try
            {
                while (true)
                {
                    TcpClient client = new TcpClient("127.0.0.1",8000);
                    Console.WriteLine("Client connected");
                    NetworkStream stream = client.GetStream();
                    Console.WriteLine("Ieskite pranesyma:Check - 0;UseCode - 1; ActionGenerate - 2;Stop Server - 3");
                    var pranesymas = Console.ReadKey();
                    Console.WriteLine();
                    StringBuilder requestdata = new StringBuilder();
                    switch (pranesymas.KeyChar)
                    {
                        case (char)48:
                            Console.WriteLine("Iveskite Korteles numeri: ");
                            requestdata.Append(Console.ReadLine());
                            break;
                        case (char)49:
                            Console.WriteLine("Iveskite Akcijos Koda");
                            var akcijosKodas = Console.ReadLine();
                            Console.WriteLine("Iveskite korteles numeri");
                            var cardNumber = Console.ReadLine();
                            UsedCode useCode = new UsedCode
                            {
                                ActionCode = akcijosKodas,
                                CardNumber = cardNumber
                            };
                            requestdata.Append(JsonConvert.SerializeObject(useCode));
                            break;
                        case (char)50:
                            requestdata.Append("GenerateAction");
                            break;
                        case (char)51:
                            requestdata.Append("ServerStop");
                            break;
                    }
                    var message = new Protocol(byte.Parse(pranesymas.KeyChar.ToString()), requestdata.ToString());
                    byte[] bytes = message.ToPacket();
                    stream.Write(bytes, 0, bytes.Length);
                    stream.Flush();

                    byte[] byteRead = new byte[256];
                    int length = stream.Read(byteRead, 0, byteRead.Length);
                    Protocol receivedProtocol = Protocol.Parse(byteRead);
                    
                    var data = receivedProtocol.GetData();
                    if(data == "Error" || data == "Kodai Irasyti"|| data == "ServerStop")
                    {
                        Console.WriteLine("Pranesymas: " + data);
                    }
                    else 
                    {
                        var result = JsonConvert.DeserializeObject<string>(data);
                        Console.WriteLine(int.Parse(result));
                    }
                }
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }
}
