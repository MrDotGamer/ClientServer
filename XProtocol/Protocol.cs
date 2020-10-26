using System.IO;
using System.Linq;
using System.Text;

namespace XProtocol
{
    public class Protocol
    {
        private byte Pranesymas { get; set;}
        private string Data { get; set; }
       
        public Protocol(byte _pranesymas, string data)
        {
            Pranesymas = _pranesymas;
            Data = data;
        }

        public byte[] ToPacket()
        {
            var pranesymas = Encoding.UTF8.GetBytes(Pranesymas.ToString());
            var stream = new MemoryStream();
            var data = Encoding.UTF8.GetBytes(Data);
            stream.Write(new byte[] { 0xAF, 0xAA, 0xAF, pranesymas[0] }, 0, 4);
            stream.Write(data, 0, data.Length);
            stream.Write(new byte[] { 0xFF, 0x00 }, 0, 2);
            return stream.ToArray();
        }
        public static Protocol Parse(byte[] packet)
        {
            if (packet.Length < 7)
            {                
                return null;
            }
            if (packet[0] != 0xAF || packet[1] != 0xAA || packet[2] != 0xAF)
            {
                return null;
            }
            byte[] data = TrunkPacketSignature(packet);
            var pranesymas = packet[3];
            return  new Protocol(pranesymas, Encoding.UTF8.GetString(data, 0, data.Length - 2));
        }
        public string GetData()
        {
            return Data;
        }
        public byte GetPranesymas()
        {
            return Pranesymas;
        }
        
        static byte[] TrunkPacketSignature(byte[] packet)
        {
            int packageLength = 0;
                do
                {
                    packageLength++;
                } while (packet[packageLength-1] != 0xFF && packet[packageLength] != 0x00);
            
            byte[] dataPacket = new byte[packageLength+1];
            for (int i = 0; i < dataPacket.Length; i++)
            {
                dataPacket[i] = packet[i];
            }
            return dataPacket.Skip(4).ToArray();
        }
    }
}
