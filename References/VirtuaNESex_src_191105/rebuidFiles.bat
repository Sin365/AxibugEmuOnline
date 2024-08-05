@echo off
@echo " --- del AboutDlg.obj ---"

if exist ".\Debug\AboutDlg.obj" (
del ".\Debug\AboutDlg.obj"
)
if exist ".\Release\AboutDlg.obj" (
del ".\Release\AboutDlg.obj"
)

