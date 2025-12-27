# Nervana-nbims-plugin

Плагин для nanoCAD BIM Строительство и ModelStudio CS "Nervana" (автоматизация работы с параметрическими объектами, параметрами, прочие утилиты)

![](./res/icons/Nervana_PluginMainLogo_256x256.png)

[Telegram: View @nervana_nanocad_plugin_news](https://t.me/nervana_nanocad_plugin_news)

# Установка и использование

Руководство по установке см. [в отдельной статье](./docs/src/INSTALL.md).

Плагин распространяется в 3 конфигурациях:

- только для платформы nanoCAD (22-26) -- перечень функций [тут](./res/ncUI/NervanaNcadCommands.txt);

- для платформы nanoCAD и COM API CS (22-26) -- перечень функций [тут](./res/ncUI/NervanaNcadCOMCommands.txt);

- для платформы nanoCAD, COM API CS и nanoCAD BIM Строительство (25.0) -- перечень функций [тут](./res/ncUI/NervanaAllCommands.txt);

# Разработчику

В качестве вспомогательной нагрузки для Visual Studio потребуется .NET Framework 4.8, .NET 6.0. Пакеты для nanoCAD взяты для NuGet-сервера (потребуется [разово настроить](https://docs.nanocad.ru/articles/#!nbim-sdk-24-1/ifce8336bf4de47f19bf22466cba6ae30) у себя на ПК по инструкции).

Документация собирается через mdbook -- необходимо [скачать](https://github.com/rust-lang/mdBook/releases) исполняемый файл `mdbook.exe` и добавить его в переменную PATH (используется в скрипте генерации документации в `./docs/Nervana_docs_Build.bat`).

Файлы меню nanoCAD собираются через [самописный редактор](https://github.com/GeorgGrebenyuk/ncad_UI_creator), который требуется расположить тут `./3rdparty/ncad_UI_creator_60`.
