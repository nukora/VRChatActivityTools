@echo off
cd "%~dp0"
if exist task_actual.xml del task_actual.xml
setlocal enabledelayedexpansion
for /f "delims=" %%a in (task.xml) do (
  set line=%%a
  echo !line:__DIR__=%~dp0! >> task_actual.xml
)

schtasks /Create /F /XML task_actual.xml /TN VRChatActivityLogger
del task_actual.xml

if not "%1" == "/c" (
  PAUSE
)
