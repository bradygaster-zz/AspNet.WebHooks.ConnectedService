using AspNet.WebHooks.ConnectedService.Utility;
using AspNet.WebHooks.ConnectedService.ViewModels;
using EnvDTE;
using Microsoft.VisualStudio.ConnectedServices;
using NuGet;
using NuGet.VisualStudio;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Threading;
using System.Threading.Tasks;

namespace AspNet.WebHooks.ConnectedService
{
    [ConnectedServiceHandlerExport(Constants.ConnectedServiceName, AppliesTo = "CSharp")]
    internal class Handler : ConnectedServiceHandler
    {
        [Import]
        private IVsPackageInstaller NuGetInstaller { get; set; }

        private Project Project { get; set; }

        public async override Task<AddServiceInstanceResult> AddServiceInstanceAsync(ConnectedServiceHandlerContext context,
            CancellationToken ct)
        {
            AddServiceInstanceResult result = new AddServiceInstanceResult(
                Resources.ConnectedServiceProjectFolderName,
                new Uri(Resources.MoreInfoUrl));

            Project = ProjectHelper.GetProjectFromHierarchy(context.ProjectHierarchy);
            string projectNamespace = Project.Properties.Item("DefaultNamespace").Value.ToString();

            // get the collection of keys to be added
            var receiverSecrets = context.ServiceInstance.Metadata[Constants.MetadataKeyForStoringReceiverSecrets]
                as IEnumerable<WebHookReceiverSecret>;

            // install all of the base WebHook NuGets
            await context.Logger.WriteMessageAsync(LoggerMessageCategory.Information,
                Resources.LogMessageGettingCoreNuGets);

            InstallPackage("Microsoft.AspNet.WebApi.WebHost", "5.2.3");
            InstallPackage("Microsoft.AspNet.WebHooks.Common", "1.2.0-beta3a");
            InstallPackage("Microsoft.AspNet.WebHooks.Receivers", "1.2.0-beta3a");

            // remember the list of providers selected
            List<string> providers = new List<string>();

            using (EditableXmlConfigHelper configHelper = context.CreateEditableXmlConfigHelper())
            {
                // iterate over the selected hook receivers
                foreach (var item in receiverSecrets)
                {
                    // add the keys in the web.config for each provider
                    configHelper.SetAppSetting(item.ReceiverSecretConfigSettingName, item.Secret);

                    // pull in the NuGet for the receiver
                    await context.Logger.WriteMessageAsync(LoggerMessageCategory.Information,
                        string.Format(Resources.LogMessageGettingReceiver, item.Option.Name));

                    InstallPackage(item.Option.NuGetPackage, item.Option.NuGetVersionOverride);

                    var receiverName = ((string.IsNullOrEmpty(item.Option.ConfigWireupOverride))
                                    ? item.Option.Name
                                    : item.Option.ConfigWireupOverride);

                    // add the handler code to the project
                    await GeneratedCodeHelper
                        .GenerateCodeFromTemplateAndAddToProject(
                            context,
                            "WebHookHandler",
                            string.Format($@"WebHookHandlers\{receiverName}WebHookHandler.cs"),
                            new Dictionary<string, object>
                            {
                                {"ns", projectNamespace},
                                {"receiverName", receiverName }
                            });

                    // remember this provider
                    providers.Add(receiverName);

                    // record the telemetry for the receiver
                    TelemetryWrapper.RecordEvent($"{item.Option.Name}");
                }

                // add the code to the project to configure all of the providers
                await GeneratedCodeHelper
                    .GenerateCodeFromTemplateAndAddToProject(
                        context,
                        "WebHookConfig",
                        string.Format($@"App_Start\WebHookConfig.cs"),
                        new Dictionary<string, object>
                        {
                            {"ns", projectNamespace},
                            {"providers", providers.ToArray() }
                        });

                // persist the configuration changes
                configHelper.Save();
            }

            // record that we finished
            TelemetryWrapper.RecordEvent("WebHook Experience Completed");

            // return
            return result;
        }

        private void InstallPackage(string name, string version)
        {
            NuGetInstaller.InstallPackage(NuGetConstants.DefaultFeedUrl, Project, name, version, false);

            // record the telemetry for the nuget install
            TelemetryWrapper.RecordEvent("NuGet Install",
                properties: new Dictionary<string, string>()
                {
                    {"Package", name},
                    {"Version", version}
                });
        }
    }
}
