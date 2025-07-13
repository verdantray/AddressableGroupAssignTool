# Addressables Entry Tool (한국어)

🌐 [View README in English](./README.md)

Unity Editor에서 이미 등록했거나 새로 등록하는 Addressable Asset의 일부 설정을 자동화할 수 있도록 도와주는 도구 입니다.

## ✨ 주요 기능

- 경로를 기반으로, 특정 경로에 있는 에셋이 Addressables에 등록되었을 때 **Address**와 **Group**이 어떻게 지정될 지 정할 수 있는 규칙 에셋을 만들 수 있습니다.
- 사전에 정의한 규칙에 따라 새로 등록하는 Addressable Asset의 **Address**와 **Group**을 자동 지정합니다.
- 이미 등록한 Addressable Asset 중 규칙에 어긋나게 설정된 에셋의 목록을 보여주고, 일괄 수정을 지원합니다.
- Addressable Asset의 Address를 수정할 때 다른 에셋과 중복되는 값을 입력할 경우 Console에 메시지를 표시합니다.

## 📂 포함 사례

프로젝트 내 수많은 에셋들에 대해 Addressables 설정을 규칙 기반으로 관리하기 쉽도록 돕습니다.
특히 일정한 Address 네이밍이나 Group 구조를 요구하는 프로젝트에서 매우 유용합니다.

## 🛍 사용 방법

### 툴 윈도우 열기 및 각 GUI 요소 설명

1. Unity Editor를 열습니다.
2. 다음 메뉴로 이동합니다:

```
Window → Asset Management → Addressables → Addressables Entry Tool
```

3. 툴 윈도우에서 `Import RuleAsset` 버튼을 눌러 기존에 생성했던 AddressablesEntryAssignRule 스크립터블 오브젝트를 툴 윈도우가 참조하도록 합니다.
4. 또는 툴 윈도우에서 `Create new RuleAsset` 버튼을 눌러 새 AddressablesEntryAssignRule 스크립터블 오브젝트를 지정한 경로에 생성할 수 있습니다. 이렇게 생성된 에셋은 툴 윈도우가 자동으로 참조합니다.
5. 생성된 AddressablesEntryAssignRule 에셋의 Inspector 화면에서 Assign Group Rules 필드를 편집하여 Addressable Asset이 등록될 때 적용할 규칙을 정합니다.
6. `Save changes of Addressable assets immediately after assign group` 토글이 활성화 되어있는 경우 Addressable Asset의 변경점을 즉시 저장합니다. 비활성화 되어있는 경우 변경점을 적용하려면 반드시 해당 에셋의 Inspector 화면이나 Addressables Groups 윈도우에서 Ctrl + S를 눌러 저장을 완료해야 합니다.
7. `Assign newly entry assets according to rule automatically` 토글이 활성화 되어있는 동안 새로 등록되는 Addressable Asset은 지정한 규칙이 즉시 적용됩니다.
8. `Addressable asset list breaking rule` 항목은 이미 등록된 Addressable Assets 중에서 규칙을 따르지 않는 에셋들을 목록으로 표시합니다. 각 항목의 `Re-assign` 버튼을 눌러 재지정하거나 하단의 `Re-Assign all assets in list` 버튼을 눌러 모든 에셋들을 한꺼번에 재지정할 수 있습니다.

### AddressablesEntryAssignRule 에셋 사용 설명

1. AssignGroupRules 필드는 리스트로 되어있으며 각 요소는 다음과 같이 이루어져 있습니다.<br>
2. `Target Group` : 에셋이 Addressable로 등록될 때 지정될 Addressable Group을 정합니다. 해당 필드가 비어 있을 경우 그룹을 지정하지 않습니다.<br>
3. `Target Paths` : 규칙을 지정할 에셋들의 경로를 정합니다. 여러 개의 경로를 지정할 수 있으며, 해당 경로 하위의 모든 에셋이 영향을 받습니다. `Add TargetPath` 버튼을 눌러 직접 디렉토리를 지정할 수 있습니다.<br>
4. `Asset Name Rule` : 에셋이 Addressable로 등록될 때 지정될 Address를 정합니다. 몇가지 키워드를 지원합니다.<br>
```
[fullpath] : 에셋의 이름과 확장자를 포함한 전체 경로를 반환합니다.
[name] : 에셋의 이름을 반환합니다. 확장자를 포함하지 않습니다.
[extension] : 에셋의 확장자를 반환합니다.
[lower] : Address 문자열 전체를 소문자로 바꾸어 반환합니다.
[nospace] : Address 문자열에 존재하는 공백문자를 모두 제거합니다.
>``` 

## 📦 설치 방법

### Git URL을 통한 설치 (Unity Package Manager 사용)

1. 리포지토리의 주소를 복사합니다.
```
https://github.com/verdantray/AddressablesEntryTool.git
```
2. Addressables Entry Tool을 적용할 Unity 프로젝트에서 Package Manager를 엽니다.
3. Package Manager 좌상단 `+` 버튼을 눌러 "Add package from Git URL..."을 선택해 1번의 주소를 붙여넣고 설치합니다.

## 🛠 필요 요건

- Addressables 패키지 설치 필요 (본 패키지 설치 시도 시 같이 설치됩니다.)

## 📄 라이선스

BSD 3-Clause License. 자세한 내용은 [LICENSE](./LICENSE) 파일을 참고하세요.

