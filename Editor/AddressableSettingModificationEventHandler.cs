using System.Collections.Generic;
using UnityEditor.AddressableAssets.Settings;

namespace AddressableGroupAssignTool.Editor
{
    public delegate void EntryAddedDelegate(AddressableAssetSettings settings, List<AddressableAssetEntry> addedEntries);

    public delegate void EntryModifiedDelegate(AddressableAssetSettings settings, AddressableAssetEntry modifiedEntry);
    
    /// <summary>
    /// A class for handle events invoked when applies or changes AddressableAssetSettings
    /// </summary>
    public sealed class AddressableSettingModificationEventHandler
    {
        public event EntryAddedDelegate OnEntryAdded = delegate { };
        public event EntryModifiedDelegate OnEntryModified = delegate { };

        public void HandleModification(AddressableAssetSettings s, AddressableAssetSettings.ModificationEvent e, object o)
        {
            // The 'object o' passed as parameter vary
            // depending on 'ModificationEvent e's Type and Methods using AddressableAssetEntry parameters...
            // 'AddressableAssetSettings.MoveEntry' cause EntryAdded,
            // but throw object as parameter that type is 'AddressableAssetEntry', not list!
            
            switch (e)
            {
                // if need to handle another ModificationEvent, add cases here
                default: return;
                
                case AddressableAssetSettings.ModificationEvent.EntryAdded:
                    if (o is not List<AddressableAssetEntry> addedEntries)
                    {
                        return;
                    }
                    
                    OnEntryAdded.Invoke(s, addedEntries);
                    break;
                
                case AddressableAssetSettings.ModificationEvent.EntryModified:
                    if (o is not AddressableAssetEntry modifiedEntry)
                    {
                        return;
                    }
                    
                    OnEntryModified.Invoke(s, modifiedEntry);
                    break;
            }
        }
    }
}
