using Sawmill.Components.Statistics.Collectors.Abstractions;
using Sawmill.Data.Models.Abstractions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Sawmill.Components.Statistics.Collectors
{
    /// <summary>
    /// Collects statistics related to URL sections.
    /// </summary>
    public class UrlSections : IStatisticsCollector, IEnumerable<IUrlSection>
    {
        private Dictionary<string, UrlSection> Sections { get; }
            = new Dictionary<string, UrlSection>(StringComparer.Ordinal);

        private SortedDictionary<int, HashSet<UrlSection>> SectionIndex { get; }
            = new SortedDictionary<int, HashSet<UrlSection>>(Comparer<int>.Create((a, b) => b.CompareTo(a)));

        /// <summary>
        /// Process the specified log entry.
        /// </summary>
        /// <param name="logEntry">Log entry to process.</param>
        /// <returns>true if the entry was processed or false otherwise.</returns>
        public bool Process(ILogEntry logEntry)
        {
            var path = this.GetSectionPath(logEntry);
            if(path == null)
            {
                return false;
            }

            if(this.Sections.TryGetValue(path, out var section))
            {
                this.RemoveFromIndex(section);
                section.HitCount++;
                this.AddToIndex(section);
            }
            else
            {
                section = new UrlSection(path, 1);
                this.Sections[path] = section;
                this.AddToIndex(section);
            }

            return true;
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection in the descending order by number of hits on the specified URL section.
        /// </summary>
        /// <returns>An enumerator that can be used to iterate through the collection in the descending order by number of hits on the specified URL section.</returns>
        public IEnumerator<IUrlSection> GetEnumerator()
        {
            return this.SectionIndex.SelectMany(x => x.Value).Cast<IUrlSection>().GetEnumerator();
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection in the descending order by number of hits on the specified URL section.
        /// </summary>
        /// <returns>An enumerator that can be used to iterate through the collection in the descending order by number of hits on the specified URL section.</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        private void AddToIndex(UrlSection section)
        {
            var key = section.HitCount;

            if(this.SectionIndex.TryGetValue(key, out var sections))
            {
                sections.Add(section);
            }
            else
            {
                sections = new HashSet<UrlSection> { section };
                this.SectionIndex.Add(section.HitCount, sections);
            }
        }

        private void RemoveFromIndex(UrlSection section)
        {
            var key = section.HitCount;

            if(this.SectionIndex.TryGetValue(key, out var sections))
            {
                sections.Remove(section);
                if(sections.Count == 0)
                {
                    this.SectionIndex.Remove(key);
                }
            }
        }

        private string GetSectionPath(ILogEntry logEntry)
        {
            // TODO: Optimize this method

            var uri = logEntry.Request.Uri;

            // convert to absolute uri, because Uri.Segments doesn't work with relative uris
            if (!uri.IsAbsoluteUri)
            {
                if(!Uri.TryCreate(new Uri("http://dummy.com"), uri, out uri))
                {
                    return null;
                }
            }

            var segmentCount = uri.Segments.Length;

            // take no more then 2 first segments
            var path = string.Concat(uri.Segments.Take(Math.Min(2, segmentCount)));

            // remove "/" from the end if there are more then one secions
            return segmentCount > 1 && path[path.Length - 1] == '/'
                ? path.Remove(path.Length - 1)
                : path;
        }

        private class UrlSection : IUrlSection
        {
            public UrlSection(string path, int hitCount)
            {
                this.SectionPath = path;
                this.HitCount = hitCount;
            }

            public string SectionPath { get; }
            public int HitCount { get; set;  }
        }
    }
}
