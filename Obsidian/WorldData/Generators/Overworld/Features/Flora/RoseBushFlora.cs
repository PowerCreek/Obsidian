﻿using Obsidian.API.BlockStates.Builders;

namespace Obsidian.WorldData.Generators.Overworld.Features.Flora;

public class RoseBushFlora : BaseTallFlora
{
    public RoseBushFlora(GenHelper helper, Chunk chunk) : 
        base(helper, chunk, Material.RoseBush, 2, new RoseBushStateBuilder().WithHalf(BlockHalf.Lower).Build(), new RoseBushStateBuilder().WithHalf(BlockHalf.Upper).Build())
    {

    }
}
