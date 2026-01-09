:: Сборка конфига для nanoCAD BIM Строительство
call "..\..\3rdparty\ncad_UI_creator_60\NC_UI_Creator_App.exe" "NervanaNBIMsCommands.xml"

xcopy NervanaNBIMsCommands* "..\..\bin\Debug" /Y /I
xcopy Nervana-plugin-NcBIMs.package "..\..\bin\Debug" /Y /I
xcopy Nervana-plugin-NcBIMs_onlyUI.package "..\..\bin\Debug" /Y /I

xcopy NervanaNBIMsCommands* "..\..\bin\Release" /Y /I
xcopy Nervana-plugin-NcBIMs.package "..\..\bin\Release" /Y /I

:: Сборка конфига только для платформы nanoCAD
call "..\..\3rdparty\ncad_UI_creator_60\NC_UI_Creator_App.exe" "NervanaNcadCommands.xml"

xcopy NervanaNcadCommands* "..\..\bin\Debug" /Y /I
xcopy "Nervana-plugin-Nc.package" "..\..\bin\Debug" /Y /I
xcopy "Nervana-plugin-Nc_OLD.package" "..\..\bin\Debug" /Y /I
xcopy NervanaNcadCommands* "..\..\bin\Release" /Y /I
xcopy "Nervana-plugin-Nc.package" "..\..\bin\Release" /Y /I
xcopy "Nervana-plugin-Nc_OLD.package" "..\..\bin\Release" /Y /I

:: Сборка конфига для COM 
call "..\..\3rdparty\ncad_UI_creator_60\NC_UI_Creator_App.exe" "NervanaNcadCOMCommands.xml"

xcopy NervanaNcadCOMCommands* "..\..\bin\Debug" /Y /I
xcopy "Nervana-plugin-NcCOM.package" "..\..\bin\Debug" /Y /I
xcopy "Nervana-plugin-NcCOM_OLD.package" "..\..\bin\Debug" /Y /I
xcopy NervanaNcadCOMCommands* "..\..\bin\Release" /Y /I
xcopy "Nervana-plugin-NcCOM.package" "..\..\bin\Release" /Y /I
xcopy "Nervana-plugin-NcCOM_OLD.package" "..\..\bin\Release" /Y /I

del "..\..\bin\Debug\*.txt"
del "..\..\bin\Debug\*.xml"

:: copy icons
xcopy "..\icons" "..\..\bin\Debug\icons" /Y /I
xcopy "..\icons" "..\..\bin\Release\icons" /Y /I

pause