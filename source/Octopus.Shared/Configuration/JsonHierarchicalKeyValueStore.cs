﻿using System.Collections.Generic;
using Newtonsoft.Json;

namespace Octopus.Shared.Configuration
{
    public abstract class JsonHierarchicalKeyValueStore : NestedDictionaryKeyValueStore
    {
        protected JsonHierarchicalKeyValueStore(bool autoSaveOnSet, bool isWriteOnly = false) : base(autoSaveOnSet, isWriteOnly)
        {
        }

        protected override void SaveSettings(IDictionary<string, object> settingsToSave)
        {
            var data = new ObjectHierarchy();
            foreach (var kvp in settingsToSave)
            {
                var keyHierarchyItems = kvp.Key.Split('.');

                var node = data;
                for (var i = 0; i < keyHierarchyItems.Length; i++)
                {
                    var keyHierarchyItem = keyHierarchyItems[i];

                    if (node.ContainsKey(keyHierarchyItem))
                    {
                        node = (ObjectHierarchy)node[keyHierarchyItem];
                    }
                    else
                    {
                        if (i == keyHierarchyItems.Length - 1)
                        {
                            node.Add(keyHierarchyItem, kvp.Value);
                        }
                        else
                        {
                            var newNode = new ObjectHierarchy();
                            node.Add(keyHierarchyItem, newNode);
                            node = newNode;
                        }
                    }
                }
            }

            var serializedData = JsonConvert.SerializeObject(data, Formatting.Indented);
            WriteSerializedData(serializedData);
        }

        protected abstract void WriteSerializedData(string serializedData);
    }

    public class ObjectHierarchy : Dictionary<string, object>
    {
    }
}
