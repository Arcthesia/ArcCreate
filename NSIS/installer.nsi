!define NAME "ArcCreate"
!define REGPATH_UNINSTSUBKEY "Software\Microsoft\Windows\CurrentVersion\Uninstall\${NAME}"
!define REGPATH_PLAYERPREFS "Software\Arcthesia\ArcCreate"
!define REGPATH_PLAYERPREFS_PARENT "Software\Arcthesia"
!define MUI_COMPONENTSPAGE_SMALLDESC
!define SRC_PATH "StandaloneWindows64"
Name "${NAME}"
OutFile "${NAME}-Installer.exe"
BrandingText "${NAME} Installer"
Unicode True
ManifestDPIAware True
XPStyle on
RequestExecutionLevel User
InstallDir "" ; will be chosen later
InstallDirRegKey HKCU "${REGPATH_UNINSTSUBKEY}" "UninstallString"
ShowInstDetails show
ShowUninstDetails show
SetCompress auto
SetCompressor bzip2


!include LogicLib.nsh
!include WinCore.nsh
!include Integration.nsh
!include MUI2.nsh
!include FileFunc.nsh

!define MUI_HEADERIMAGE
!define MUI_HEADERIMAGE_BITMAP "header.bmp"
!define MUI_ICON "installer.ico"
!define MUI_UNICON "uninstaller.ico"
!define MUI_WELCOMEFINISHPAGE_BITMAP "banner.bmp"
!define MUI_WELCOMEFINISHPAGE_BITMAP_STRETCH AspectFitHeight

!macro _DirExists _a _b _t _f
  !insertmacro _LOGICLIB_TEMP
  StrCpy $_LOGICLIB_TEMP "0"  
  StrCmp `${_b}` `` +3 0 ;if path is not blank, continue to next check
  IfFileExists `${_b}\*.*` 0 +2 ;if directory exists, continue to confirm exists
  StrCpy $_LOGICLIB_TEMP "1"
  StrCmp $_LOGICLIB_TEMP "1" `${_t}` `${_f}`
!macroend
!define DirExists `"" DirExists`

!macro _File _file
    File "/oname=${_file}" `${SRC_PATH}\${_file}`
!macroend
!define _File '!insertmacro "_File"'

!macro _OptionalFile _file _instdir
  IfFileExists `${_instdir}\${_file}` 0 +3
  File "/oname=${_file}.new" `${SRC_PATH}\${_file}`
  Goto +2
  File "/oname=${_file}" `${SRC_PATH}\${_file}`
!macroend
!define _OptionalFile '!insertmacro "_OptionalFile"'

Function .onInit
  SetShellVarContext Current
  ${If} $InstDir == "" ; No /D= nor InstallDirRegKey
    GetKnownFolderPath $InstDir ${FOLDERID_UserProgramFiles} ; This folder only exists on Win7+
    StrCmp $InstDir "" 0 +2 
    StrCpy $InstDir "$LocalAppData\Programs" ; Fallback directory

    StrCpy $InstDir "$InstDir\$(^Name)"
  ${EndIf}
FunctionEnd

Function un.onInit
  SetShellVarContext Current
FunctionEnd


!define MUI_FINISHPAGE_NOAUTOCLOSE
!define MUI_UNFINISHPAGE_NOAUTOCLOSE

!insertmacro MUI_PAGE_WELCOME
!insertmacro MUI_PAGE_DIRECTORY
Var StartMenuFolder
!insertmacro MUI_PAGE_STARTMENU StartMenuPage $StartMenuFolder
!insertmacro MUI_PAGE_INSTFILES

!define MUI_FINISHPAGE_RUN "$InstDir\ArcCreate.exe"
!insertmacro MUI_PAGE_FINISH

!insertmacro MUI_UNPAGE_WELCOME
!insertmacro MUI_UNPAGE_COMPONENTS
!insertmacro MUI_UNPAGE_CONFIRM
!insertmacro MUI_UNPAGE_INSTFILES
!insertmacro MUI_UNPAGE_FINISH

!insertmacro MUI_LANGUAGE "English"
!insertmacro MUI_LANGUAGE "TradChinese"

LangString DESC_SecProgramFiles ${LANG_ENGLISH} "Files required for ArcCreate to run."
LangString DESC_SecProgramFiles ${LANG_TRADCHINESE} "ArcCreate 所需執行檔。"
LangString DESC_SecRemoveUserData ${LANG_ENGLISH} "Remove PlayerPrefs, StreamingAssets etc. "
LangString DESC_SecRemoveUserData ${LANG_TRADCHINESE} "刪除 PlayerPrefs, StreamingAssets 等使用者設定"




