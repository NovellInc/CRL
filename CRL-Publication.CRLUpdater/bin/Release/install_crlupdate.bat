@echo off
setlocal
set filepath=%~dp0

"%filepath%CRL-Publication.CRLUpdate.exe" install

timeout 10