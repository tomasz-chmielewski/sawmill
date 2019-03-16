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

            // is it absolute uri ? "http://xxxxxx.xxx/section/..."
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
                this.Path = path;
                this.HitCount = hitCount;
            }

            public string Path { get; }
            public int HitCount { get; set;  }
        }
    }
}
