using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ruler.Wpf.Models
{
    public class UpdateVersion
    {
        private Version _version=new Version(0,0,0,0);
        private bool _userWantsUpdate;
        private bool _updated;
        public Version Version
        {
            get => _version;
            set
            {
                if (_version != value)
                {
                    _version = value;
                }
            }
        }
        public bool UserWantsUpdate
        {
            get => _userWantsUpdate;
            set
            {
                if (_userWantsUpdate != value)
                {
                    _userWantsUpdate = value;
                }
            }
        }
        public bool Updated
        {
            get => _updated;
            set
            {
                if (_updated != value)
                {
                    _updated = value;
                }
            }
        }
    }
}
