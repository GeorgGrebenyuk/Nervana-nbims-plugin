call "..\..\3rdparty\ncad_UI_creator_60\NC_UI_Creator_App.exe" "NervanaCommands.xml"

xcopy NC_UI_DEF* "..\..\bin\Debug" /Y /I
xcopy Nervana-plugin.package "..\..\bin\Debug" /Y /I
xcopy NC_UI_DEF* "..\..\bin\Release" /Y /I
xcopy Nervana-plugin.package "..\..\bin\Release" /Y /I

:: copy icons
xcopy "..\icons" "..\..\bin\Debug\icons" /Y /I
xcopy "..\icons" "..\..\bin\Release\icons" /Y /I

pause