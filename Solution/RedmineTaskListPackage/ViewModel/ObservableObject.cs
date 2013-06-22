using System;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Reflection;

namespace RedmineTaskListPackage.ViewModel
{
    public abstract class ObservableObject : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;


        protected virtual void OnPropertyChanged(string propertyName)
        {
            var d = PropertyChanged;

            if (d != null)
            {
                d(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
