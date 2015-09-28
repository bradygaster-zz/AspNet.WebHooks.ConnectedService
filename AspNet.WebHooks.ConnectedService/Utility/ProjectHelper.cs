using EnvDTE;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;

namespace AspNet.WebHooks.ConnectedService.Utility
{
    internal static class ProjectHelper
    {
        public static Project GetProjectFromHierarchy(IVsHierarchy projectHierarchy)
        {
            object projectObject;
            int result = projectHierarchy.GetProperty(
                VSConstants.VSITEMID_ROOT,
                (int)__VSHPROPID.VSHPROPID_ExtObject,
                out projectObject);
            ErrorHandler.ThrowOnFailure(result);
            return (Project)projectObject;
        }
    }
}