Section "Program files (Required)" SecProgramFiles
  SectionIn Ro

  SetOutPath $InstDir
  WriteUninstaller "$InstDir\uninstall.exe"
  WriteRegStr HKCU "${REGPATH_UNINSTSUBKEY}" "Comments" "Fast and powerful .aff editor made with Unity."
  WriteRegStr HKCU "${REGPATH_UNINSTSUBKEY}" "DisplayName" "${NAME}"
  WriteRegStr HKCU "${REGPATH_UNINSTSUBKEY}" "DisplayIcon" "$InstDir\ArcCreate.exe,0"
  ${GetTime} "" "L" $0 $1 $2 $3 $4 $5 $6
  WriteRegStr HKCU "${REGPATH_UNINSTSUBKEY}" "DisplayVersion" "$2.$1.$0$4$5"
  WriteRegStr HKCU "${REGPATH_UNINSTSUBKEY}" "Publisher" "Arcthesia"
  WriteRegStr HKCU "${REGPATH_UNINSTSUBKEY}" "URLInfoAbout" "https://github.com/Arcthesia/ArcCreate"
  WriteRegStr HKCU "${REGPATH_UNINSTSUBKEY}" "UninstallString" '"$InstDir\uninstall.exe"'
  WriteRegStr HKCU "${REGPATH_UNINSTSUBKEY}" "QuietUninstallString" '"$InstDir\uninstall.exe" /S'
  WriteRegDWORD HKCU "${REGPATH_UNINSTSUBKEY}" "NoModify" 1
  WriteRegDWORD HKCU "${REGPATH_UNINSTSUBKEY}" "NoRepair" 1

  !include "install-files.nsi"
  !include "install-files-optional.nsi"
  File "ArcCreateIcons.dll"

  ${GetSize} "$INSTDIR" "/S=0K" $0 $1 $2
  IntFmt $0 "0x%08X" $0
  WriteRegDWORD HKCU "${REGPATH_UNINSTSUBKEY}" "EstimatedSize" "$0"

  ; create some useful folders here
  CreateDirectory "$InstDir\Macros"

  !insertmacro MUI_STARTMENU_WRITE_BEGIN StartMenuPage
    CreateDirectory "$SMPrograms\$StartMenuFolder"
    CreateShortcut /NoWorkingDir "$SMPrograms\$StartMenuFolder\${NAME}.lnk" "$InstDir\ArcCreate.exe"
    CreateShortcut /NoWorkingDir "$SMPrograms\$StartMenuFolder\Uninstall.lnk" "$InstDir\uninstall.exe"
  !insertmacro MUI_STARTMENU_WRITE_END

SectionEnd
!insertmacro MUI_FUNCTION_DESCRIPTION_BEGIN
  !insertmacro MUI_DESCRIPTION_TEXT ${SecProgramFiles} $(DESC_SecProgramFiles)
!insertmacro MUI_FUNCTION_DESCRIPTION_END


