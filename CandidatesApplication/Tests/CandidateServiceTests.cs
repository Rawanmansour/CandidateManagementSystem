using CandidateDomain.DTOs;
using CandidatesApplication.Services;
using CsvHelper;
using Microsoft.Extensions.Caching.Memory;
using Moq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace CandidatesApplication.Tests
{
    public class CandidateServiceTests
    {
        private readonly Mock<IMemoryCache> _mockMemoryCache;
        private readonly string _testFilePath = Path.Combine(Path.GetTempPath(), "test_candidates.csv");

        public CandidateServiceTests()
        {
            _mockMemoryCache = new Mock<IMemoryCache>();
        }

        [Fact]
        public async Task AddOrUpdateCandidate_ShouldAddNewCandidate_WhenCandidateDoesNotExist()
        {
            // Arrange
            var candidateService = new CandidateService(_testFilePath, _mockMemoryCache.Object);
            var candidate = new CandidateDTO
            {
                FirstName = "rawan",
                LastName = "mansour",
                Email = "user@example.com",
                PhoneNumber = "1234567890",
                PreferredCallTime = "9AM - 5PM",
                LinkedInUrl = "https://linkedin.com/rawanmansour",
                GitHubUrl = "https://github.com/rawanmansour",
                Comment = "New candidate"
            };

            // Act
            await candidateService.AddOrUpdateCandidate(candidate);

            // Assert
            var candidates = ReadCandidatesFromFile();
            Assert.Single(candidates);
            Assert.Equal(candidate.Email, candidates.First().Email);
        }

        [Fact]
        public async Task AddOrUpdateCandidate_ShouldUpdateExistingCandidate_WhenCandidateExists()
        {
            // Arrange
            var candidateService = new CandidateService(_testFilePath, _mockMemoryCache.Object);
            var existingCandidate = new CandidateDTO
            {
                FirstName = "rawan",
                LastName = "mansour",
                Email = "user@example.com",
                PhoneNumber = "07888888",
                PreferredCallTime = "9AM - 5PM",
                LinkedInUrl = "https://linkedin.com/rawanmansour",
                GitHubUrl = "https://github.com/rawanmansour",
                Comment = "Existing candidate"
            };
            await candidateService.AddOrUpdateCandidate(existingCandidate);

            var updatedCandidate = new CandidateDTO
            {
                FirstName = "rawan",
                LastName = "mansour",
                Email = "user@example.com",
                PhoneNumber = "07888888",
                PreferredCallTime = "9AM - 5PM",
                LinkedInUrl = "https://linkedin.com/rawanmansour",
                GitHubUrl = "https://github.com/rawanmansour",
                Comment = "Updated candidate"
            };

            // Act
            await candidateService.AddOrUpdateCandidate(updatedCandidate);

            // Assert
            var candidates = ReadCandidatesFromFile();
            Assert.Single(candidates);
            Assert.Equal("mansour", candidates.First().LastName); // Verify the last name is updated
        }

        private List<CandidateDTO> ReadCandidatesFromFile()
        {
            using (var reader = new StreamReader(_testFilePath))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                return csv.GetRecords<CandidateDTO>().ToList();
            }
        }
    }
}
