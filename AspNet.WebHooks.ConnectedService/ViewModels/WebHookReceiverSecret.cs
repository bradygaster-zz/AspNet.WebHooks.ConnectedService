using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AspNet.WebHooks.ConnectedService.ViewModels
{
    public class WebHookReceiverSecret : INotifyPropertyChanged
    {
        public WebHookReceiverOption Option { get; private set; }

        public WebHookReceiverSecret(WebHookReceiverOption option)
        {
            Option = option;
        }

        private string _secret;

        public string Secret
        {
            get { return _secret; }
            set
            {
                _secret = value;
                OnPropertyChanged("Secret");
            }
        }

        public string ReceiverSecretConfigSettingName
        {
            get
            {
                var receiverName = string.IsNullOrEmpty(Option.ReceiverSecretConfigSettingNameOverride)
                    ? (string.IsNullOrEmpty(Option.ConfigWireupOverride) ? Option.Name : Option.ConfigWireupOverride)
                    : Option.ReceiverSecretConfigSettingNameOverride;

                return string.Concat(Constants.ReceiverSecretConfigPrefix, receiverName);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
