using System;
using System.Windows.Input;

namespace VoiceSubtitle.Model
{
    public class ActionCommand : ICommand
    {
        private Action<object> action;
        Func<object, bool> canExe;
        public ActionCommand(Action<object> action,Func<object,bool> canExe=null)
        {
            this.action = action;
            this.canExe = canExe;
        }

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            if (canExe == null)
                return true;

            return canExe(parameter);
        }

        public void Execute(object parameter)
        {
            action(parameter);
        }
    }
}