using System;
using MediatR;
using Microsoft.EntityFrameworkCore;
using VolunteerScheduler.Application.Commands.TaskCommandHandler;
using VolunteerScheduler.Application.Interfaces;
using VolunteerScheduler.Domain.Entities;
using VolunteerScheduler.Infrastructure.Data;
using VolunteerScheduler.Infrastructure.Repositories;
using VolunteerScheduler.Infrastructure.SeedData;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddScoped<ITaskRepository, TaskRepository>();
builder.Services.AddScoped<IParentRepository, ParentRepository>();
builder.Services.AddScoped<ITeacherRepository, TeacherRepository>();
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(CreateTaskCommand).Assembly));
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Volunteer Scheduler API",
        Version = "v1",
        Description = "API for managing volunteer tasks and users"
    });

    c.EnableAnnotations();
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
    DbSeeder.Seed(db);
}
app.UseStaticFiles();
app.UseSwagger(options => options.OpenApiVersion =
Microsoft.OpenApi.OpenApiSpecVersion.OpenApi2_0);
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("./v1/swagger.json", "Volunteer Scheduler API v1");
});
app.UseMiddleware<VolunteerScheduler.API.Middleware.ExceptionHandlingMiddleware>();
app.MapControllers();

app.Run();
