@echo off
schtasks /Delete /F /TN VRChatActivityLogger
if not "%1" == "/c" (
  PAUSE
)
