using System;
using System.Collections.Generic;

namespace dotnet_utcareers.DTOs
{
    public class PaginatedResponse<T>
    {
        public IEnumerable<T> Data { get; set; }
        public int CurrentPage { get; set; }
        public int From { get; set; }
        public int LastPage { get; set; }
        public string Path { get; set; }
        public int PerPage { get; set; }
        public int To { get; set; }
        public int Total { get; set; }
        public string FirstPageUrl { get; set; }
        public string LastPageUrl { get; set; }
        public string NextPageUrl { get; set; }
        public string PrevPageUrl { get; set; }
        public List<PageLink> Links { get; set; }

        public PaginatedResponse()
        {
            Links = new List<PageLink>();
        }
    }

    public class PageLink
    {
        public string Url { get; set; }
        public string Label { get; set; }
        public bool Active { get; set; }
    }
}