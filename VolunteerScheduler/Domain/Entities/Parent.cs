namespace VolunteerScheduler.Domain.Entities
{
    public class Parent
    {
        public int ParentId { get; set; }
        public string Name { get; set; }

        // Navigation property for claimed tasks
        public ICollection<VolunteerTask> ClaimedTasks { get; set; } = new List<VolunteerTask>();
    }
}
