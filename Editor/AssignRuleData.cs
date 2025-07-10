using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;

namespace AddressablesEntryTool.Editor
{
    public static class PathTags
    {
        public const string FULL_PATH = "[fullpath]";
        public const string NAME = "[name]";
        public const string EXTENSION = "[extension]";
        public const string LOWER = "[lower]";
        public const string NO_SPACE = "[nospace]";

        public const string FULL_PATH_PATTERN = @"\[fullpath\]";
        public const string NAME_PATTERN = @"\[name\]";
        public const string EXTENSION_PATTERN = @"\[extension\]";
        public const string LOWER_PATTERN = @"\[lower\]";
        public const string NO_SPACE_PATTERN = @"\[nospace\]";
    }
    
    [Serializable]
    public class AssignRuleData
    {
        public AddressableAssetGroup targetGroup;
        public List<string> targetPaths;
        public string assetNameRule;

        public bool IsPathIncluding(string assetPath)
        {
            string replacedPath = assetPath.Replace(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            
            return targetPaths.Exists(Predicate);
            
            bool Predicate(string path)
            {
                return replacedPath.StartsWith(path, StringComparison.CurrentCultureIgnoreCase);
            }
        }

        public string GetAddressableName(AddressableAssetEntry entry)
        {
            string assetName = assetNameRule;
            
            if (Regex.IsMatch(assetName, PathTags.FULL_PATH_PATTERN))
            {
                assetName = ReplaceFullPathTag(assetName, entry);
            }

            if (Regex.IsMatch(assetName, PathTags.NAME_PATTERN))
            {
                assetName = ReplaceNameTag(assetName, entry);
            }

            if (Regex.IsMatch(assetName, PathTags.EXTENSION_PATTERN))
            {
                assetName = ReplaceExtensionTag(assetName, entry);
            }

            if (Regex.IsMatch(assetName, PathTags.LOWER_PATTERN))
            {
                assetName = ToLowercase(assetName);
            }

            if (Regex.IsMatch(assetName, PathTags.NO_SPACE_PATTERN))
            {
                assetName = RemoveAllSpaces(assetName);
            }

            return assetName;
        }

        #region Methods for modify addressable name with tags

        private static string ReplaceFullPathTag(string name, AddressableAssetEntry entry)
        {
            return Regex.Replace(name, PathTags.FULL_PATH_PATTERN, entry.AssetPath);
        }

        private static string ReplaceNameTag(string name, AddressableAssetEntry entry)
        {
            string fileName = Path.GetFileNameWithoutExtension(entry.AssetPath);

            return Regex.Replace(name, PathTags.NAME_PATTERN, fileName);
        }

        private static string ReplaceExtensionTag(string name, AddressableAssetEntry entry)
        {
            string extension = Path.GetExtension(entry.AssetPath);

            return Regex.Replace(name, PathTags.EXTENSION_PATTERN, extension);
        }

        private static string ToLowercase(string name)
        {
            return Regex.Replace(name, PathTags.LOWER_PATTERN, string.Empty).ToLowerInvariant();
        }

        private static string RemoveAllSpaces(string name)
        {
            return Regex.Replace(name, @"\s", string.Empty);
        }

        #endregion
    }
    
    [CustomPropertyDrawer(typeof(AssignRuleData))]
    public class AssignRuleDataDrawer : PropertyDrawer
    {
        private readonly string[] _propertyNames = new[]
        {
            "targetGroup",
            "targetPaths",
            "assetNameRule"
        };
        
        private readonly float _singleLineHeight = EditorGUIUtility.singleLineHeight;
        private readonly float _propertySpace = 3.0f;
        private readonly float _arrayPropertyAdditiveHeight = EditorGUIUtility.singleLineHeight * 3;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            float accumulatedHeight = 0.0f;
        
            #region draw label
        
            Rect labelRect = GetPropertyRect(position, accumulatedHeight);
            EditorGUI.LabelField(labelRect, label);

            accumulatedHeight = labelRect.height + _propertySpace;
        
            #endregion
        
            #region draw properties
        
            foreach (var propertyName in _propertyNames)
            {
                SerializedProperty targetProperty = property.FindPropertyRelative(propertyName);
                
                Rect propertyRect = GetPropertyRect(position, accumulatedHeight, targetProperty);
                EditorGUI.PropertyField(propertyRect, targetProperty);

                accumulatedHeight += propertyRect.height + _propertySpace;

                if (propertyName == "targetPaths")
                {
                    DrawAddTargetPathButtonOnGUI(position, accumulatedHeight, targetProperty, out float buttonHeight);
                    accumulatedHeight += buttonHeight + _propertySpace;
                }
            }
            
            #endregion
        }
        
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float height = 0.0f;
            
            Rect position = new Rect(Vector2.zero, Vector2.up * _singleLineHeight);

            Rect rectForLabelHeight = GetPropertyRect(position, height);
            height += rectForLabelHeight.height + _propertySpace;

            foreach (var propertyName in _propertyNames)
            {
                SerializedProperty serializedProperty = property.FindPropertyRelative(propertyName);
                Rect propertyRect = GetPropertyRect(position, height, serializedProperty);
                height += propertyRect.height + _propertySpace;

                if (propertyName == "targetPaths")
                {
                    Rect rectForButtonHeight = GetPropertyRect(position, height);
                    height += rectForButtonHeight.height + _propertySpace;
                }
            }
            
            return height;
        }

        private void DrawAddTargetPathButtonOnGUI(Rect position, float accumulatedHeight, SerializedProperty targetPathsProperty, out float propertyHeight)
        {
            Rect buttonRect = GetPropertyRect(position, accumulatedHeight);
            propertyHeight = buttonRect.height;

            if (!GUI.Button(buttonRect, "Add TargetPath"))
            {
                return;
            }
            
            string selectedPath = EditorUtility.OpenFolderPanel(
                title: "Select for target path",
                folder: Application.dataPath,
                defaultName: ""
            );

            if (string.IsNullOrEmpty(selectedPath))
            {
                return;
            }
                
            int insertIndex = targetPathsProperty.arraySize;
            targetPathsProperty.InsertArrayElementAtIndex(insertIndex);
                
            SerializedProperty elementProperty = targetPathsProperty.GetArrayElementAtIndex(insertIndex);
            string relativePath = $"Assets/{Path.GetRelativePath(Application.dataPath, selectedPath)}"
                .Replace(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

            elementProperty.boxedValue = relativePath;
            targetPathsProperty.serializedObject.ApplyModifiedProperties();
        }
        
        private Rect GetPropertyRect(Rect position, float accumulatedHeight, SerializedProperty property = null)
        {
            Vector2 mostTopPropertyPosition = position.position + new Vector2(0.0f, accumulatedHeight);
            Vector2 propertySize = new Vector2(position.width, GetPropertyHeight(property));

            return new Rect(mostTopPropertyPosition, propertySize);
        }

        private float GetPropertyHeight(SerializedProperty property)
        {
            if (property == null || !property.isArray || !property.isExpanded)
            {
                return _singleLineHeight + _propertySpace;
            }

            return Mathf.Max(1, property.arraySize) * _singleLineHeight
                   + _arrayPropertyAdditiveHeight;
        }
    }
}
