using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace demo.Models
{
    public class TokenParameter
    {
        public string Secret { get; set; }
        public string Issuer { get; set; }
        public int AccessExpiration { get; set; }
        public int RefreshExpiration { get; set; }
        public string ClientId { get; set; }
        public string ClientScrect { get; set; }
    }
}
