using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Online_Recruitment_Portal.Data;
using Online_Recruitment_Portal.Models;

namespace Online_Recruitment_Portal.Controllers
{
    [Authorize(Roles ="Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment webHostEnvironment;
        public AdminController(ApplicationDbContext context,IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            this.webHostEnvironment = webHostEnvironment;
        }
        public async Task<IActionResult> ManageJobs()
        {
            var JobPostings = await _context.Jobs.Where(j=>j.ApplicationDeadline>=DateTime.Now)
                .ToListAsync();
            return View(JobPostings);
        }
        [HttpPost]
        public async Task<IActionResult> Approve(int id)
        {
            var jobPosting = await _context.Jobs.FindAsync(id);

            if (jobPosting == null)
            {
                return NotFound();
            }
            if (jobPosting.Status == JobStatus.Approved)
            {
                TempData["Message"] = "The job is already approved.";
                return RedirectToAction(nameof(ManageJobs));
            }
            // Set the status to "Approved"
            jobPosting.Status = JobStatus.Approved;
            _context.Update(jobPosting);
            await _context.SaveChangesAsync();
            TempData["ApproveMessage"] = "The job has been approved.";
            return RedirectToAction(nameof(ManageJobs));
        }
        [HttpPost]
        public async Task<IActionResult> Reject(int id)
        {
            var jobPosting = await _context.Jobs.FindAsync(id);

            if (jobPosting == null)
            {
                return NotFound();
            }
            // Check if the job posting was previously approved
            if (jobPosting.Status == JobStatus.Approved)
            {
                // Delete previous job applications for this job posting
                var previousApplications = _context.JobApplications
                    .Where(a => a.JobPostingId == id)
                    .ToList();

                _context.JobApplications.RemoveRange(previousApplications);
            }

            // Set the status to "Rejected"
            jobPosting.Status = JobStatus.Rejected;
            _context.Update(jobPosting);
            await _context.SaveChangesAsync();
            TempData["RejectedMessage"] = "The job has been rejected.";
            return RedirectToAction(nameof(ManageJobs));
        }

        public async Task<IActionResult> AllJobs()
        {
            var currentDate = DateTime.Now;
            var approvedJobPostings = await _context.Jobs
                .Where(j => j.Status == JobStatus.Approved && j.ApplicationDeadline>=currentDate).ToListAsync();
            return View(approvedJobPostings);
        }
        public async Task<IActionResult> ViewApplications(int jobId)
        {
            var jobApplications = await _context.JobApplications
         .Where(a => a.JobPostingId == jobId).OrderByDescending(a => a.ApplicationDate).ToListAsync();
            return View(jobApplications);
        }
        public IActionResult Download(int id)
        {
            var applicant = _context.JobApplications.FirstOrDefault(a => a.Id == id);

            if (applicant == null)
            {
                return NotFound();
            }
            var filePath = Path.Combine(webHostEnvironment.WebRootPath, "Resumes", applicant.ResumeFile);
            if (System.IO.File.Exists(filePath))
            {
                return PhysicalFile(filePath, "application/octet-stream", applicant.ResumeFile);
            }
            else
            {
                return NotFound();
            }
        }
    }
}

