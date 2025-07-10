using System;
using System.Collections.Generic;
using Unity.Plastic.Newtonsoft.Json;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;

namespace AddressableGroupAssignTool.Editor
{
    [InitializeOnLoad]
    public class AddressableGroupAssignor
    {
        public static AddressableGroupAssignor Instance { get; private set; } = null;
        public AddressableGroupAssignRule RuleAsset => _ruleAsset;

        public bool IsRuleAssetImported => (bool)RuleAsset;

        public string RuleAssetPath
        {
            get => _settings.ruleAssetPath;
            set
            {
                _settings.ruleAssetPath = value;
                Settings.SaveSettings(_settings);
            }
        }

        public bool SaveGroupChangesImmediately
        {
            get => _settings.saveGroupChangeImmediately;
            set
            {
                _settings.saveGroupChangeImmediately = value;
                Settings.SaveSettings(_settings);
            }
        }

        public bool AssignGroupAutomatically
        {
            get => _settings.assignAutomatically;
            set
            {
                _settings.assignAutomatically = value;

                DescribeAssetModificationEvent();
                
                if (value)
                {
                    SubscribeAssetModificationEvent();
                }
                
                Settings.SaveSettings(_settings);
            }
        }

        private readonly AddressableSettingModificationEventHandler _modificationEventHandler;
        private readonly AssignLogBuilder _logBuilder;
        private readonly Settings _settings;
        
        private AddressableGroupAssignRule _ruleAsset;

        private AddressableGroupAssignor()
        {
            _modificationEventHandler = new AddressableSettingModificationEventHandler();
            _modificationEventHandler.OnEntryModified += AlertSameAddressAlreadyExists;

            _logBuilder = new AssignLogBuilder();
            _settings = Settings.LoadSettings();
        }
        
        [ExecuteAlways]
        static AddressableGroupAssignor()
        {
            Instance = new AddressableGroupAssignor();

            if (string.IsNullOrEmpty(Instance.RuleAssetPath))
            {
                return;
            }
            
            Instance.ImportRuleAsset(Instance.RuleAssetPath);

            if (!Instance.AssignGroupAutomatically)
            {
                return;
            }
            
            Instance.SubscribeAssetModificationEvent();
        }

        public void ImportRuleAsset(string assetPath)
        {
            _ruleAsset = AssetDatabase.LoadAssetAtPath<AddressableGroupAssignRule>(assetPath);
            // Debug.Log($"{nameof(AddressableGroupAssignor)} try import rule asset : {IsRuleAssetImported}\n from '{assetPath}'");
        }

        public void SubscribeAssetModificationEvent()
        {
            if (!AddressableAssetSettingsDefaultObject.SettingsExists)
            {
                AddressableAssetSettingsDefaultObject.GetSettings(true);
                Debug.Log($"{nameof(AddressableGroupAssignor)} : Can't find Addressables Settings asset. create new one.");
            }
            
            AddressableAssetSettingsDefaultObject.Settings.OnModification += _modificationEventHandler.HandleModification;
            _modificationEventHandler.OnEntryAdded += AssignAssetGroupWithPathAndSaveChanges;
            
            Debug.Log($"{nameof(AddressableGroupAssignor)} : Subscribe modification event");
        }

        public void DescribeAssetModificationEvent()
        {
            if (!AddressableAssetSettingsDefaultObject.SettingsExists)
            {
                return;
            }

            AddressableAssetSettingsDefaultObject.Settings.OnModification -= _modificationEventHandler.HandleModification;
            _modificationEventHandler.OnEntryAdded -= AssignAssetGroupWithPathAndSaveChanges;
            
            Debug.Log($"{nameof(AddressableGroupAssignor)} : Describe modification event");
        }

        public void AssignAssetGroupWithPath(AddressableAssetSettings settings, List<AddressableAssetEntry> entries)
        {
            int successCount = 0;
            
            foreach (var entry in entries)
            {
                if (!_ruleAsset.TryGetAssignRuleData(entry.AssetPath, out var rule))
                {
                    _logBuilder.AppendFailedResult(entry.MainAsset.name, entry.parentGroup.Name);
                    continue;
                }
                
                entry.SetAddress(rule.GetAddressableName(entry));
                settings.MoveEntry(entry, rule.targetGroup);
                
                _logBuilder.AppendSuccessResult(entry.MainAsset.name, entry.parentGroup.Name, rule.targetGroup.Name);
                successCount++;
            }
            
            Debug.Log($"AddressableGroupAssignor has assigned {successCount} of {entries.Count} assets.\n\n{_logBuilder}");
            _logBuilder.Clear();
        }

        private void AssignAssetGroupWithPathAndSaveChanges(AddressableAssetSettings settings, List<AddressableAssetEntry> entries)
        {
            AssignAssetGroupWithPath(settings, entries);

            if (!SaveGroupChangesImmediately)
            {
                return;
            }
            
            foreach (var group in settings.groups)
            {
                AssetDatabase.SaveAssetIfDirty(group);
            }
        }

        private void AlertSameAddressAlreadyExists(AddressableAssetSettings settings, AddressableAssetEntry entry)
        {
            List<AddressableAssetEntry> sameNameEntries = new();
            settings.GetAllAssets(sameNameEntries, false, entryFilter: Filter);

            if (sameNameEntries.Count == 0)
            {
                return;
            }
            
            _logBuilder.AppendLine($"Addressable Name '{entry.address}' is already exist {sameNameEntries.Count} more assets on");

            foreach (var sameNameEntry in sameNameEntries)
            {
                _logBuilder.AppendLine($"{sameNameEntry.parentGroup}");
            }

            Debug.Log(_logBuilder.ToString());
            _logBuilder.Clear();
            
            return;

            bool Filter(AddressableAssetEntry toCheck)
            {
                return toCheck != entry && toCheck.address == entry.address;
            }
        }

        public void DeleteSetting()
        {
            Settings.DeleteSettings();
        }

        [Serializable]
        private class Settings
        {
            public const string PLAYERPREFS_KEY = "AddressableGroupAssignorSettings";

            public string ruleAssetPath = "";
            public bool saveGroupChangeImmediately = true;
            public bool assignAutomatically = true;

            public static void SaveSettings(Settings settings)
            {
                string toSave = JsonConvert.SerializeObject(settings);
                
                PlayerPrefs.SetString(PLAYERPREFS_KEY, toSave);
                PlayerPrefs.Save();
            }

            public static Settings LoadSettings()
            {
                string serialized = PlayerPrefs.GetString(PLAYERPREFS_KEY, string.Empty);
                Settings settings = null;

                if (string.IsNullOrEmpty(serialized))
                {
                    settings = new Settings();
                    SaveSettings(settings);
                }
                else
                {
                    settings = JsonConvert.DeserializeObject<Settings>(serialized) ?? new Settings();
                }
                
                return settings;
            }

            public static void DeleteSettings()
            {
                PlayerPrefs.DeleteKey(PLAYERPREFS_KEY);
            }
        }
    }
}
