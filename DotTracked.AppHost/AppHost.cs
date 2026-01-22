var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.DotTracked>("dottracked");

builder.Build().Run();
