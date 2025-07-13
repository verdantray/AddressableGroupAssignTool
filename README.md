# Addressables Entry Tool

🌐 [한국어 README 읽기](./README.KO.md)

A tool that helps automate part of the setup process for Addressable Assets already registered or newly added in the Unity Editor.

## ✨ Features

- Allows you to create rule assets that define how **Address** and **Group** should be assigned when assets located in specific paths are registered as Addressables.
- Automatically assigns **Address** and **Group** to newly registered Addressable Assets based on predefined rules.
- Identifies existing Addressable Assets that do not conform to the rules and allows batch correction.
- If a duplicate Address value is set for an asset, a warning message is displayed in the Console.

## 📂 Use Case

This tool helps you manage Addressables more easily using rule-based configuration for a large number of assets.\
It's particularly useful for projects that require consistent Address naming conventions or group structures.

## 🛍 How to Use

### Opening the Tool Window and Description of GUI Elements

1. Open the Unity Editor.
2. Navigate to the following menu:

```
Window → Asset Management → Addressables → Addressables Entry Tool
```

3. In the tool window, click `Import RuleAsset` to assign an existing `AddressablesEntryAssignRule` ScriptableObject to the tool.
4. Or click `Create new RuleAsset` to generate a new `AddressablesEntryAssignRule` ScriptableObject at a specified path. It will be automatically assigned to the tool.
5. Edit the `Assign Group Rules` field in the Inspector of the created `AddressablesEntryAssignRule` asset to define how the tool should assign addresses and groups.
6. If the toggle `Save changes of Addressable assets immediately after assign group` is enabled, changes will be saved immediately. If disabled, you must press Ctrl + S in the asset’s Inspector or Addressables Groups window to finalize changes.
7. While the `Assign newly entry assets according to rule automatically` toggle is enabled, newly added Addressable Assets will be assigned automatically according to the rule.
8. The `Addressable asset list breaking rule` section lists Addressable Assets that do not match the rule. Click `Re-assign` for each, or use `Re-Assign all assets in list` to fix them all at once.

### Explanation of AddressablesEntryAssignRule Asset

1. The `AssignGroupRules` field is a list where each item includes the following:
2. `Target Group`: The Addressable Group to assign when an asset is registered. If empty, no group will be assigned.
3. `Target Paths`: One or more paths where the rule applies. All assets under these paths will be affected. Use `Add TargetPath` to specify directories.
4. `Asset Name Rule`: Defines how the Address should be set. Supports the following keywords:

```
[fullpath]   : Returns the full path including filename and extension.
[name]       : Returns the filename without extension.
[extension]  : Returns the file extension.
[lower]      : Converts the address to lowercase.
[nospace]    : Removes all spaces from the address.
```

## 📦 Installation

### Install via Git URL (Unity Package Manager)

1. Copy the repository URL:

```
https://github.com/verdantray/AddressablesEntryTool.git
```

2. Open the Unity project where you want to install the tool and launch the Package Manager.
3. Click the `+` button in the top-left corner of the Package Manager and select **Add package from Git URL...**, then paste the copied URL to install.

## 🛠 Requirements

- Requires the Addressables package (automatically installed with this package if not already present).

## 📄 License

BSD 3-Clause License. For more details, see the [LICENSE](./LICENSE) file.

