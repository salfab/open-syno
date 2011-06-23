using System.ComponentModel;

namespace OpenSyno.ViewModels
{
    using System.Runtime.Serialization;

    [DataContract]
    public class ViewModelBase : INotifyPropertyChanged
    {
        virtual public void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                var eventArgs = new PropertyChangedEventArgs(propertyName);
                PropertyChanged(this, eventArgs);
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;
    }
}