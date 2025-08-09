using VolunteerScheduler.Domain.Entities;
using VolunteerScheduler.Infrastructure.Data;

namespace VolunteerScheduler.Infrastructure.SeedData
{
    public static class DbSeeder
    {
        public static void Seed(AppDbContext db)
        {
            // Seed Parents
            if (!db.Parents.Any())
            {
                db.Parents.AddRange(
                    new Parent { Name = "Alice Smith" },
                    new Parent { Name = "Bob Johnson" }
                );
                db.SaveChanges();
            }

            // Seed Teachers
            if (!db.Teachers.Any())
            {
                db.Teachers.AddRange(
                    new Teacher { Name = "Ms. Lopez" },
                    new Teacher { Name = "Mr. Tan" }
                );
                db.SaveChanges();
            }

            if (!db.VolunteerTasks.Any())
            {
                var msLopez = db.Teachers.First(t => t.Name == "Ms. Lopez");
                var mrTan = db.Teachers.First(t => t.Name == "Mr. Tan");

                var readingHelperTask = new VolunteerTask
                {
                    Title = "Reading Helper",
                    Start = DateTime.UtcNow.AddDays(1),
                    End = DateTime.UtcNow.AddDays(1).AddHours(1),
                    NumberOfAvailableSlots = 5,
                    CreatedByTeacherId = msLopez.TeacherId,
                };

                var mathAssistantTask = new VolunteerTask
                {
                    Title = "Math Assistant",
                    Start = DateTime.UtcNow.AddDays(2),
                    End = DateTime.UtcNow.AddDays(2).AddHours(1),
                    NumberOfAvailableSlots = 10,
                    CreatedByTeacherId = mrTan.TeacherId,
                };

                db.VolunteerTasks.AddRange(readingHelperTask, mathAssistantTask);

                msLopez.CreatedTasks.Add(readingHelperTask);
                mrTan.CreatedTasks.Add(mathAssistantTask);

                db.SaveChanges();
            }
        }
    }
}
