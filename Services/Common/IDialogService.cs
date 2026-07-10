using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibraryManagement.Services.Common
{
    public interface IDialogService
    {
        void ShowInfo(string message, string title = "Info");
        void ShowWarning(string message, string title = "Warning");
        void ShowError(string message, string title = "Error");
        bool Confirm(string message, string title = "Confirm");
    }
}
