using System.Collections.Generic;

namespace XProtocol
{
    public class Akcija
    {
        public string ActionCode { get; set; }
        public string CardNumber { get; set; }
        public List<string> Products = new List<string>();
    }
}
