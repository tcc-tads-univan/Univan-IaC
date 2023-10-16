using System;
using Pulumi;
using Docker = Pulumi.Docker;

return await Deployment.RunAsync(() =>
{
   var configs = new Config("univan");

   var rabbitMQImageName = configs.Require("rabbitMQ-image");
   var msHistoryImage = configs.Require("ms-history-image");

   var dbUser = configs.Require("dbUser");
   var dbPassword = configs.RequireSecret("dbPassword");

   var network = new Docker.Network("network-univan");

   CreateRabbitMQContainer(rabbitMQImageName, network);
   CreateHistoryMicroservice(msHistoryImage, network, dbUser,   dbPassword);
});


static void CreateRabbitMQContainer(string rabbitMQImageName, Docker.Network network)
{
   var rabbitMQImage = StackHelper.GetDockerImage(rabbitMQImageName, "RabbitMQ");

   var rabbitMQContainer = new Docker.Container("RabbitMQContainer", new Docker.ContainerArgs
   {
      Name = "rabbit-service",
      Image = rabbitMQImage.Latest,
      Ports = new InputList<Docker.Inputs.ContainerPortArgs>{
         new Docker.Inputs.ContainerPortArgs{
            Internal = 15672,
            External = 15672
         },
         new Docker.Inputs.ContainerPortArgs{
            Internal = 5672,
            External = 5672
         }
      },
      NetworksAdvanced = new InputList<Docker.Inputs.ContainerNetworksAdvancedArgs>{
         new Docker.Inputs.ContainerNetworksAdvancedArgs{
            Name = network.Name
         }
      }
   }, new CustomResourceOptions
   {
      DependsOn = rabbitMQImage
   });
}

static void CreateHistoryMicroservice(string msHistoryImage, Docker.Network network, string dbUser, Output<string> dbPassword)
{
   var msHistory = StackHelper.GetDockerImage(msHistoryImage, "univan-history");

   var historyApp = new Docker.Container("MS-History", new Docker.ContainerArgs
   {
      Name = "univanHistory",
      Image = msHistory.Latest,
      Ports = new InputList<Docker.Inputs.ContainerPortArgs> {
         new Docker.Inputs.ContainerPortArgs
         {
            Internal = 80,
            External = 5003
         }
      },
      Envs = new InputList<string> {
         dbPassword.Apply(password => $"ConnectionStrings__HistoryDatabase=Server=tcp:tccunivanfinal.database.windows.net,1433;Initial Catalog=history;Persist Security Info=False;User ID={dbUser};Password={password};MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"),
         $"ConnectionStrings__RabbitMq=amqp://guest:guest@rabbit-service:5672"
      },
      NetworksAdvanced = new InputList<Docker.Inputs.ContainerNetworksAdvancedArgs> {
         new Docker.Inputs.ContainerNetworksAdvancedArgs
         {
            Name = network.Name
         }
      }
   });
}