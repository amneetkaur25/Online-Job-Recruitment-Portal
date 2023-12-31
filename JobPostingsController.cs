using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Online_Recruitment_Portal.Data;
using Online_Recruitment_Portal.Models;
using Online_Recruitment_Portal.Models.ViewModel;

namespace Online_Recruitment_Portal.Controllers
{
    [Authorize(Roles ="User")]
    public class JobPostingsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment webHostEnvironment;
        private readonly UserManager<IdentityUser> userManager;

        public JobPostingsController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment, UserManager<IdentityUser> userManager)
        {
            _context = context;
            this.webHostEnvironment = webHostEnvironment;
            this.userManager = userManager;
        }
        // GET: JobPostings
        public async Task<IActionResult> JobPostings()
        {
            var jobs = await _context.Jobs.ToListAsync();
            return View(jobs);
                       
        }
        public IActionResult AddVacancy()
        {
            return View();
        }

        // POST: JobPostings/Create
       
        [HttpPost]
        public async Task<IActionResult> Create([Bind("Id,JobTitle,CompanyName,JobDescription,Location,PostedDate,ApplicationDeadline,Status")] JobPosting jobPosting)
        {
            if (ModelState.IsValid)
            {
                _context.Add(jobPosting);
                await _context.SaveChangesAsync();
                TempData["Message"] = "Job posting submitted for approval. An admin will review it.";
                return RedirectToAction(nameof(JobPostings));

            }
            return View(jobPosting);
        }

        // GET: JobPostings/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Jobs == null)
            {
                return NotFound();
            }

            var jobPosting = await _context.Jobs.FindAsync(id);
            if (jobPosting == null)
            {
                return NotFound();
            }
            return View(jobPosting);
        }

        // POST: JobPostings/Edit/5

        [HttpPost]
        public async Task<IActionResult> Edit(int id, [Bind("Id,JobTitle,CompanyName,JobDescription,Location,PostedDate,ApplicationDeadline,Status")] JobPosting jobPosting)
        {
            if (id != jobPosting.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(jobPosting);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!JobPostingExists(jobPosting.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(JobPostings));
            }
            return View(jobPosting);
        }

        // GET: JobPostings/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Jobs == null)
            {
                return NotFound();
            }

            var jobPosting = await _context.Jobs
                .FirstOrDefaultAsync(m => m.Id == id);
            if (jobPosting == null)
            {
                return NotFound();
            }

            return View(jobPosting);
        }

        // POST: JobPostings/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Jobs == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Jobs'  is null.");
            }
            var jobPosting = await _context.Jobs.FindAsync(id);
            if (jobPosting != null)
            {
                _context.Jobs.Remove(jobPosting);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(JobPostings));
        }
        public async Task<IActionResult> MyJobs()
        {
            var currentDate = DateTime.Now;
            var query = _context.Jobs
                .Where(j => j.Status == JobStatus.Approved && j.ApplicationDeadline >= currentDate);

            var approvedJobPostings = await query.ToListAsync();
            return View(approvedJobPostings);
        }
        //Apply For Jobs
        public IActionResult Apply(int id)
        {
            var jobPosting = _context.Jobs.FirstOrDefault(j => j.Id == id);

            if (jobPosting == null)
            {
                return NotFound();
            }
            var applicant = new ApplicantViewModel
            {
                JobPostingId = jobPosting.Id 
            };

            return View(applicant);
        }


        [HttpPost]
        public async Task<IActionResult> Apply(ApplicantViewModel viewModel)
        {
            if (ModelState.IsValid)
            {   // Check if the user has already applied for this job posting
                var user = await userManager.GetUserAsync(User);
                var existingApplication = _context.JobApplications
                    .FirstOrDefault(a => a.JobPostingId == viewModel.JobPostingId && a.UserId==user.Id);

                if (existingApplication != null)
                {
                    ModelState.AddModelError(string.Empty, "You have already applied for this job.");
                    return View(viewModel);
                }

                // Check allowed extensions (pdf, doc, docx, txt)
                var allowedExtensions = new[] { ".pdf", ".doc", ".docx", ".txt" ,".jpg"};
                var fileExtension = Path.GetExtension(viewModel.ResumeFile.FileName).ToLower();
                if (!allowedExtensions.Contains(fileExtension))
                {
                    ModelState.AddModelError(nameof(viewModel.ResumeFile), "Only PDF, Word,Image and Text files are allowed.");
                    return View(viewModel);
                }
                // Check file size (500KB limit)
                if (viewModel.ResumeFile.Length > 500 * 1024)
                {
                    ModelState.AddModelError(nameof(viewModel.ResumeFile), "File size should be less than 500KB.");
                    return View(viewModel);
                }
                // Save the uploaded resume file
                string uniqueFileName = null;
                if (viewModel.ResumeFile != null)
                {
                    string uploadsFolder = Path.Combine(webHostEnvironment.WebRootPath, "Resumes");
                    uniqueFileName = Guid.NewGuid().ToString() + "_" + viewModel.ResumeFile.FileName;
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                    viewModel.ResumeFile.CopyTo(new FileStream(filePath, FileMode.Create));
                }

                // Create a new job application and save it to the database
                var jobApplication = new Applicant
                {
                    ApplicantName = viewModel.ApplicantName,
                    ResumeFile = uniqueFileName,
                    JobPostingId = viewModel.JobPostingId,
                    UserId = user.Id
                };

                _context.JobApplications.Add(jobApplication);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "You have successfully applied for the job.";
                return RedirectToAction("MyJobs", "JobPostings");
            }
            return View(viewModel);
        }
     
        private bool JobPostingExists(int id)
        {
          return (_context.Jobs?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
