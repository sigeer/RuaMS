var builder = DistributedApplication.CreateBuilder(args);

// 可以生成docker-compose.yml 但是发布环境下通过docker-compose没有意义
builder.AddDockerComposeEnvironment("compose")
    .WithDashboard(dashboard =>
    {
        dashboard.WithHostPort(7979)
                 .WithForwardedHeaders(enabled: true);
    });

var masterServerNode = builder.AddProject<Projects.Application_Host>("ruams-master").WithArgs("/p:IsStandalone=false")
    .PublishAsDockerComposeService((resource, service) =>
    {
        service.Name = "ruams-master";
        service.Image = "ghcr.io/sigeer/ruams-master:latest";
        service.Expose = ["7878:7878", "8484:8484", "7575-7600:7575-7600"];
        service.Volumes = new List<Aspire.Hosting.Docker.Resources.ServiceNodes.Volume>()
        {
            new Aspire.Hosting.Docker.Resources.ServiceNodes.Volume(){ Name = "Log", Source ="./logs", Target = "/app/logs"}
        };
    });

//var channelServerNode = builder.AddProject<Projects.Application_Host_Channel>("ruams-channel")
//    .WithReference(masterServerNode)
//    .WaitFor(masterServerNode)
//    .PublishAsDockerComposeService((resource, service) =>
//    {
//        service.Name = "ruams-channel";
//        service.Image = "ghcr.io/sigeer/ruams-channel:latest";
//        service.Expose = ["7879:7879", "7575-7600:7575-7600"];
//        service.Volumes = new List<Aspire.Hosting.Docker.Resources.ServiceNodes.Volume>()
//        {
//            new Aspire.Hosting.Docker.Resources.ServiceNodes.Volume(){ Name = "Log", Source ="./logs", Target = "/app/logs"}
//        };
//    });



builder.Build().Run();
