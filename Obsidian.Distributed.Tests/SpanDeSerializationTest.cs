using Obsidian.Distributed.Backend.World;

namespace Obsidian.Distributed.Tests;

public class SpanDeSerializationTest
{
    [Fact(DisplayName = "Serial/Deserial")]
    public void GetBytes_SerializeAndDeserialize_ReturnsEqualObject()
    {
        var command = new ServerWorldCommand { WorldValue = 42 };

        byte[] serializedData = command.Serialize();
        ServerWorldCommand deserializedCommand = serializedData.Deserialize<ServerWorldCommand>();

        Assert.Equal(command.WorldValue, deserializedCommand.WorldValue);
    }
}
