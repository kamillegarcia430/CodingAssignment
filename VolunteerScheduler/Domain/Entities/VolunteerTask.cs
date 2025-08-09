using System.ComponentModel.DataAnnotations;

namespace VolunteerScheduler.Domain.Entities
{
    public class VolunteerTask
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public int NumberOfAvailableSlots { get; set; }
        // Many-to-many with Parents
        public List<int> ParticipatingParents { get; set; } = new();

        // Many-to-one with Teacher
        [Required]
        public int CreatedByTeacherId { get; set; }

        [Timestamp]
        public byte[]? RowVersion { get; set; }
    }
}
