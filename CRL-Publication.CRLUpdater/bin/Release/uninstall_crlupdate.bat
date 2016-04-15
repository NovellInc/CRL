@echo off
setlocal
set filepath=%~dp0

"%filepath%CRL-Publication.CRLUpdate.exe" uninstall

timeout 10