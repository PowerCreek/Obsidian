﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Obsidian.Net;

namespace Obsidian.Commands
{
    [Obsolete("Note: this should only be used as a workaround until specific code has been written for the parser")]
    public class EmptyFlagsCommandParser : CommandParser
    {
        public EmptyFlagsCommandParser(string identifier) : base(identifier)
        {
        }

        public override async Task WriteAsync(MinecraftStream stream)
        {
            await base.WriteAsync(stream);
            await stream.WriteByteAsync(0);
        }
    }
}