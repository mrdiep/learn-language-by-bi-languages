using System;
using System.Windows.Input;

namespace VoiceSubtitle.Model
{
    public class ActionCommand : ICommand
    {
        private Action<object> action;
        Func<object, bool> canExe;

        private Action action2;
        Func< bool> canExe2;
        public ActionCommand(Action<object> action,Func<object,bool> canExe=null)
        {
            this.action = action;
            this.canExe = canExe;
        }
        public ActionCommand(Action action, Func<bool> canExe = null)
        {
            this.action2 = action;
            this.canExe2 = canExe;
        }
        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            if (canExe == null)
                return true;

            bool? value =  canExe?.Invoke(parameter);
            if (!value.HasValue)
                return canExe2.Invoke();

            return value.Value;
        }

        public void Execute(object parameter)
        {
            action?.Invoke(parameter);
            action2?.Invoke();
        }
    }
}