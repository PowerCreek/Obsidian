namespace Obsidian.PrimaryServer.Server;

public record struct RunnerMetaData(
    string ServerVersion,
    int ServerPort
    )
{
    public static readonly RunnerMetaData SampleMetaData = new
        (
            ServerVersion: "A",
            ServerPort: 12994
        );
}
