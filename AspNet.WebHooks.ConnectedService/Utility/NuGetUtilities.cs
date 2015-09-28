using EnvDTE;
using Microsoft.VisualStudio.ConnectedServices;
using NuGet;
using NuGet.VisualStudio;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AspNet.WebHooks.ConnectedService.Utility
{
    public class NuGetUtilities
    {
        [Import]
        private IVsPackageInstaller NuGetInstaller { get; set; }

        private void InstallNuGetPackages(ConnectedServiceHandlerContext context, Project project)
        {
            string feed = NuGetConstants.DefaultFeedUrl;
            
            NuGetInstaller.InstallPackage(
                feed,
                project,
                "simple.odata.client",
                "4.10.0",
                false
                );
        }
    }
}
