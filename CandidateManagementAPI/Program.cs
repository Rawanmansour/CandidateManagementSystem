using CandidateDomain.IServices;
using CandidatesApplication.Services;
using Microsoft.Extensions.Caching.Memory;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddMemoryCache();
var csvFilePath = Path.Combine(AppContext.BaseDirectory, "candidates.csv");
builder.Services.AddScoped<ICandidateService>(provider =>
{
    var cache = provider.GetRequiredService<IMemoryCache>();
    return new CandidateService(csvFilePath, cache);
});
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