!define FileAssoc_Extension ".arcproj"
!define FileAssoc_ProgramID "ArcCreate.Project"
!define FileAssoc_Verb "open"
!define FileAssoc_Executable "ArcCreate.exe"
!define FileAssoc2_Extension ".aff"
!define FileAssoc2_ProgramID "ArcCreate.Aff"
!define FileAssoc3_Extension ".arcpkg"
!define FileAssoc3_ProgramID "ArcCreate.Arcpkg"
Section -FileAssociation
  # register arcproj
  WriteRegStr ShCtx "Software\Classes\${FileAssoc_ProgramID}\DefaultIcon" "" "$InstDir\ArcCreateIcons.dll,0"
  WriteRegStr ShCtx "Software\Classes\${FileAssoc_ProgramID}\shell\${FileAssoc_Verb}" "" "&Open ArcCreate Project"
  WriteRegStr ShCtx "Software\Classes\${FileAssoc_ProgramID}\shell\${FileAssoc_Verb}\command" "" '"$InstDir\${FileAssoc_Executable}" "%1"'
  WriteRegStr ShCtx "Software\Classes\${FileAssoc_Extension}" "" "${FileAssoc_ProgramID}"
  # register arcpkg
  WriteRegStr ShCtx "Software\Classes\${FileAssoc3_ProgramID}\DefaultIcon" "" "$InstDir\ArcCreateIcons.dll,1"
  WriteRegStr ShCtx "Software\Classes\${FileAssoc3_Extension}" "" "${FileAssoc3_ProgramID}"
  WriteRegStr ShCtx "Software\Classes\${FileAssoc3_Extension}" "PerceivedType" "compressed"
  WriteRegNone ShCtx "Software\Classes\${FileAssoc3_Extension}\OpenWithProgids" "CompressedFolder"
  # register aff
  WriteRegStr ShCtx "Software\Classes\${FileAssoc2_ProgramID}\DefaultIcon" "" "$InstDir\ArcCreateIcons.dll,2"
  WriteRegStr ShCtx "Software\Classes\${FileAssoc2_Extension}" "" "${FileAssoc2_ProgramID}"
  WriteRegStr ShCtx "Software\Classes\${FileAssoc2_Extension}" "PerceivedType" "text"

  # File association
  WriteRegNone ShCtx "Software\Classes\${FileAssoc_Extension}\OpenWithList" "${FileAssoc_Executable}"
  WriteRegNone ShCtx "Software\Classes\${FileAssoc_Extension}\OpenWithProgids" "${FileAssoc_ProgramID}"
  WriteRegStr ShCtx "Software\Classes\Applications\${FileAssoc_Executable}\shell\open\command" "" '"$InstDir\${FileAssoc_Executable}" "%1"'
  WriteRegStr ShCtx "Software\Classes\Applications\${FileAssoc_Executable}" "FriendlyAppName" "ArcCreate"
  WriteRegStr ShCtx "Software\Classes\Applications\${FileAssoc_Executable}" "ApplicationCompany" "Arcthesia"
  WriteRegNone ShCtx "Software\Classes\Applications\${FileAssoc_Executable}\SupportedTypes" "${FileAssoc_Extension}"


  ${NotifyShell_AssocChanged}
SectionEnd


