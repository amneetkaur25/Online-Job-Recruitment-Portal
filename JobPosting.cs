using System.ComponentModel.DataAnnotations;

namespace Online_Recruitment_Portal.Models
{
    public class JobPosting
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string JobTitle { get; set; }

        [Required]
        public string CompanyName { get; set; }

        [Required]
        public string JobDescription { get; set; }

        [Required]
        public string Location { get; set; }

        [Display(Name = "Posted Date")]
        public DateTime PostedDate { get; set; }

        //[Display(Name = "Application Deadline")]
        //[Required]
        //public DateTime ApplicationDeadline { get; set; }
        private DateTime _applicationDeadline;
        public DateTime ApplicationDeadline
        {
            get { return _applicationDeadline; }
            set
            {
                if (IsApplicationDeadlineValid(value))
                {
                    _applicationDeadline = value;
                    ApplicationDeadlineErrorMessage = null; // Clear any previous error message
                }
                else
                {
                    ApplicationDeadlineErrorMessage = "Application Deadline cannot be earlier than Posted Date.";
                }
            }
        }

        public string ApplicationDeadlineErrorMessage { get; private set; }

        private bool IsApplicationDeadlineValid(DateTime value)
        {
            // Ensure that the ApplicationDeadline is not earlier than PostedDate.
            return value >= PostedDate;
        }

        // Other properties and methods...
    

    public JobStatus Status { get; set; } = JobStatus.Pending;
    }

    public enum JobStatus
    {
        Pending,
        Approved,
        Rejected
    }
}
