using Microsoft.VisualStudio.ConnectedServices;
using System.Threading.Tasks;

namespace AspNet.WebHooks.ConnectedService.ViewModels
{
    public class AddWebHookWizard : ConnectedServiceWizard
    {
        internal ConnectedServiceProviderContext Context { get; set; }
        internal ConnectedServiceInstance Instance { get; set; }

        public AddWebHookWizard(ConnectedServiceProviderContext context,
            ConnectedServiceInstance instance)
        {
            Context = context;
            Instance = instance;
            Pages.Add(new SelectWebHooksWizardPage(context,instance));
            Pages.Add(new AddConfigurationSettingsWizardPage(context, instance));
            IsNextEnabled = false;
        }

        public override Task<ConnectedServiceInstance> GetFinishedServiceInstanceAsync()
        {
            return Task.FromResult(Instance);
        }
    }
}
