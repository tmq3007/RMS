
using RMS.Services.AI;
using RMS.Services.AI.NSFW;
using RMS.Services.AI.Service;

var builder = WebApplication.CreateBuilder(args);
// Đăng ký Singleton để chỉ load model nặng (343MB) 1 lần duy nhất lúc khởi động
builder.Services.AddSingleton<NsfwOfflineService>();
// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
string apiKey = "AIzaSyCGc4Y2nzAArfyLAyzd1Kp1xRMmktErAqw";
builder.Services.AddSingleton(new GeminiApiClient(apiKey));
builder.Services.AddScoped<SchedulePredictor>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors();
var app = builder.Build();
app.UseCors(option => option
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader()
            .WithExposedHeaders("Content-Dispostion"));
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();