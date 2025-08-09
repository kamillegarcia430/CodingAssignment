namespace VolunteerScheduler.Domain.Entities
{
    public class Teacher
    {
        public int TeacherId { get; set; }
        public string Name { get; set; }

        public ICollection<VolunteerTask> CreatedTasks { get; set; } = new List<VolunteerTask>();
    }
}
