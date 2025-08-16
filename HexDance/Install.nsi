!define PRODUCT_EXE "${PACKAGE_ID}.exe"

OutFile "bin\${CONFIGURATION}\${PACKAGE_ID}_${CONFIGURATION}_v${VERSION}.exe"
RequestExecutionLevel user
InstallDir "$LOCALAPPDATA\${PACKAGE_ID}"

Page directory
Page instfiles

Section "Install"
    SetOutPath "$INSTDIR"
    File /r "${OUTPUT_PATH}\*.*"
    WriteUninstaller "$INSTDIR\Uninstall.exe"
    CreateShortCut "$SMSTARTUP\${PACKAGE_ID}.lnk" "$INSTDIR\${PRODUCT_EXE}"
    ExecShell "" "$INSTDIR\${PRODUCT_EXE}"
SectionEnd

Section "Uninstall"
    Delete "$SMSTARTUP\${PACKAGE_ID}.lnk"
    RMDir /r "$INSTDIR"
SectionEnd
