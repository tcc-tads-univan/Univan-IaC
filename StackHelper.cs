using Docker = Pulumi.Docker;
public class StackHelper
{
    public static Docker.RemoteImage GetDockerImage(string imageName, string name)
    {
        var dockerImage = new Docker.RemoteImage(name, new Docker.RemoteImageArgs
        {
            KeepLocally = true,
            Name = imageName
        });

        return dockerImage;
    }
}
