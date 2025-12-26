REM Step 1: Autobuild solution in Release-mode
set VSVER=[17.0^,18.0^)

::Edit path if VS 2022 is installed on other path
call "C:\Program Files\Microsoft Visual Studio\2022\Community\VC\Auxiliary\Build\vcvarsall.bat" x64

rmdir /s /q Nervana-app

::Build VinnyLibConverter
devenv Nervana-apps.sln /Build "Release|x64"

::For net6.0-windows
xcopy .\README.md "bin\Release" /Y /I
xcopy .\UPDATES.md "bin\Release" /Y /I
xcopy .\LICENSE "bin\Release" /Y /I
xcopy .\docs\NervanaGuide.pdf "bin\Release" /Y /I

xcopy bin\Release\*.* Nervana-app /Y /I /E

::ZIP release
del "Nervana-app.zip"
"C:\Program Files\7-Zip\7z" a -tzip "Nervana-app.zip" Nervana-app
rmdir /s /q Nervana-app

pause
::@endlocal
::@exit /B 1
