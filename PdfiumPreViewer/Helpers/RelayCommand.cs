using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace PdfiumPreViewer.Helpers
{
    public class RelayCommand : ICommandBase
    {
        private Action commandTask;

        public RelayCommand(Action action)
        {
            commandTask = action;
        }
        public override void Execute(object parameter)
        {
            commandTask();
        }
    }
}
