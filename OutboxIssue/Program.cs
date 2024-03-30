var builder = WebApplication.CreateBuilder(args);

// All configuration done in CustomWebApplicationFactory.ConfigureWebHost

var app = builder.Build();

app.Run();

public partial class Program { }