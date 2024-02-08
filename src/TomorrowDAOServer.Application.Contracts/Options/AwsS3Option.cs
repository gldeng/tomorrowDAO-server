namespace TomorrowDAOServer.Options;

public class AwsS3Option
{
    
    public string SecurityKeyId { get; set; }
    public string BucketName { get; set; }
    public string Path { get; set; }

    public string RegionEndpoint { get; set; } = "ap-northeast-1";

}