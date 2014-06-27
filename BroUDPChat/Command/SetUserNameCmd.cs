using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace BroUDPChat.Command
{
    public class SetUserNameCmd : ICommand
    {
        private ISetUserNameCmdVm vm;

        public SetUserNameCmd(ISetUserNameCmdVm vm)
        {
            this.vm = vm;
        }

        public bool CanExecute(object parameter)
        {
            return string.IsNullOrEmpty(vm.UserName);
        }

        public event EventHandler CanExecuteChanged;

        public void Execute(object parameter)
        {
            vm.SetUserName();
        }
    }
}
