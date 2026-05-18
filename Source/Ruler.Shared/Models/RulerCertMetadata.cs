using Ruler.Shared.Enums;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ruler.Shared.Models
{
    [Serializable]
    public class RulerCertMetadata
    {
        // The unique identifier for the physical certificate
        public string Thumbprint { get; set; }

        // Human-readable context (e.g., "Workstation-Office-01")
        public DevicesTypes DeviceType { get; set; }

        // Status Flags
        public bool IsRevoked { get; set; }  // Manual kill-switch

        // Audit Data (Nullable is perfectly valid here in 4.8)
        public DateTime? LastUsed { get; set; }
        public DateTime ExpirationDate { get; set; }

        // Computed Properties (Helpers for your UI)
        public bool IsExpired => DateTime.Now > ExpirationDate;

        // A cert is only "Functional" if it's not expired, not revoked, 
        // and physically exists with a private key (checked at runtime).
        public bool IsHealthy { get; set; }
    }
}
