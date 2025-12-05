var builder = DistributedApplication.CreateBuilder(args);

builder.AddDockerComposeEnvironment("compose")
    .WithDashboard(dashboard =>
    {
        dashboard.WithHostPort(7979)
                 .WithForwardedHeaders(enabled: true);
    });

var masterServerNode = builder.AddProject<Projects.Application_Host>("ruams-master").WithArgs("/p:IsStandalone=false")
    .WithHttpEndpoint(7878, name: "Grpc")
    .PublishAsDockerComposeService((resource, service) =>
    {
        service.Name = "ruams-master";
        service.Image = "ghcr.io/sigeer/ruams-master:latest";
        service.Expose = ["8484", "7575-7600"];
        service.Volumes = new List<Aspire.Hosting.Docker.Resources.ServiceNodes.Volume>()
        {
            new Aspire.Hosting.Docker.Resources.ServiceNodes.Volume(){ Name = "Log", Source ="./logs", Target = "/app/logs"}
        };
    });


var channelServerNode = builder.AddProject<Projects.Application_Host_Channel>("ruams-channel")
    .WithHttpEndpoint(7879, name: "Grpc")
    .WithReference(masterServerNode)
    .WaitFor(masterServerNode)
    .PublishAsDockerComposeService((resource, service) =>
    {
        service.Name = "ruams-channel";
        service.Image = "ghcr.io/sigeer/ruams-channel:latest";
        service.Expose = ["7575-7600"];
        service.Volumes = new List<Aspire.Hosting.Docker.Resources.ServiceNodes.Volume>()
        {
            new Aspire.Hosting.Docker.Resources.ServiceNodes.Volume(){ Name = "Log", Source ="./logs", Target = "/app/logs"}
        };
    });

builder.Build().Run();
