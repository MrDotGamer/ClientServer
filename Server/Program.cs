using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using XProtocol;

namespace Server
{
    class Program
    {
        const int port = 8000;
        const int maxActionCodeLength = 8;
        const string AkctionPath = @"C:\Users\vgds7\source\repos\ClientServerSocket\Akcijos.txt";
        const string KoduSimboliai = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";
        const string AkcijosKoduSimboliai = "1234567890";
        const string CardNumberPath = @"C:\Users\vgds7\source\repos\ClientServerSocket\CardNumber.txt";
        const string UsedActionCodes = @"C:\Users\vgds7\source\repos\ClientServerSocket\PanaudotiAkcijosKodai.txt";
        static void Main()
        {
            IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, port);
            TcpListener listener = new TcpListener(endPoint);
            listener.Start();
            Console.WriteLine("Server started");
            while (true)
            {
                try
                {
                    TcpClient client = listener.AcceptTcpClient();
                    NetworkStream stream = client.GetStream();
                    byte[] bytesRequest = new byte[256];
                    stream.Read(bytesRequest, 0, bytesRequest.Length);
                    Protocol receivedProtocol = Protocol.Parse(bytesRequest);
                    switch (receivedProtocol.GetPranesymas())
                    {
                        case 48:
                            var responseonCheck = ResponseOnCheckStatement(receivedProtocol.GetData());
                            Protocol protocolResponseCheck = new Protocol(receivedProtocol.GetPranesymas(), responseonCheck);
                            byte[] onCheck = protocolResponseCheck.ToPacket();
                            stream.Write(onCheck, 0, onCheck.Length);
                            stream.Flush();
                            break;
                        case 49:
                            UsedCode usingCode = JsonConvert.DeserializeObject<UsedCode>(receivedProtocol.GetData());
                            var responseUseCode = GetActionOnUseCodeStatement(usingCode.ActionCode, usingCode.CardNumber);
                                Protocol protocolResponseUseCode = new Protocol(receivedProtocol.GetPranesymas(), responseUseCode);
                                byte[] onUseCode = protocolResponseUseCode.ToPacket();
                                stream.Write(onUseCode, 0, onUseCode.Length);
                            if(responseUseCode != "Error")
                            {
                                SaveUsedCode(usingCode);
                                RemoveAction(usingCode);
                            }
                            stream.Flush();
                            break;
                        case 50:
                            GenerateCardNumbers(1000);
                            var result = WriteAkcijosKodus(2000);
                            Protocol protocolResponseActionGenerate = new Protocol(receivedProtocol.GetPranesymas(), result);
                            byte[] onActionGenerate = protocolResponseActionGenerate.ToPacket();
                            stream.Write(onActionGenerate, 0, onActionGenerate.Length);
                            stream.Flush();
                            break;
                        case 51:
                            ServerStop();
                            listener.Stop();
                            break;
                    }
                    client.Close();
                }
                catch(Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }
        static void SaveUsedCode(UsedCode usedCode)
        {
            try
            {
                List<UsedCode> _usedCodesList = new List<UsedCode>();
                var usedCodesList = JsonConvert.DeserializeObject<List<UsedCode>>((File.ReadAllText(UsedActionCodes)));
                if (usedCodesList == null)
                {
                    _usedCodesList.Add(usedCode);
                }
                else
                {
                    _usedCodesList = usedCodesList;
                    _usedCodesList.Add(usedCode);
                }
                using (StreamWriter sw = new StreamWriter(UsedActionCodes, false, System.Text.Encoding.Default))
                {
                    var json = JsonConvert.SerializeObject(_usedCodesList);
                    sw.Write(json);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
        static string WriteAkcijosKodus(int quantity)
        {
            try
            {
                using StreamWriter sw = new StreamWriter(AkctionPath, false, System.Text.Encoding.Default);
                List<Akcija> akcijos = new List<Akcija>();
                for (int i = 0; i < quantity; i++)
                {
                    Random rnd = new Random();
                    akcijos.Add(CreateAkcija(rnd.Next(1, maxActionCodeLength + 1)));
                }
                var json = JsonConvert.SerializeObject(akcijos.ToArray());
                sw.Write(json);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            return "Kodai Irasyti";
        }
        static Akcija CreateAkcija(int maxCodeLength)
        {
            StringBuilder res = new StringBuilder();
            using (RNGCryptoServiceProvider rnd = new RNGCryptoServiceProvider())
            {
                while (res.Length != maxCodeLength)
                {
                    byte[] oneByte = new byte[1];
                    rnd.GetBytes(oneByte);
                    char character = (char)oneByte[0];
                    if (AkcijosKoduSimboliai.Contains(character))
                    {
                        res.Append(character);
                    }
                }
            }
            List<string> products = new List<string>();
            for (int i = 0; i < 5; i++)
            {
                products.Add(CreateProduct(10));
            }
            var cardNumberList = JsonConvert.DeserializeObject<List<string>>(File.ReadAllText(CardNumberPath));
            Random randomCardNumber = new Random();
            var item = randomCardNumber.Next(0, cardNumberList.Count);
            return new Akcija
            {
                ActionCode = res.ToString(),
                Products = products,
                CardNumber = cardNumberList[item]
            };
        }
        static string ResponseOnCheckStatement(string cardNumber)
        {
            var akcijosKodaiJson = JsonConvert.DeserializeObject<List<Akcija>>(File.ReadAllText(AkctionPath));
            
            if (akcijosKodaiJson == null)
            {
                return "Error";
            }
            var akcija = akcijosKodaiJson.FirstOrDefault(x => x.CardNumber == cardNumber);
            if (akcija == null)
            {
                return "Error";
            } 
            var json = JsonConvert.SerializeObject(akcija.ActionCode);
            return json;
        }
        static string GetActionOnUseCodeStatement(string actionCode, string cardNumber)
        {
            if (actionCode == "" || cardNumber == "")
            {
                return "Error";
            }
            else
            {
                var akcijosKodaiJson = JsonConvert.DeserializeObject<List<Akcija>>(File.ReadAllText(AkctionPath));
                var akcija = akcijosKodaiJson.Where(x => x.CardNumber == cardNumber).FirstOrDefault(x => x.ActionCode == actionCode);
                if (akcija == null)
                {
                    return "Akcijos nera";
                }
                return akcija.ActionCode;
            }
        }
        static string CreateProduct(int length)
        {
            Random productName = new Random();
            return new string(Enumerable.Repeat(KoduSimboliai, length)
             .Select(s => s[productName.Next(s.Length)]).ToArray());
        }
        static void GenerateCardNumbers(int quantity)
        {
            try
            {
                var cardNumberList = JsonConvert.DeserializeObject<List<string>>(File.ReadAllText(CardNumberPath));
                using StreamWriter sw = new StreamWriter(CardNumberPath, false, System.Text.Encoding.Default);
                List<string> _cardNumber = new List<string>();
                if (cardNumberList == null)
                {
                    for (int i = 0; i < quantity; i++)
                    {
                        _cardNumber.Add(GenerateCardNumber(8));
                    }
                }
                else
                {
                    _cardNumber = cardNumberList;
                    for (int i = 0; i < quantity; i++)
                    {
                        _cardNumber.Add(GenerateCardNumber(8));
                    }
                }
                var json = JsonConvert.SerializeObject(_cardNumber.ToArray());
                sw.Write(json);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
        static string GenerateCardNumber(int length)
        {
            Random productName = new Random();
            return new string(Enumerable.Repeat(KoduSimboliai, length)
             .Select(s => s[productName.Next(s.Length)]).ToArray());
        }
        static void ServerStop()
        {
            Console.WriteLine("Server stopped");
            Console.ReadKey();
        }
        private static void RemoveAction(UsedCode usingCode)
        {
            try
            {
                var akcijosKodaiJson = JsonConvert.DeserializeObject<List<Akcija>>(File.ReadAllText(AkctionPath));
                using StreamWriter sw = new StreamWriter(AkctionPath, false, System.Text.Encoding.Default);
                
                var akcija = akcijosKodaiJson.Where(x => x.ActionCode == usingCode.ActionCode).FirstOrDefault(c => c.CardNumber == usingCode.CardNumber);
                if (akcija != null)
                {
                    akcijosKodaiJson.Remove(akcija);
                    var json = JsonConvert.SerializeObject(akcijosKodaiJson.ToArray());
                    sw.Write(json);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }
}
