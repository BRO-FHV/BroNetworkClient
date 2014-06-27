using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BroUDPChat.Command
{
    public interface ISetLEDVM
    {
        public void EnableLED(int num);
        public void DisableLED(int num);
    }
}
