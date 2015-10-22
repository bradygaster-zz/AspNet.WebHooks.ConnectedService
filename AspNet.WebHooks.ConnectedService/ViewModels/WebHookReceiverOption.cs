using System.ComponentModel;

namespace AspNet.WebHooks.ConnectedService.ViewModels
{
    public class WebHookReceiverOption : INotifyPropertyChanged
    {
        public string Name { get; set; }
        public string NuGetPackage { get; set; }
        public string NuGetVersionOverride { get; set; }
        public string ConfigWireupOverride { get; set; }
        public string ReceiverNameOverride { get; set; }
        public string IncomingTypeOverride { get; set; }
        public string ReceiverSecretConfigSettingNameOverride { get; set; }

        private bool _isChecked;
        public bool IsChecked
        {
            get { return _isChecked; }
            set
            {
                _isChecked = value;
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs("IsChecked"));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
