﻿using DotCast.SharedKernel.Messages;
using DotCast.Storage.Storage;

namespace DotCast.Storage.Handlers
{
    public class AudioBookMetadataRequestHandler(IStorage storage) : MessageHandler<AudioBookMetadataRequest>
    {
        public override async Task Handle(AudioBookMetadataRequest message)
        {
            await storage.ExtractMetadataAsync(message.Id);
        }
    }
}