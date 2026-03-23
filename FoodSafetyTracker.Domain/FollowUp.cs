using System.ComponentModel.DataAnnotations;

namespace FoodSafetyTracker.Domain
{
    public class FollowUp
    {
        public int Id { get; set; }

        public int InspectionId { get; set; }
        public Inspection Inspection { get; set; } = null!;

        [Required]
        public DateTime DueDate { get; set; }

        [Required]
        public FollowUpStatus Status { get; set; }

        public DateTime? ClosedDate { get; set; }
    }

    public enum FollowUpStatus { Open, Closed }
}
