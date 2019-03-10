using Sawmill.Components.Statistics.Collectors.Abstractions;
using Sawmill.Models.Abstractions;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Sawmill.Components.Statistics.Collectors
{
    public class UrlSections : IStatisticsCollector
    {
        public UrlSections(string name)
        {
            this.Name = name;
        }

        public string Name { get; }
        public string Value => this.GetValue();

        public SortedDictionary<string, int> Mapping { get; } = new SortedDictionary<string, int>();

        public bool Process(ILogEntry logEntry)
        {
            var section = this.GetSection(logEntry);

            if(this.Mapping.TryGetValue(section, out int hits))
            {
                this.Mapping[section] = hits + 1;
            }
            else
            {
                this.Mapping[section] = 1;
            }

            return true;
        }

        private string GetSection(ILogEntry logEntry)
        {
            var uri = logEntry.Request.Uri;

            // convert to absolute uri, because Uri.Segments doesn't work with relative uris
            var absoluteUri = uri.IsAbsoluteUri ? uri : new Uri(new Uri("http://dummy"), uri);
            var segmentCount = absoluteUri.Segments.Length;

            // take no more then 2 first segments
            var section = string.Concat(absoluteUri.Segments.Take(Math.Min(2, segmentCount)));

            // remove "/" from the end if there are more then one secions
            return segmentCount > 1 && section[section.Length - 1] == '/' 
                ? section.Remove(section.Length - 1)
                : section;
        }

        private string GetValue()
        {
            var sb = new StringBuilder();
            var isFirstSection = true;

            foreach(var section in this.Mapping)
            {
                if(!isFirstSection)
                {
                    sb.Append(", ");
                }

                sb.Append(section.Key);
                sb.Append(": ");
                sb.Append(section.Value.ToString(CultureInfo.InvariantCulture));

                isFirstSection = false;
            }

            return sb.ToString();
        }
    }
}
