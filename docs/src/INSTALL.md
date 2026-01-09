# Установка плагина

Последняя версия плагина [доступна по ссылке](https://github.com/GeorgGrebenyuk/Nervana-nbims-plugin/releases/latest). Скачайте архив с именем `Nervana-app.zip` и распакуйте себе на ПК.

Функции плагина разделены на 3 отдельных модуля (библиотек):

- для платформы nanoCAD (22-26) - имя на ленте "Nervana NC";

- для вертикальных приложения ModelStudio CS, nanoCAD BIM Строительство (взаимодействие с COM API) - имя на ленте "Nervana COM";

- для .NET API nanoCAD BIM Строительство (25.0) - имя на ленте "Nervana NBIMS";

Каждый из модулей описывается отдельной конфигурацией (файлами пользовательского интерфейса). Для удобства, конфигурации представлены в виде готовых подключаемых пакетов `*.package`, которые необходимо будет добавить в автозагрузку nanoCAD (команда `APPLOAD`) по следующей схеме:

![](assets/2025-12-26-23-52-26-image.png)

- для nanoCAD BIM Строительство: `Nervana-plugin-NcBIMs.package`;

- для ModelStudio CS: `Nervana-plugin-NcCOM.package`;

- для платформы nanoCAD: `Nervana-plugin-Nc.package`;

Для запуска плагина под версии nanoCAD 22 и старше используйте `*.package`-файлы с суффиксом `_OLD`.
