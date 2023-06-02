global using ClientMapKey = string;
global using ClientMapValue = (Obsidian.Stripped.Host.IClientInstance Instance, Obsidian.Stripped.EventPackets.PacketDataConsumer PacketConsumer);
global using ClientMapType = System.Collections.Concurrent.ConcurrentDictionary<string, (Obsidian.Stripped.Host.IClientInstance, Obsidian.Stripped.EventPackets.PacketDataConsumer)>;
