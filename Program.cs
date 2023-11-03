using System.Collections.Generic;
using Pulumi;
using Docker = Pulumi.Docker;

return await Deployment.RunAsync(() =>
{
   var configs = new Config("univan");

   var rabbitMQImageName = configs.Require("rabbitMQ-image");
   var msHistoryImage = configs.Require("ms-history-image");
   var msUnivanImage = configs.Require("ms-univan-image");
   var msCarpoolImage = configs.Require("ms-carpool-image");
   var msNotificationImage = configs.Require("ms-notification-image");

   var dbUser = configs.Require("dbUser");
   var dbPassword = configs.RequireSecret("dbPassword");
   var blobPassword = configs.RequireSecret("blobPassword");
   var redisPassword = configs.RequireSecret("redisPassword");
   var emailKey = configs.RequireSecret("emailKey");

   var network = new Docker.Network("network-univan");

   CreateRabbitMQContainer(rabbitMQImageName, network);
   CreateHistoryMicroservice(msHistoryImage, network, dbUser,   dbPassword);
   CreateUnivanMicroservice(msUnivanImage, network, dbUser,   dbPassword, blobPassword);
   CreateCarpoolMicroservice(msCarpoolImage, network, dbUser,   dbPassword,redisPassword);
   CreateNotificationMicroservice(msNotificationImage, network, dbUser,   dbPassword, emailKey);
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

static void CreateUnivanMicroservice(string msUnivanImage, Docker.Network network, string dbUser, Output<string> dbPassword, Output<string> blobPassword)
{
   var msUnivan = StackHelper.GetDockerImage(msUnivanImage, "univan-core");

   var univanApp = new Docker.Container("MS-Univan", new Docker.ContainerArgs
   {
      Name = "univanCore",
      Image = msUnivan.Latest,
      Ports = new InputList<Docker.Inputs.ContainerPortArgs> {
         new Docker.Inputs.ContainerPortArgs
         {
            Internal = 80,
            External = 5000
         }
      },
      Envs = new InputList<string> {
         dbPassword.Apply(password => $"ConnectionStrings__UnivanDatabase=Server=tcp:tccunivanfinal.database.windows.net,1433;Initial Catalog=univan;Persist Security Info=False;User ID={dbUser};Password={password};MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"),
         $"ConnectionStrings__RabbitMq=amqp://guest:guest@rabbit-service:5672",
         blobPassword.Apply(password => $"BlobSettings__ConnectionString=DefaultEndpointsProtocol=https;AccountName=tccunivan;AccountKey={password};EndpointSuffix=core.windows.net")
      },
      NetworksAdvanced = new InputList<Docker.Inputs.ContainerNetworksAdvancedArgs> {
         new Docker.Inputs.ContainerNetworksAdvancedArgs
         {
            Name = network.Name
         }
      }
   });
}

static void CreateCarpoolMicroservice(string msCarpoolImage, Docker.Network network, string dbUser, Output<string> dbPassword, Output<string> redisPassword)
{
   var msCarpool = StackHelper.GetDockerImage(msCarpoolImage, "univan-carpool");

   var carpoolApp = new Docker.Container("MS-Carpool", new Docker.ContainerArgs
   {
      Name = "univanCarpool",
      Image = msCarpool.Latest,
      Ports = new InputList<Docker.Inputs.ContainerPortArgs> {
         new Docker.Inputs.ContainerPortArgs
         {
            Internal = 80,
            External = 5001
         }
      },
      Envs = new InputList<string> {
         dbPassword.Apply(password => $"ConnectionStrings__CarpoolDatabase=Server=tcp:tccunivanfinal.database.windows.net,1433;Initial Catalog=carpool-service;Persist Security Info=False;User ID={dbUser};Password={password};MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"),
         $"ConnectionStrings__RabbitMq=amqp://guest:guest@rabbit-service:5672",
         $"UnivanService__Url=http://univanCore/api/",
         redisPassword.Apply(password => $"ConnectionStrings__Redis=tccunivan.redis.cache.windows.net:6380,password={password},ssl=True,abortConnect=False")
      },
      NetworksAdvanced = new InputList<Docker.Inputs.ContainerNetworksAdvancedArgs> {
         new Docker.Inputs.ContainerNetworksAdvancedArgs
         {
            Name = network.Name
         }
      }
   });
}

static void CreateNotificationMicroservice(string msNotificationImage, Docker.Network network, string dbUser, Output<string> dbPassword,Output<string> emailKey)
{
   var msNotification = StackHelper.GetDockerImage(msNotificationImage, "univan-notification");

   var notificationApp = new Docker.Container("MS-Notification", new Docker.ContainerArgs
   {
      Name = "univanNotification",
      Image = msNotification.Latest,
      Ports = new InputList<Docker.Inputs.ContainerPortArgs> {
         new Docker.Inputs.ContainerPortArgs
         {
            Internal = 80,
            External = 5002
         }
      },
      Envs = new InputList<string> {
         dbPassword.Apply(password => $"ConnectionStrings__HistoryDatabase=Server=tcp:tccunivanfinal.database.windows.net,1433;Initial Catalog=history;Persist Security Info=False;User ID={dbUser};Password={password};MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"),
         $"ConnectionStrings__RabbitMq=amqp://guest:guest@rabbit-service:5672",
         emailKey.Apply(key => $"EmailSettings__ApiKey={key}")
      },
      NetworksAdvanced = new InputList<Docker.Inputs.ContainerNetworksAdvancedArgs> {
         new Docker.Inputs.ContainerNetworksAdvancedArgs
         {
            Name = network.Name
         }
      }
   });
}