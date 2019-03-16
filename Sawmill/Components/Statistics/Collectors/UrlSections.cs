using Sawmill.Components.Statistics.Collectors.Abstractions;
using Sawmill.Data.Models.Abstractions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Sawmill.Components.Statistics.Collectors
{
    public class UrlSections : IStatisticsCollector, IEnumerable<IUrlSection>
    {
        private Dictionary<string, UrlSection> Sections { get; } 
            = new Dictionary<string, UrlSection>(StringComparer.Ordinal);

        private SortedDictionary<int, HashSet<UrlSection>> SectionIndex { get; }
            = new SortedDictionary<int, HashSet<UrlSection>>(Comparer<int>.Create((a, b) => b.CompareTo(a)));

        public bool Process(ILogEntry logEntry)
        {
            var path = this.GetSectionPath(logEntry);

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

        public IEnumerator<IUrlSection> GetEnumerator()
        {
            return this.SectionIndex.SelectMany(x => x.Value).Cast<IUrlSection>().GetEnumerator();
        }

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

            var firstSlash = uri.IndexOf('/');
            var secondSlash = firstSlash != -1 && firstSlash + 1 < uri.Length ? uri.Slice(firstSlash + 1).IndexOfAny('/', '?') : -1;

            return (secondSlash != -1 ? uri.Slice(0, firstSlash + secondSlash + 1) : uri).ToString();
        }

        private class UrlSection : IUrlSection
        {
            public UrlSection(string path, int hitCount)
            {
                this.Path = path;
                this.HitCount = hitCount;
            }

            public string Path { get; }
            public int HitCount { get; set;  }
        }
    }
}
