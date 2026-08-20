using CisCompliance.Core.Abstractions;
using CisCompliance.Core.Classification;
using CisCompliance.Core.Extraction;
using CisCompliance.Infrastructure;
using CisCompliance.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCisComplianceInfrastructure(builder.Configuration, builder.Environment.ContentRootPath);
builder.Services.AddScoped<IClassificationService, ClassificationService>();
builder.Services.AddScoped<IInvoiceExtractionService, InvoiceExtractionService>();

const string AngularDevCorsPolicy = "AngularDev";
builder.Services.AddCors(options =>
{
    options.AddPolicy(AngularDevCorsPolicy, policy =>
    {
        policy.WithOrigins("http://localhost:4200").AllowAnyHeader().AllowAnyMethod();
    });
});

var app = builder.Build();

// Applies pending migrations and seeds the 8 real POC invoices on first run,
// so a fresh checkout is fully populated with nothing more than `dotnet run`,
// no manual migration step required.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<CisComplianceDbContext>();
    await db.Database.MigrateAsync();
    await CisComplianceDbSeeder.SeedIfEmptyAsync(db, scope.ServiceProvider.GetRequiredService<IRulesRepository>());
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseCors(AngularDevCorsPolicy);
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
