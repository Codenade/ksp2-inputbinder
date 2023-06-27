@echo off
SETLOCAL EnableDelayedExpansion
for /F "tokens=1,2 delims=#" %%a in ('"prompt #$H#$E# & echo on & for %%b in (1) do     rem"') do (
  set "DEL=%%a"
)
powershell rm "$PWD\build\build.zip"
echo Starting assembly build
dotnet build >> %CD%/build.log
if errorlevel 1 goto ERROR
call :colorEcho 0A "Assembly build finished"
echo.
echo Building assets
"%ProgramFiles%\Unity\Hub\Editor\2020.3.33f1\Editor\Unity.exe" -projectPath "%CD%\ksp2-inputbinder-assets\" -quit -batchmode -executeMethod BuildAssets.PerformBuild >> %CD%/build.log
if errorlevel 1 goto ERROR
xcopy "%CD%\ksp2-inputbinder-assets\Library\com.unity.addressables\aa\Windows\" "%CD%\build\\GameData\Mods\inputbinder\addressables\" /Y /I /E >> %CD%/build.log
call :colorEcho 0A "Building assets finished"
echo.
echo Creating build.zip
powershell Compress-Archive -Path "$PWD\build\*" -DestinationPath "$PWD\build\build.zip" >> %CD%/build.log
call :colorEcho 0A "Done"
echo.
exit /b 0

:ERROR
call :colorEcho 0C "Build failed"
echo.
exit /b 1

:colorEcho
echo off
<nul set /p ".=%DEL%" > "%~2"
findstr /v /a:%1 /R "^$" "%~2" nul
del "%~2" > nul 2>&1i