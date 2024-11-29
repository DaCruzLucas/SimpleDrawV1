using SimpleDrawServer;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSignalR();
builder.Services.AddSingleton<DrawingService>(); // Enregistrer le service comme singleton

var app = builder.Build();

app.MapHub<DrawHub>("/draw");

app.Run();
