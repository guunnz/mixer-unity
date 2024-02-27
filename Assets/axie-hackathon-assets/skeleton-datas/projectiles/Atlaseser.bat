@echo off
setlocal enabledelayedexpansion

REM Loop through each .atlas file in the current directory and rename it to .atlas.txt
for %%f in (*.atlas) do (
    ren "%%f" "%%f.txt"
)

echo Renaming complete.
pause
