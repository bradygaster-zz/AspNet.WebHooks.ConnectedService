using AspNet.WebHooks.ConnectedService.Utility;
using AspNet.WebHooks.ConnectedService.ViewModels;
using Microsoft.VisualStudio.ConnectedServices;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

namespace AspNet.WebHooks.ConnectedService
{
    [ConnectedServiceProviderExport(Constants.ConnectedServiceName)]
    internal class Provider : ConnectedServiceProvider
    {
        public Provider()
        {
            Category = Resources.ConnectedServiceCategory;
            Name = Resources.ConnectedServiceDialogTitle;
            Description = Resources.ConnectedServiceListDescription;
            CreatedBy = "Microsoft";
            Version = new Version(1, 0, 0);
            MoreInfoUri = new Uri(Resources.MoreInfoUrl);
            Icon = Imaging
                .CreateBitmapSourceFromHBitmap(
                    Resources.webhook.GetHbitmap(),
                    IntPtr.Zero,
                    Int32Rect.Empty,
                    BitmapSizeOptions.FromWidthAndHeight(64, 64)
                );
        }

        public override Task<ConnectedServiceConfigurator> CreateConfiguratorAsync(ConnectedServiceProviderContext context)
        {
            ConnectedServiceInstance instance = new ConnectedServiceInstance();
            ConnectedServiceConfigurator configurator = new AddWebHookWizard(context, instance);
            TelemetryWrapper.Refresh();

            return Task.FromResult(configurator);
        }
    }
}
