using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace BroUDPChat.Command
{
    public class SetLEDCommand : ICommand
    {
        private ISetLEDVM _vm;
        private int _ledNum;
        public SetLEDCommand(ISetLEDVM vm, int ledNum)
        {
            _vm = vm;
            _ledNum = ledNum;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public event EventHandler CanExecuteChanged;

        public void Execute(object parameter)
        {
            bool enable = (bool)parameter;
            if (enable)
            {
                _vm.EnableLED(_ledNum);
            }
            else
            {
                _vm.DisableLED(_ledNum);
            }
        }
    }
}
