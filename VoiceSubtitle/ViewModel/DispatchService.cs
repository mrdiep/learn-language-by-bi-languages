using System;
using System.Windows;
using System.Windows.Threading;

namespace VoiceSubtitle.ViewModel
{
    public class DispatchService
    {
        public void Invoke(Action action)
        {
            var dispatchObject = Application.Current.Dispatcher;
            if (dispatchObject == null || dispatchObject.CheckAccess())
            {
                action();
            }
            else
            {
                dispatchObject.Invoke(action);
            }
        }
    }
}