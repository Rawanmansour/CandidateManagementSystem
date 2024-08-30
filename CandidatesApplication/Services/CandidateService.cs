using CandidateDomain.DTOs;
using CandidateDomain.Entities;
using CandidateDomain.IServices;
using CsvHelper;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Formats.Asn1;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CandidatesApplication.Services
{
    public class CandidateService : ICandidateService
    {
        private readonly string _filePath;
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);
        private readonly IMemoryCache _cache;
        private readonly MemoryCacheEntryOptions _cacheOptions;
        public CandidateService(string filePath, IMemoryCache cache)
        {
            _filePath = filePath;

            if (!File.Exists(_filePath))
            {
                using (var writer = new StreamWriter(_filePath))
                using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
                {
                    csv.WriteHeader<CandidateDTO>();
                    csv.NextRecord();
                }
            }
            _cache = cache;
            _cacheOptions = new MemoryCacheEntryOptions()
                .SetSlidingExpiration(TimeSpan.FromMinutes(30));
        }

        public async Task<CandidateDTO> GetCandidateByEmail(string email)
        {
            if (_cache.TryGetValue(email, out CandidateDTO candidate))
            {
                return candidate;
            }

            await _semaphore.WaitAsync();
            try
            {
                using (var reader = new StreamReader(_filePath))
                using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
                {
                    var records = csv.GetRecords<CandidateDTO>().ToList();
                    candidate = records.FirstOrDefault(c => c.Email.Equals(email, StringComparison.OrdinalIgnoreCase));
                    if (candidate != null)
                    {
                        _cache.Set(email, candidate, _cacheOptions);
                    }
                    return candidate;
                }
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task AddOrUpdateCandidate(CandidateDTO candidate)
        {
            await _semaphore.WaitAsync();
            try
            {
                var candidates = new List<CandidateDTO>();
                if (File.Exists(_filePath))
                {
                    using (var reader = new StreamReader(_filePath))
                    using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
                    {
                        candidates = csv.GetRecords<CandidateDTO>().ToList();
                    }
                }

                var existing = candidates.FirstOrDefault(c => c.Email.Equals(candidate.Email, StringComparison.OrdinalIgnoreCase));
                if (existing != null)
                {
                    // Update existing candidate
                    existing.FirstName = candidate.FirstName;
                    existing.LastName = candidate.LastName;
                    existing.PhoneNumber = candidate.PhoneNumber;
                    existing.PreferredCallTime = candidate.PreferredCallTime;
                    existing.LinkedInUrl = candidate.LinkedInUrl;
                    existing.GitHubUrl = candidate.GitHubUrl;
                    existing.Comment = candidate.Comment;
                }
                else
                {
                    // Add new candidate
                    candidates.Add(candidate);
                    _cache.Set(candidate.Email, candidate, _cacheOptions);
                }

                using (var writer = new StreamWriter(_filePath))
                using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
                {
                    csv.WriteHeader<CandidateDTO>();
                    csv.NextRecord();
                    csv.WriteRecords(candidates);
                }
            }
            finally
            {
                _semaphore.Release();
            }
        }
    
}
}
