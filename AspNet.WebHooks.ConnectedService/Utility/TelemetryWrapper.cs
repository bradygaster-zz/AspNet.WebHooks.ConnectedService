using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AspNet.WebHooks.ConnectedService.Utility
{
    internal class TelemetryWrapper
    {
        private static Stopwatch Stopwatch { get; set; }
        private static string CurrentPage { get; set; }

        private static TelemetryClient _telemetryClient = new TelemetryClient();
        private static TelemetryClient TelemetryClient
        {
            get
            {
                if (_telemetryClient == null)
                    Refresh();

                return _telemetryClient;
            }
            set
            {
                _telemetryClient = value;
            }
        }

        internal static void Refresh()
        {
            if (_telemetryClient == null)
                return;
            else
            {
                _telemetryClient.Flush();

                _telemetryClient = new TelemetryClient();
                _telemetryClient.InstrumentationKey = Constants.InstrumentationKey;
                _telemetryClient.Context.Session.Id = Guid.NewGuid().ToString();
            }
        }

        internal static void StartPageView(string pageName)
        {
            CurrentPage = pageName;
            Stopwatch = new Stopwatch();
            Stopwatch.Start();
        }

        internal static void EndPageView()
        {
            Stopwatch.Stop();
            var elapsed = Stopwatch.Elapsed;
            TelemetryClient.TrackPageView(CurrentPage);
        }

        internal static void RecordEvent(string name,
            Dictionary<string, string> properties = null,
            Dictionary<string, double> metrics = null)
        {
            var em = new EventTelemetry(name);

            if (properties != null)
                foreach (var key in properties.Keys)
                    em.Properties.Add(key, properties[key]);

            if (metrics != null)
                foreach (var key in metrics.Keys)
                    em.Metrics.Add(key, metrics[key]);
            
            TelemetryClient.TrackEvent(em);
        }
    }
}
