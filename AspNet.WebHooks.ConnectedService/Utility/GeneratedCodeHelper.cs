using Microsoft.VisualStudio.ConnectedServices;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.TextTemplating;
using Microsoft.VisualStudio.TextTemplating.VSHost;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AspNet.WebHooks.ConnectedService.Utility
{
    public static class GeneratedCodeHelper
    {
        private static ITextTemplating TextTemplating
        {
            get { return (ITextTemplating)Package.GetGlobalService(typeof(STextTemplating)); }
        }

        public static async System.Threading.Tasks.Task GenerateCodeFromTemplateAndAddToProject(
            ConnectedServiceHandlerContext context,
            string templateFileName,
            string targetPath,
            IDictionary<string, object> parameters)
        {
            ITextTemplating t4 = TextTemplating;
            ITextTemplatingSessionHost sessionHost = (ITextTemplatingSessionHost)t4;
            sessionHost.Session = sessionHost.CreateSession();

            if (parameters != null)
                foreach (string key in parameters.Keys)
                    sessionHost.Session[key] = parameters[key];

            await context.Logger.WriteMessageAsync(LoggerMessageCategory.Information,
                "Opening the template '{0}'",
                templateFileName);

            //Stream templateStream = File.OpenRead(
            //    string.Format(@"Content\{0}.tt", templateFileName)
            //    );

            Stream templateStream = Assembly.GetAssembly(typeof(GeneratedCodeHelper))
                .GetManifestResourceStream(
                    string.Format("AspNet.WebHooks.ConnectedService.Content.{0}.tt", templateFileName)
                );

            if (templateStream == null)
            {
                throw new Exception("Could not find code generation template");
            }

            string templateContent = new StreamReader(templateStream).ReadToEnd();

            await context.Logger.WriteMessageAsync(LoggerMessageCategory.Information,
                "Generating code from template '{0}'",
                templateFileName);

            string generatedCode = t4.ProcessTemplate("", templateContent, new T4Callback(context));
            string tempFile = CreateTempFile(generatedCode);

            await context.Logger.WriteMessageAsync(LoggerMessageCategory.Information,
                "Adding code generated from template '{0}' as new file {1}",
                templateFileName,
                targetPath);

            await context.HandlerHelper.AddFileAsync(tempFile, targetPath);
        }

        private static string CreateTempFile(string contents)
        {
            string tempFileName = Path.GetTempFileName();
            File.WriteAllText(tempFileName, contents);
            return tempFileName;
        }
    }

    internal class T4Callback : ITextTemplatingCallback
    {
        private readonly ConnectedServiceHandlerContext _context;
        public readonly List<string> ErrorMessages = new List<string>();

        public T4Callback(ConnectedServiceHandlerContext context)
        {
            _context = context;
        }

        public void ErrorCallback(bool warning, string message, int line, int column)
        {
            ErrorMessages.Add(message);

            _context.Logger.WriteMessageAsync(LoggerMessageCategory.Error,
                "Error during generation: '{0}'",
                message).Wait();
        }

        public void SetFileExtension(string extension)
        {
        }

        public void SetOutputEncoding(Encoding encoding, bool fromOutputDirective)
        {
        }
    }
}
