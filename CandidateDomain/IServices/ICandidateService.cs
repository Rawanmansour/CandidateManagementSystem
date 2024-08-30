using CandidateDomain.DTOs;
using CandidateDomain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CandidateDomain.IServices
{
    public interface ICandidateService
    {
        Task<CandidateDTO> GetCandidateByEmail(string email);
        Task AddOrUpdateCandidate(CandidateDTO candidate);
    }
}
