global using ClientMapKey = string;
global using ClientMapType = System.Collections.Concurrent.ConcurrentDictionary<string, (Obsidian.Stripped.Host.IClientInstance, Obsidian.Stripped.EventPackets.Channels.ChannelOperator<object>)>;
global using ClientMapValue = (Obsidian.Stripped.Host.IClientInstance Instance, Obsidian.Stripped.EventPackets.Channels.ChannelOperator<object> Channel);
global using ChannelOperatorImpl = Obsidian.Stripped.EventPackets.Channels.ChannelOperator<object>;
global using Obsidian.Stripped.BusinessLogic.Run;
