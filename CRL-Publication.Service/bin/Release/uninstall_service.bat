@echo off
setlocal
set filepath=%~dp0

"%filepath%CRL-Publication.Service.exe" uninstall

timeout 10