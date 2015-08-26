using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace WpfMasterPassword
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            //From all threads in the AppDomain.
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            //  From a single specific UI dispatcher thread.
            Dispatcher.UnhandledException += Dispatcher_UnhandledException;
            //Application.DispatcherUnhandledException From the main UI dispatcher thread in your WPF application.
//TaskScheduler.UnobservedTaskException from within each AppDomain that uses a task scheduler for asynchronous operations.
        }

        private void Dispatcher_UnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            MessageBox.Show(e.Exception.Message);
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (!e.IsTerminating)
            {
                MessageBox.Show(e.ExceptionObject.ToString());
            }
        }
    }
}
