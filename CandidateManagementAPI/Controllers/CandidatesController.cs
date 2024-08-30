using CandidateDomain.DTOs;
using CandidateDomain.Entities;
using CandidateDomain.IServices;
using CandidateManagementAPI.Models;
using Microsoft.AspNetCore.Mvc;

namespace CandidateManagementAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CandidatesController : Controller
    {
        private readonly ICandidateService _service;
        public CandidatesController(ICandidateService service)
        {
            _service = service;
        }
        [HttpPost]
        public async Task<IActionResult> AddOrUpdateCandidate([FromBody] CandidateViewModel candidate)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            //Mapping ViewModel To DTO
            CandidateDTO candidateDTO = new CandidateDTO
            {
                FirstName = candidate.FirstName,
                LastName = candidate.LastName,
                Comment = candidate.Comment,
                Email = candidate.Email,
                GitHubUrl = candidate.GitHubUrl,
                LinkedInUrl = candidate.LinkedInUrl,
                PhoneNumber = candidate.PhoneNumber,
                PreferredCallTime = candidate.PreferredCallTime
            };
            await _service.AddOrUpdateCandidate(candidateDTO);
            return Ok(new { message = "Candidate added or updated successfully." });
        }
        [HttpGet]
        public async Task<IActionResult> GetCandidate(string email)
        {
            var candidate =await _service.GetCandidateByEmail(email);
            if (candidate == null)
            {
                return Ok("Candidate Not Found");
            }
            return Ok(candidate);
        }
    }
}
