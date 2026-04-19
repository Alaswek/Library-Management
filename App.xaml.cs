using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.IO;

namespace LibraryManagement
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            string exeFolder = AppDomain.CurrentDomain.BaseDirectory;
            string projectFolder = Path.GetFullPath(Path.Combine(exeFolder, @"..\.."));

            AppDomain.CurrentDomain.SetData("DataDirectory", projectFolder);
        }
    }
}
