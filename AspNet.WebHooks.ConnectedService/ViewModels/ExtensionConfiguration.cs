using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AspNet.WebHooks.ConnectedService.ViewModels
{
    internal class ExtensionConfiguration
    {
        public string DefaultNugetVersion { get; set; }
        public IEnumerable<WebHookReceiverOption> AvailableReceivers { get; set; }
    }
}
