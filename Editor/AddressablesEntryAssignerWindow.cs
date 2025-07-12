using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditorInternal;
using UnityEngine;

namespace AddressablesEntryTool.Editor
{
    public class AddressablesEntryAssignerWindow : EditorWindow
    {
        private AddressablesEntryAssigner Assignor => AddressablesEntryAssigner.Instance;
        
        private readonly string[] _ruleAssetFilters = { "Asset File", "asset" };

        private List<AssetEntryGroupPair> _assetEntriesBreakingRule; 
        private Vector2 _entireScrollPos = Vector2.zero;
        private Vector2 _listScrollPos = Vector2.zero;
        
        [MenuItem("Window/Asset Management/Addressables/Addressables Entry Tool")]
        private static void OpenWindow()
        {
            GetWindow<AddressablesEntryAssignerWindow>(nameof(AddressablesEntryAssigner));
        }

        private void OnFocus()
        {
            if (!Assignor.IsRuleAssetImported)
            {
                return;
            }
            
            _assetEntriesBreakingRule = GetAssetEntriesBreakingRules();
        }

        private void OnGUI()
        {
            _entireScrollPos = GUILayout.BeginScrollView(_entireScrollPos);
            
            DrawImportAssetMenu();
            EditorGUILayout.Space(EditorGUIUtility.singleLineHeight);

            bool isAssetImported = Assignor.IsRuleAssetImported;
            
            EditorGUI.BeginDisabledGroup(!isAssetImported);
            
            DrawSelectImportedRuleAssetMenu();
            EditorGUILayout.Space(EditorGUIUtility.singleLineHeight);
            
            DrawOptionMenu(isAssetImported);
            EditorGUILayout.Space(EditorGUIUtility.singleLineHeight);
            
            DrawAssetListBreakingRule(isAssetImported);
            
            EditorGUI.EndDisabledGroup();
            GUILayout.EndScrollView();
        }

        #region Draw Methods OnGUI

        private void DrawImportAssetMenu()
        {
            EditorGUILayout.LabelField("Path of imported asset for assign rule :", EditorStyles.boldLabel);
            EditorGUILayout.Space(EditorGUIUtility.singleLineHeight * 0.2f);

            string ruleAssetPath = Assignor.IsRuleAssetImported
                ? Assignor.RuleAssetPath
                : "Import or create RuleAsset first...";

            EditorGUILayout.LabelField(ruleAssetPath, EditorStyles.textField);
            EditorGUILayout.Space(EditorGUIUtility.singleLineHeight);

            string initialDirectory = string.IsNullOrEmpty(Assignor.RuleAssetPath)
                ? Application.dataPath
                : Assignor.RuleAssetPath;

            if (GUILayout.Button("Import RuleAsset"))
            {
                string assetPath = EditorUtility.OpenFilePanelWithFilters(
                    title: $"Please select {nameof(AddressablesEntryAssignRule)} asset",
                    directory: initialDirectory,
                    filters: _ruleAssetFilters
                );
                
                ImportRuleAssetOnAssignor(assetPath);
            }

            if (GUILayout.Button("Create new RuleAsset"))
            {
                string createPath = EditorUtility.SaveFilePanelInProject(
                    title : $"Please select path to create {nameof(AddressablesEntryAssignRule)} asset",
                    defaultName: nameof(AddressablesEntryAssignRule),
                    extension: "asset",
                    message: "What",
                    path: initialDirectory
                );

                if (string.IsNullOrEmpty(createPath))
                {
                    // Can't create asset because of path is empty when canceled SaveFilePanel
                    return;
                }

                AddressablesEntryAssignRule toCreate = CreateInstance<AddressablesEntryAssignRule>();
                
                AssetDatabase.CreateAsset(toCreate, createPath);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                
                ImportRuleAssetOnAssignor(createPath);
                Selection.activeObject = toCreate;
            }
        }

        private void DrawSelectImportedRuleAssetMenu()
        {
            if (GUILayout.Button("Select imported RuleAsset"))
            {
                Selection.activeObject = Assignor.RuleAsset;
            }
        }

