:: Сборка всего конфига
call "..\..\3rdparty\ncad_UI_creator_60\NC_UI_Creator_App.exe" "NervanaAllCommands.xml"

xcopy NervanaAllCommands* "..\..\bin\Debug" /Y /I
xcopy Nervana-plugin.package "..\..\bin\Debug" /Y /I
xcopy NervanaAllCommands* "..\..\bin\Release" /Y /I
xcopy Nervana-plugin.package "..\..\bin\Release" /Y /I

:: Сборка конфига только для платформы nanoCAD
call "..\..\3rdparty\ncad_UI_creator_60\NC_UI_Creator_App.exe" "NervanaNcadCommands.xml"

xcopy NervanaNcadCommands* "..\..\bin\Debug" /Y /I
xcopy "Nervana-plugin-NcadOnly.package" "..\..\bin\Debug" /Y /I
xcopy NervanaNcadCommands* "..\..\bin\Release" /Y /I
xcopy "Nervana-plugin-NcadOnly.package" "..\..\bin\Release" /Y /I

:: copy icons
xcopy "..\icons" "..\..\bin\Debug\icons" /Y /I
xcopy "..\icons" "..\..\bin\Release\icons" /Y /I

pause