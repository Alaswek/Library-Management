using LibraryManagement.MVVM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibraryManagement.Services.Admin.LibraryBooks.Results
{
    public class ServiceResult : ViewModelBase
    {
        private bool _success;
        public bool Success
        {
            get { return _success; }
            set { _success = value; OnPropertyChanged(); }
        }

        private string _message;
        public string Message
        {
            get { return _message; }
            set { _message = value; OnPropertyChanged(); }
        }

        // doar eu si copiii mei am putut folosi constructorul
        protected ServiceResult(bool success, string message)
        {
            this.Success = success;
            this.Message = message;
        }

        public static ServiceResult Ok(string message)
        {
            return new ServiceResult(true, message);
        }

        public static ServiceResult Fail(string message)
        {
            return new ServiceResult(false, message);
        }

    }

    public class ServiceResult<T> : ServiceResult
    {
        private bool _success;
        public bool Success
        {
            get { return _success; }
            set { _success = value; OnPropertyChanged(); }
        }

        private string _message;
        public string Message
        {
            get { return _message; }
            set { _message = value; OnPropertyChanged(); }
        }

        private T _data;
        public T Data
        {
            get { return _data; }
            set { _data = value; OnPropertyChanged();  }
        }

        public ServiceResult(bool success, string message, T data) : base(success, message)
        {
                Success = success;
                Message = message;
                Data = data;
        }

        public static ServiceResult<T> Ok(T data, string message)
        {
            return new ServiceResult<T>(true, message, data);
        }

        public static ServiceResult<T> Fail(string message)
        {
            return new ServiceResult<T>(false, message, default(T));
        }

    }
}
