using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using dotnet_utcareers.DTOs;

namespace dotnet_utcareers.Services
{
    public static class PaginationService
    {
        public static PaginatedResponse<T> CreatePaginatedResponse<T>(
            IEnumerable<T> data,
            int totalCount,
            int page,
            int perPage,
            HttpRequest request)
        {
            var totalPages = (int)Math.Ceiling((double)totalCount / perPage);
            var from = totalCount > 0 ? ((page - 1) * perPage) + 1 : 0;
            var to = Math.Min(from + perPage - 1, totalCount);
            
            var baseUrl = $"{request.Scheme}://{request.Host}{request.Path}";
            
            var response = new PaginatedResponse<T>
            {
                Data = data,
                CurrentPage = page,
                From = from,
                LastPage = totalPages,
                Path = baseUrl,
                PerPage = perPage,
                To = to,
                Total = totalCount,
                FirstPageUrl = BuildUrl(baseUrl, 1, perPage, request.Query),
                LastPageUrl = BuildUrl(baseUrl, totalPages, perPage, request.Query),
                NextPageUrl = page < totalPages ? BuildUrl(baseUrl, page + 1, perPage, request.Query) : null,
                PrevPageUrl = page > 1 ? BuildUrl(baseUrl, page - 1, perPage, request.Query) : null
            };

            // Build pagination links
            response.Links = BuildPaginationLinks(baseUrl, page, totalPages, perPage, request.Query);

            return response;
        }

        private static string BuildUrl(string baseUrl, int page, int perPage, IQueryCollection query)
        {
            var queryParams = new List<string>();
            
            // Add existing query parameters except page and per_page
            foreach (var param in query)
            {
                if (param.Key != "page" && param.Key != "per_page")
                {
                    queryParams.Add($"{param.Key}={param.Value}");
                }
            }
            
            queryParams.Add($"page={page}");
            queryParams.Add($"per_page={perPage}");
            
            return $"{baseUrl}?{string.Join("&", queryParams)}";
        }

        private static List<PageLink> BuildPaginationLinks(string baseUrl, int currentPage, int totalPages, int perPage, IQueryCollection query)
        {
            var links = new List<PageLink>();

            // Previous link
            links.Add(new PageLink
            {
                Url = currentPage > 1 ? BuildUrl(baseUrl, currentPage - 1, perPage, query) : null,
                Label = "&laquo; Previous",
                Active = false
            });

            // Page number links
            var startPage = Math.Max(1, currentPage - 2);
            var endPage = Math.Min(totalPages, currentPage + 2);

            for (int i = startPage; i <= endPage; i++)
            {
                links.Add(new PageLink
                {
                    Url = BuildUrl(baseUrl, i, perPage, query),
                    Label = i.ToString(),
                    Active = i == currentPage
                });
            }

            // Next link
            links.Add(new PageLink
            {
                Url = currentPage < totalPages ? BuildUrl(baseUrl, currentPage + 1, perPage, query) : null,
                Label = "Next &raquo;",
                Active = false
            });

            return links;
        }
    }
}