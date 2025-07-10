using System;
using UnityEditor;
using UnityEngine;

namespace AddressableGroupAssignTool.Editor
{
    public class AddressableGroupAssignRule : ScriptableObject
    {
        [SerializeField] private AssignRuleData[] assignGroupRules;

        public bool TryGetAssignRuleData(string assetPath, out AssignRuleData ruleData)
        {
            ruleData = Array.Find(assignGroupRules, Predicate);

            return ruleData != null;

            bool Predicate(AssignRuleData ruleData)
            {
                return ruleData.IsPathIncluding(assetPath);
            }
        }
    }

    public class GroupAssignRuleDeleteProcessor : AssetModificationProcessor
    {
        public static AssetDeleteResult OnWillDeleteAsset(string assetPath, RemoveAssetOptions options)
        {
            if (assetPath == AddressableGroupAssignor.Instance.RuleAssetPath)
            {
                AddressableGroupAssignor.Instance.DeleteSetting();
            }

            return AssetDeleteResult.DidNotDelete;
        }
    }

    [CustomEditor(typeof(AddressableGroupAssignRule))]
    public class AddressableGroupAssignRuleEditor : UnityEditor.Editor
    {
        private readonly string _assetNameRuleMessage = "Available Tags for AssetNameRule : \n\n" +
                                                        $"{PathTags.FULL_PATH} : full path of addressable, include asset's name and extension.\n" +
                                                        $"{PathTags.NAME} : name of asset. (not include extension)\n" +
                                                        $"{PathTags.EXTENSION} : extension of asset.\n" +
                                                        $"{PathTags.LOWER} : make addressable name to lowercase.\n" +
                                                        $"{PathTags.NO_SPACE} : remove all white spaces";
        
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            
            EditorGUILayout.Space(EditorGUIUtility.singleLineHeight);
            EditorGUILayout.HelpBox(_assetNameRuleMessage, MessageType.Info);
        }
    }
}
