using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ruler.Shared.Models
{
    public class SignedSettings
    {
        public string Data { get; set; }      // The JSON list of rulers
        public string Signature { get; set; } // The RSA signature of that JSON

    }
}


