using Microsoft.VisualStudio.ConnectedServices;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System;
using System.ComponentModel;
using AspNet.WebHooks.ConnectedService.Utility;
using System.Collections.Generic;
using System.Reflection;

namespace AspNet.WebHooks.ConnectedService.ViewModels
{
    public class SelectWebHooksWizardPage : ConnectedServiceWizardPage
    {
        internal ConnectedServiceProviderContext Context { get; set; }
        internal ConnectedServiceInstance Instance { get; set; }

        private ObservableCollection<WebHookReceiverOption> _webHookReceiverOptions;

        public ObservableCollection<WebHookReceiverOption> WebHookReceiverOptions
        {
            get { return _webHookReceiverOptions; }
            set
            {
                _webHookReceiverOptions = value;
                OnPropertyChanged();
            }
        }

        public string SelectWebHooksLabel { get; set; }

        public SelectWebHooksWizardPage(ConnectedServiceProviderContext context,
            ConnectedServiceInstance instance)
        {
            // set up the configuration dialog
            Title = Resources.SelectWebHooksPageTitle;
            Description = Resources.SelectWebHooksPageDescription;
            Legend = Resources.SelectWebHooksPageLegend;
            SelectWebHooksLabel = Resources.SelectWebHooksPageDescription;
            Context = context;
            Instance = instance;

            WebHookReceiverOptions = new ObservableCollection<WebHookReceiverOption>();

            // load up the view
            View = new Views.SelectWebHooksWizardPageView
            {
                DataContext = this
            };

            // handle properties changing and user input
            WebHookReceiverOptions.CollectionChanged += WebHookReceiverOptions_CollectionChanged;

            // load the receiver options from the JSON file
            LoadReceiverOptions();
        }

        public override Task OnPageEnteringAsync(WizardEnteringArgs args)
        {
            base.Wizard.IsNextEnabled = WebHookReceiverOptions.Any(x => x.IsChecked == true);
            base.Wizard.Pages[1].IsEnabled = WebHookReceiverOptions.Any(x => x.IsChecked == true);

            TelemetryWrapper.StartPageView("Select Receivers");

            return base.OnPageEnteringAsync(args);
        }

        public override Task<PageNavigationResult> OnPageLeavingAsync(WizardLeavingArgs args)
        {
            TelemetryWrapper.EndPageView();

            return base.OnPageLeavingAsync(args);
        }

        private void WebHookReceiverOptions_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
                foreach (WebHookReceiverOption option in e.NewItems)
                    option.PropertyChanged += WebHookOption_PropertyChanged;

            if (e.OldItems != null)
                foreach (WebHookReceiverOption option in e.OldItems)
                    option.PropertyChanged -= WebHookOption_PropertyChanged;
        }

        private void WebHookOption_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.Wizard.IsNextEnabled = WebHookReceiverOptions.Any(x => x.IsChecked == true);
            base.Wizard.Pages[1].IsEnabled = WebHookReceiverOptions.Any(x => x.IsChecked == true);

            if (!Instance.Metadata.Any(x => x.Key == Constants.MetadataKeyForStoringSelectedReceivers))
                Instance.Metadata.Add(Constants.MetadataKeyForStoringSelectedReceivers, string.Empty);

            var selected = WebHookReceiverOptions.Where(x => x.IsChecked == true).ToList();
            Instance.Metadata[Constants.MetadataKeyForStoringSelectedReceivers] = selected;
        }

        private void LoadReceiverOptions()
        {
            // open the JSON file containing the list of receivers and NuGets
            Stream templateStream = Assembly.GetAssembly(typeof(GeneratedCodeHelper))
                .GetManifestResourceStream(
                    "AspNet.WebHooks.ConnectedService.Content.WebHooksConnectedServiceConfig.json"
                );

            StreamReader rdr = //new StreamReader(@"Content\WebHooksConnectedServiceConfig.json");
                new StreamReader(templateStream);

            JsonSerializer serializer = new JsonSerializer();
            JObject json = serializer.Deserialize<JObject>(new JsonTextReader(rdr));

            // get the receivers
            JArray receivers = json["availableReceivers"].Value<JArray>();

            foreach (var receiver in receivers.Children())
            {
                WebHookReceiverOptions.Add(
                    serializer.Deserialize<WebHookReceiverOption>(new JTokenReader(receiver))
                    );
            }

            var tmp = WebHookReceiverOptions.OrderBy(x => x.Name).ToList();
            WebHookReceiverOptions.Clear();
            tmp.ForEach(x => WebHookReceiverOptions.Add(x));
        }
    }
}
