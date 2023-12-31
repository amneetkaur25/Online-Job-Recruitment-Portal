using Online_Recruitment_Portal.Data;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Online_Recruitment_Portal.Models
{
    public class Applicant
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Display(Name = "Applicant Name")]
        public string ApplicantName { get; set; }

        [Required]
        [Display(Name = "Resume")]
        public string ResumeFile { get; set; }

        [Display(Name = "Application Date")]
        public DateTime ApplicationDate { get; set; } = DateTime.Now;

        [Display(Name = "Job Posting")]
        public int JobPostingId { get; set; }

        [ForeignKey("JobPostingId")]
        public JobPosting JobPosting { get; set; }

        [Display(Name = "User ID")]
        public string UserId { get; set; }

        [ForeignKey("UserId")]
        public ApplicationUser User { get; set; }
    }
}
