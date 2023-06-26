@echo off
echo Starting Build
dotnet build
echo Plugin build finished
echo Building Addressables
"%ProgramFiles%\Unity\Hub\Editor\2020.3.33f1\Editor\Unity.exe" -projectPath "%CD%\ksp2-inputbinder-assets\" -quit -batchmode -executeMethod BuildAssets.PerformBuild
xcopy "%CD%\ksp2-inputbinder-assets\Library\com.unity.addressables\aa\Windows\" "%CD%\build\GameData\Mods\inputbinder\addressables\" /Y /I /E
echo Done