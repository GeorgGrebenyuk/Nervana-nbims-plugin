# Установка плагина

Последняя версия плагина [доступна по ссылке](https://github.com/GeorgGrebenyuk/Nervana-nbims-plugin/releases/latest). Скачайте архив с именем `Nervana-app.zip` и распакуйте себе на ПК.

## При работе с nanoCAD

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

## При работе с CADLib: Архив

**Внимание**! Вам потребуются права администратора для редактирования файла с перечнем подключаемых к CADLib плагинам.

1. Идём по пути `C:\Program Files (x86)\CSoft\Model Studio CS\Library\bin\x64` или в иное место (например, `C:\Program Files (x86)\CSoft\Model Studio CS\3.0\Library\bin\x64`), где установлена программа "Менеджер библиотек стандартных компонентов";

2. Открываем в любом редакторе файл `plugins.xml`;

3. Добавляем в тело `Plugins` новую запись для нашего подключаемого плагина

```xml
<?xml version="1.0" encoding="utf-8" ?>
<Plugins logFolder="Library">
  ...
  <Plugin name="C:\Users\Georg\Documents\GitHub\Nervana-nbims-plugin\bin\Debug\net48\NervanaCADLibLibraryMgd.dll"/>
</Plugins>
```

Где в строке с `Plugin` указываем ПОЛНЫЙ путь к файлу `NervanaCADLibLibraryMgd.dll` из распакованного архива.

4. Сохраняем файлы (нужны права Администратора);

Эти действия необходимо будет выполнить лишь единожды, в дальнейшем они не потребуются. 

После перезапуска МБСК появится новая вкладка меню "Nervana" с командами плагина.

**Примечание**: вы также можете подгружать плагин и в сам CADLib (другой файл xml), работать должно также.
