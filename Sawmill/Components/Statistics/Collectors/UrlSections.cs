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
            var uri = logEntry.Request.Uri.ToString().AsSpan();

            var firstSlash = this.IndexOfRelativePath(uri);
            if(firstSlash < 0)
            {
                return null;
            }

            uri = uri.Slice(firstSlash);
            var secondSlash = uri.Length > 0 ? uri.Slice(1).IndexOfAny('/', '?') : -1;

            return (secondSlash != -1 ? uri.Slice(0, secondSlash + 1) : uri).ToString();
        }

        private int IndexOfRelativePath(ReadOnlySpan<char> uri)
        {
            var slashIndex = uri.IndexOf('/');

            // Is it an absolute uri ? "http://xxxxxx.xxx/section/..."
            if(slashIndex > 0 && uri[slashIndex - 1] == ':')
            {
                if(slashIndex + 2 >= uri.Length || uri[slashIndex + 1] != '/')
                {
                    return -1;
                }

                slashIndex += 2 + uri.Slice(slashIndex + 2).IndexOf('/');
            }

            return slashIndex;
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
