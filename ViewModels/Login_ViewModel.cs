using GenericUi.Commands;
using LibraryManagement.MVVM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace LibraryManagement.ViewModels
{
    public class Login_ViewModel : ViewModelBase
    {


        #region iCommands
        public ICommand Login_command { get; }
        #endregion

        public Login_ViewModel()
        {
            // constructor 
            Login_command = new RelayCommand(_ => DoLogin());
        }


        public void DoLogin()
        {
            throw new NotImplementedException();
        }

    }
}
