@echo off

set "PROTOC_EXE=%cd%\protoc.exe"
set "WORK_DIR=%cd%\proto"
set "CS_OUT_PATH=%cd%\out\CS"

echo "==>>buildStart"
for /f "delims=" %%i in ('dir /b proto "proto/*.proto"') do (
    echo build file:%%%i
    %PROTOC_EXE% --proto_path="%WORK_DIR%" --csharp_out="%CS_OUT_PATH%" "%WORK_DIR%\%%i"
)
echo "==>>build finish"
echo "==>>copy cs"

::copy %cd%\out\CS\ ..\AxibugEmuOnline.Server\AxibugEmuOnline.Server\Protobuf
pause