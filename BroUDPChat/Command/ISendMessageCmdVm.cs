using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BroUDPChat.Command
{
    public interface ISendMessageCmdVm
    {
        public string UserName;
        public void SendMessage();
    }
}
