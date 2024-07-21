using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace FullText.Helpers
{
    public abstract class ICommandBase : ICommand
    {
        public event EventHandler CanExecuteChanged;

        public virtual bool CanExecute(object parameter)
        {
            return true;
        }

        public abstract void Execute(object parameter);

        protected void OnCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, new EventArgs());
        }
    }

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