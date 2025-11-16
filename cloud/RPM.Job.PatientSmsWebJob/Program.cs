using RPM.Job.PatientSmsWebJob;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<Worker>();
builder.Services.AddHostedService<PatientSmsQueueProcessor>();

var host = builder.Build();
host.Run();
