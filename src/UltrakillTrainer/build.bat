@echo off
echo Compiling UltrakillTrainer...
"C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe" /target:exe /out:UltrakillTrainer.exe /platform:x64 Program.cs GameObjects.cs Memory.cs
if %ERRORLEVEL% EQU 0 (
    echo.
    echo ==========================================
    echo Compilation successful! Created UltrakillTrainer.exe
    echo ==========================================
) else (
    echo.
    echo [!] Compilation failed.
)
pause