Section -un.FileAssociation
  # Unregister arcproj
  ClearErrors
  DeleteRegKey ShCtx "Software\Classes\${FileAssoc_ProgramID}\shell\${FileAssoc_Verb}"
  DeleteRegKey /IfEmpty ShCtx "Software\Classes\${FileAssoc_ProgramID}\shell"
  ${IfNot} ${Errors}
    DeleteRegKey ShCtx "Software\Classes\${FileAssoc_ProgramID}\DefaultIcon"
  ${EndIf}
  ReadRegStr $0 ShCtx "Software\Classes\${FileAssoc_Extension}" ""
  DeleteRegKey /IfEmpty ShCtx "Software\Classes\${FileAssoc_ProgramID}"
  ${IfNot} ${Errors}
  ${AndIf} $0 == "${FileAssoc_ProgramID}"
    DeleteRegValue ShCtx "Software\Classes\${FileAssoc_Extension}" ""
    DeleteRegKey /IfEmpty ShCtx "Software\Classes\${FileAssoc_Extension}"
  ${EndIf}
  # unregister aff
  DeleteRegKey ShCtx "Software\Classes\${FileAssoc2_ProgramID}\DefaultIcon"
  ReadRegStr $0 ShCtx "Software\Classes\${FileAssoc2_Extension}" ""
  ${If} $0 == "${FileAssoc2_ProgramID}"
    DeleteRegValue ShCtx "Software\Classes\${FileAssoc2_Extension}" "PerceivedType"
    DeleteRegValue ShCtx "Software\Classes\${FileAssoc2_Extension}" ""
    DeleteRegKey /IfEmpty ShCtx "Software\Classes\${FileAssoc2_Extension}"
  ${EndIf}
  # unregister arcpkg
  DeleteRegKey ShCtx "Software\Classes\${FileAssoc3_ProgramID}\DefaultIcon"
  ReadRegStr $0 ShCtx "Software\Classes\${FileAssoc3_Extension}" ""
  DeleteRegValue ShCtx "Software\Classes\${FileAssoc3_Extension}\OpenWithProgids" "CompressedFolder"
  DeleteRegValue ShCtx "Software\Classes\${FileAssoc3_Extension}" "PerceivedType"
  DeleteRegValue ShCtx "Software\Classes\${FileAssoc3_Extension}" ""
  DeleteRegKey /IfEmpty ShCtx "Software\Classes\${FileAssoc3_Extension}"


  # Unregister file association
  DeleteRegValue ShCtx "Software\Classes\${FileAssoc_Extension}\OpenWithList" "${FileAssoc_Executable}"
  DeleteRegKey /IfEmpty ShCtx "Software\Classes\${FileAssoc_Extension}\OpenWithList"
  DeleteRegValue ShCtx "Software\Classes\${FileAssoc_Extension}\OpenWithProgids" "${FileAssoc_ProgramID}"
  DeleteRegKey /IfEmpty ShCtx "Software\Classes\${FileAssoc_Extension}\OpenWithProgids"
  DeleteRegKey /IfEmpty  ShCtx "Software\Classes\${FileAssoc_Extension}"
  DeleteRegKey ShCtx "Software\Classes\Applications\${FileAssoc_Executable}"

  # Explorer may leave garbage like opened file history, this cleans them up
  DeleteRegValue HKCU "Software\Microsoft\Windows\CurrentVersion\Search\JumplistData" "$InstDir\${FileAssoc_Executable}"
  DeleteRegValue HKCU "Software\Classes\Local Settings\Software\Microsoft\Windows\Shell\MuiCache" "$InstDir\${FileAssoc_Executable}.FriendlyAppName"
  DeleteRegValue HKCU "Software\Classes\Local Settings\Software\Microsoft\Windows\Shell\MuiCache" "$InstDir\${FileAssoc_Executable}.ApplicationCompany"
  DeleteRegValue HKCU "Software\Microsoft\Windows\ShellNoRoam\MUICache" "$InstDir\${FileAssoc_Executable}"
  DeleteRegValue HKCU "Software\Microsoft\Windows NT\CurrentVersion\AppCompatFlags\Compatibility Assistant\Store" "$InstDir\${FileAssoc_Executable}"
  DeleteRegValue HKCU "Software\Microsoft\Windows\CurrentVersion\ApplicationAssociationToasts" "${FileAssoc_ProgramID}_${FileAssoc_Extension}"
  DeleteRegValue HKCU "Software\Microsoft\Windows\CurrentVersion\ApplicationAssociationToasts" "Applications\${FileAssoc_Executable}_${FileAssoc_Extension}"
  DeleteRegValue HKCU "Software\Microsoft\Windows\CurrentVersion\Explorer\FileExts\${FileAssoc_Extension}\OpenWithProgids" "${FileAssoc_ProgramID}"
  DeleteRegKey /IfEmpty HKCU "Software\Microsoft\Windows\CurrentVersion\Explorer\FileExts\${FileAssoc_Extension}\OpenWithProgids"
  DeleteRegKey /IfEmpty HKCU "Software\Microsoft\Windows\CurrentVersion\Explorer\FileExts\${FileAssoc_Extension}\OpenWithList"
  DeleteRegKey /IfEmpty HKCU "Software\Microsoft\Windows\CurrentVersion\Explorer\FileExts\${FileAssoc_Extension}"

  ${NotifyShell_AssocChanged}
SectionEnd


Section /o "un.Remove User Data" SecRemoveUserData
  !include "uninstall-files-optional.nsi"

  DeleteRegKey HKCU "${REGPATH_PLAYERPREFS}"
  DeleteRegKey /IfEmpty HKCU "${REGPATH_PLAYERPREFS_PARENT}"
SectionEnd
!insertmacro MUI_UNFUNCTION_DESCRIPTION_BEGIN
  !insertmacro MUI_DESCRIPTION_TEXT ${SecRemoveUserData} $(DESC_SecRemoveUserData)
!insertmacro MUI_UNFUNCTION_DESCRIPTION_END


Section -Uninstall
  !include "uninstall-files.nsi"

  ; useful folders
  RMDir "$InstDir\Macros"

  !insertmacro MUI_STARTMENU_GETFOLDER StartMenuPage $StartMenuFolder
  ${UnpinShortcut} "$SMPrograms\$StartMenuFolder\${NAME}.lnk"
  Delete "$SMPrograms\$StartMenuFolder\${NAME}.lnk"
  Delete "$SMPrograms\$StartMenuFolder\Uninstall.lnk"
  RMDir "$SMPrograms\$StartMenuFolder"
  DeleteRegKey HKCU "${REGPATH_UNINSTSUBKEY}"
  Delete "$InstDir\ArcCreateIcons.dll"
  Delete "$InstDir\uninstall.exe"
  RMDir "$InstDir"
SectionEnd
