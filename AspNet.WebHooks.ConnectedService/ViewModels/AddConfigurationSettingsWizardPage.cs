using AspNet.WebHooks.ConnectedService.Views;
using Microsoft.VisualStudio.ConnectedServices;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using AspNet.WebHooks.ConnectedService.Utility;

namespace AspNet.WebHooks.ConnectedService.ViewModels
{
    public class AddConfigurationSettingsWizardPage : ConnectedServiceWizardPage
    {
        internal ConnectedServiceProviderContext Context { get; set; }
        internal ConnectedServiceInstance Instance { get; set; }

        private ObservableCollection<WebHookReceiverSecret> _receiverSecrets;

        public ObservableCollection<WebHookReceiverSecret> ReceiverSecrets
        {
            get { return _receiverSecrets; }
            set
            {
                _receiverSecrets = value;
                OnPropertyChanged();
            }
        }

        public AddConfigurationSettingsWizardPage(ConnectedServiceProviderContext context,
            ConnectedServiceInstance instance)
        {
            Context = context;
            Instance = instance;
            View = new AddConfigurationSettingsWizardPageView
            {
                DataContext = this
            };
            ReceiverSecrets = new ObservableCollection<WebHookReceiverSecret>();
            ReceiverSecrets.CollectionChanged += ReceiverSecrets_CollectionChanged;
            Title = Resources.AddConfigurationSettingsWizardPageTitle;
            Description = Resources.AddConfigurationSettingsWizardPageDescription;
            Legend = Resources.AddConfigurationSettingsWizardPageLegend;
        }

        private void ReceiverSecrets_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
                foreach (WebHookReceiverSecret option in e.NewItems)
                    option.PropertyChanged += WebHookReceiverSecret_PropertyChanged;

            if (e.OldItems != null)
                foreach (WebHookReceiverSecret option in e.OldItems)
                    option.PropertyChanged -= WebHookReceiverSecret_PropertyChanged;
        }

        private void WebHookReceiverSecret_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // todo: just here if we need to validate the secrets
            Wizard.IsFinishEnabled = true;
        }

        public override Task<PageNavigationResult> OnPageLeavingAsync(WizardLeavingArgs args)
        {
            if (!Instance.Metadata.ContainsKey(Constants.MetadataKeyForStoringReceiverSecrets))
                Instance.Metadata.Add(Constants.MetadataKeyForStoringReceiverSecrets, new List<WebHookReceiverSecret>());

            ((List<WebHookReceiverSecret>)Instance.Metadata[Constants.MetadataKeyForStoringReceiverSecrets]).Clear();
            foreach (var item in ReceiverSecrets)
            {
                ((List<WebHookReceiverSecret>)Instance.Metadata[Constants.MetadataKeyForStoringReceiverSecrets]).Add(item);
            }

            TelemetryWrapper.EndPageView();

            return base.OnPageLeavingAsync(args);
        }

        public override Task OnPageEnteringAsync(WizardEnteringArgs args)
        {
            TelemetryWrapper.StartPageView("Configure Receivers");

            Wizard.IsNextEnabled = false;
            Wizard.IsFinishEnabled = true;

            ReceiverSecrets.Clear();

            var selectedReceiverOptions = Instance.Metadata[Constants.MetadataKeyForStoringSelectedReceivers]
                as IEnumerable<WebHookReceiverOption>;

            if (selectedReceiverOptions != null)
            {
                foreach (var option in selectedReceiverOptions)
                {
                    ReceiverSecrets.Add(new WebHookReceiverSecret(option));
                }
            }

            return base.OnPageEnteringAsync(args);
        }
    }
}