        private void DrawOptionMenu(bool assetImported)
        {
            if (!assetImported)
            {
                return;
            }
            
            EditorGUILayout.LabelField("Addressable Group Assignor Options", EditorStyles.boldLabel);
            EditorGUILayout.Space(EditorGUIUtility.singleLineHeight * 0.2f);

            bool saveGroupChangesImmediately = GUILayout.Toggle(
                Assignor.SaveGroupChangesImmediately,
                "Save changes of addressable assets immediately after assign group"
            );

            ApplySaveGroupChangesImmediatelyToggleOption(saveGroupChangesImmediately);

            bool assignGroupAutomatically = GUILayout.Toggle(
                Assignor.AssignGroupAutomatically,
                "Assign newly entry assets according to rule automatically"
            );
            
            ApplyAssignGroupAutomaticallyToggleOption(assignGroupAutomatically);

            if (EditorGUILayout.BeginFadeGroup(Assignor.AssignGroupAutomatically ? 1.0f : 0.0f))
            {
                string warningMessage = "This feature will apply to all newly entries of addressable assets while active.";
                
                EditorGUILayout.Space(1.0f);
                EditorGUILayout.HelpBox(warningMessage, MessageType.Warning);
            }
            
            EditorGUILayout.EndFadeGroup();
        }

        private void DrawAssetListBreakingRule(bool assetImported)
        {
            if (!assetImported)
            {
                return;
            }
            
            EditorGUILayout.LabelField("Addressable asset list breaking rule", EditorStyles.boldLabel);
            EditorGUILayout.Space(EditorGUIUtility.singleLineHeight * 0.2f);

            ReorderableList breakingRuleList = new ReorderableList(
                elements: _assetEntriesBreakingRule,
                elementType: typeof(AssetEntryGroupPair),
                draggable: false,
                displayHeader: false,
                displayAddButton: false,
                displayRemoveButton: false
            )
            {
                elementHeight = EditorGUIUtility.singleLineHeight * 3.0f,
                drawElementCallback = DrawElementOfAssetEntriesBreakingRule
            };

            _listScrollPos = EditorGUILayout.BeginScrollView(_listScrollPos, GUILayout.MaxHeight(500.0f));
            breakingRuleList.DoLayoutList();
            EditorGUILayout.EndScrollView();

            if (_assetEntriesBreakingRule == null || _assetEntriesBreakingRule.Count == 0)
            {
                return;
            }
            
            EditorGUILayout.Space(EditorGUIUtility.singleLineHeight * 0.5f);

            if (GUILayout.Button("Re-Assign all assets in list"))
            {
                AssignAssetsGroupDirect(_assetEntriesBreakingRule.Select(element => element.AssetEntry).ToList());
            }
        }

        private void DrawElementOfAssetEntriesBreakingRule(Rect rect, int index, bool isActive, bool isFocused)
        {
            var element = _assetEntriesBreakingRule[index];

            Rect buttonPosition = rect;
            buttonPosition.width = 60.0f;
            buttonPosition.height = EditorGUIUtility.singleLineHeight;
            buttonPosition.y = rect.y + EditorGUIUtility.singleLineHeight;

            Rect assetNamePosition = rect;
            assetNamePosition.width = rect.width - buttonPosition.width;
            assetNamePosition.height = EditorGUIUtility.singleLineHeight;

            Rect addressPosition = assetNamePosition;
            addressPosition.y += EditorGUIUtility.singleLineHeight;

            Rect groupNamePosition = addressPosition;
            groupNamePosition.y += EditorGUIUtility.singleLineHeight;

            buttonPosition.x = rect.x + assetNamePosition.width;
            rect.y = EditorGUIUtility.singleLineHeight * 2.0f;
            
            EditorGUI.LabelField(assetNamePosition, element.AssetName, EditorStyles.boldLabel);
            EditorGUI.LabelField(addressPosition,
                element.IsBreakingAddressRule
                    ? $"Address : {element.CurrentAddress}  ->  {element.ToChangeAddress}"
                    : "Address : No need to change",
                EditorStyles.miniLabel
            );
            EditorGUI.LabelField(
                groupNamePosition,
                element.IsBreakingGroupRule
                    ? $"Group : {element.CurrentGroupName}    ->  {element.ToAssignGroupName}"
                    : "Group : No need to change",
                EditorStyles.miniLabel
            );

            if (EditorGUI.LinkButton(buttonPosition, "Re-assign"))
            {
                AssignAssetsGroupDirect(new List<AddressableAssetEntry> { element.AssetEntry });
            }
        }

