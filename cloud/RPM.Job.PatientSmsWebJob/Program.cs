using RPM.Job.PatientSmsWebJob;


var builder = Host.CreateApplicationBuilder(args);
var config = builder.Configuration;
var processor = new PatientSmsQueueProcessor(config);
await processor.RunOnceAsync();
