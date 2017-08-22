﻿using System;

namespace Octopus.Shared.Configuration
{
    public class JsonConsoleKeyValueStore : JsonFlatKeyValueStore
    {
        public JsonConsoleKeyValueStore() : base(autoSaveOnSet: false, isWriteOnly: true)
        {
        }

        protected override void WriteSerializedData(string serializedData)
        {
            Console.WriteLine(serializedData);
        }
    }
}