        #endregion

        private void ImportRuleAssetOnAssignor(string assetPath)
        {
            string relativePath = (assetPath.StartsWith(Application.dataPath)
                    ? $"Assets/{Path.GetRelativePath(Application.dataPath, assetPath)}"
                    : assetPath)
                .Replace(
                    Path.DirectorySeparatorChar,
                    Path.AltDirectorySeparatorChar
                );
            
            Assignor.ImportRuleAsset(relativePath);
            
            if (!Assignor.IsRuleAssetImported)
            {
                return;
            }
            
            Assignor.RuleAssetPath = relativePath;
            Debug.Log($"{nameof(AddressablesEntryAssigner)} imported rule asset from '{relativePath}'");
        }

        private void ApplySaveGroupChangesImmediatelyToggleOption(bool toggle)
        {
            if (toggle != Assignor.SaveGroupChangesImmediately)
            {
                Assignor.SaveGroupChangesImmediately = toggle;
            }
        }

        private void ApplyAssignGroupAutomaticallyToggleOption(bool toggle)
        {
            if (toggle != Assignor.AssignGroupAutomatically)
            {
                Assignor.AssignGroupAutomatically = toggle;
            }
        }

        private List<AssetEntryGroupPair> GetAssetEntriesBreakingRules()
        {
            List<AddressableAssetEntry> allAssets = new List<AddressableAssetEntry>();

            if (AddressableAssetSettingsDefaultObject.SettingsExists)
            {
                AddressableAssetSettingsDefaultObject.Settings.GetAllAssets(allAssets, false);
            }
            
            List<AssetEntryGroupPair> assetListBreakingRules = new List<AssetEntryGroupPair>();
            
            foreach (var assetEntry in allAssets)
            {
                if (!Assignor.RuleAsset.TryGetAssignRuleData(assetEntry.AssetPath, out var ruleData))
                {
                    continue;
                }

                var pair = new AssetEntryGroupPair(assetEntry, ruleData);
                if (!pair.IsBreakingAddressRule && !pair.IsBreakingGroupRule)
                {
                    continue;
                }
                
                assetListBreakingRules.Add(pair);
            }

            return assetListBreakingRules;
        }

        private void AssignAssetsGroupDirect(List<AddressableAssetEntry> entries)
        {
            Assignor.AssignAssetGroupWithPath(AddressableAssetSettingsDefaultObject.Settings, entries);
            _assetEntriesBreakingRule = GetAssetEntriesBreakingRules();

            if (!Assignor.SaveGroupChangesImmediately)
            {
                return;
            }

            foreach (var group in AddressableAssetSettingsDefaultObject.Settings.groups)
            {
                AssetDatabase.SaveAssetIfDirty(group);
            }
        }

        private class AssetEntryGroupPair
        {
            public readonly AddressableAssetEntry AssetEntry;
            
            private readonly AddressableAssetGroup _groupToAssign;
            private readonly string _addressToChange;

            public bool IsBreakingAddressRule => AssetEntry.address != _addressToChange;
            public bool IsBreakingGroupRule => AssetEntry.parentGroup != _groupToAssign;

            public string AssetName => Path.GetFileName(AssetEntry.AssetPath);
            public string CurrentAddress => AssetEntry?.address ?? string.Empty;
            public string ToChangeAddress => _addressToChange;
            public string CurrentGroupName => AssetEntry.parentGroup?.Name ?? string.Empty;
            public string ToAssignGroupName => _groupToAssign?.Name ?? string.Empty;

            public AssetEntryGroupPair(AddressableAssetEntry assetEntry, AssignRuleData ruleData)
            {
                AssetEntry = assetEntry;

                _groupToAssign = ruleData.targetGroup;
                _addressToChange = ruleData.GetAddressableName(assetEntry);
            }
        }
    }

}