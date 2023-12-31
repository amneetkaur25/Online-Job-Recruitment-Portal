using System.ComponentModel.DataAnnotations;

namespace Online_Recruitment_Portal.Models.ViewModel
{
    public class ApplicantViewModel
    {
        public int Id { get; set; }
        [RegularExpression(@"^[A-Za-z]+$", ErrorMessage = "Please enter a valid name with alphabetic characters only.")]
        [Display(Name = "Applicant Name")]
        public string ApplicantName { get; set; }
        [Required(ErrorMessage = "Please upload your resume.")]
        public IFormFile ResumeFile { get; set; }
        public DateTime ApplicationDate { get; set; } = DateTime.Now;
        public int JobPostingId { get; set; }
    }
}
