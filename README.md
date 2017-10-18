# API tools.moab.pro (beta)
Описание API-интерфейса, библиотека и тестовый проект для взаимодействия с MOAB Tools

## Описание API

Для работы с API нужно получить API-ключ, который находится в вашем профиле http://tools.moab.pro/Profile. 
Все запросы отправляются на сервис методом POST в кодировке UTF-8 с Content-Type "application/json".

## Добавление заданий
Адрес, куда слать запросы: http://tools.moab.pro/api/Parse/AddTasks

Пример отправляемых данных:

```javascript
{
  task: tsk,
  api_key: api_key
}
```
tsk – конструкция вида:
```javascript
{
   phrases_list: [],
   regions: '',
   syntax: 1,
   depth: 1,
   db: 0,
   group_id: null,
   also_suggests: false,
   also_check: false,
   fix_words_order: false,
   type: 0,
   minus_words: [],
   suggests_types: [],
   suggests_depth: 1
}

```
Описание полей:

Поле  | Тип данных | Описание
------------- | ------------- | -------------
phrases_list  | массив строк | Массив исходных фраз<br />Максимальное количество фраз в массиве:<br />- Wordstat Deep  - 1<br />- Direct Check – 10 000<br />- Подсказки – 10 000<br />Хотя бы одна фраза должна быть в массиве
regions  | строка | Список регионов через запятую, пример "256,13,1"<br/>Все коды регионов - https://yandex.ru/yaca/geo.c2n<br/>По умолчанию – пустая строка (все регионы).
syntax  | число | Синтаксис запросов к Wordstat<br/>1 – без кавычек<br/>2 – в кавычках "слово1 слово2"<br/>3 – в кавычках с воскл.знаком "!слово1 !слово2"<br/>По умолчанию 1<br/>В Wordstat Deep и Подсказках поддерживается только 1
depth  | число | Глубина парсинга в Wordstat Deep<br/>По умолчанию 2<br/>Поддерживается только в Wordstat Deep, для остальных типов заданий нужно ставить 1
db  | число | Устройства в Wordstat<br/>0 – все устройства<br/>1 – десктопы<br/>2 – мобильные<br/>3 – только телефоны<br/>4 – только планшеты<br/>По умолчанию  0 (Все устройства)<br/>Поддерживается только в Wordstat Deep
group_id  | число | Id группы (в API пока не поддерживается<br/>Нужно ставить null – попадет в группу «Без группы»
also_suggests  | boolean | Поддерживается только в Wordstat Deep, соответствует флажку «Также получить Яндекс-подсказки по полученным фразам». Для остальных типов заданий нужно ставить false
also_check  | boolean | Поддерживается только в Wordstat Deep, соответствует флажку «Также проверить полученные подсказки на общую частоту Wordstat». Для остальных типов заданий нужно ставить false
fix_words_order  | boolean | Пока не поддерживается в API, нужно ставить false
type  | число | Тип выборки:<br/>0 – Wordstat Deep<br/>1 – Direct Check<br/>2 – Подсказки
minus_words  | массив строк | Массив минус-слов<br/>До 100 минус-слов в массиве<br/>Поддерживается только в Wordstat Deep, для остальных типов заданий массив должен быть пустым
suggests_types  | массив чисел | Способ сбора подсказок в Wordstat Deep или Подсказках:<br/>1 – фраза<br/>2 – фраза и пробел<br/>3 – фраза и русский алфавит<br/>4 – фраза и английский алфавит<br/>5 – фраза и цифры<br/>Нельзя передавать пустой массив, хотя бы одно число должно быть
suggests_depth  | число | Глубина сбора подсказок в Wordstat Deep или Подсказках. В Wordstat Deep всегда 1, в Подсказках может быть от 1 до 3

При успешном добавлении задания возвращается код 200 и JSON-конструкция вида:
```javascript
{
  total_pages: 10, 
  ids: [12345],
  errors: null
}
```

В случае ошибки возвращается HTTP-код 400 и JSON-конструкция вида:
```javascript
{
  total_pages: 0, 
  ids: null,
  errors: ["Задание с такими параметрами уже было добавлено вами ранее"]
}
```

Поле  | Тип данных | Описание
------------- | ------------- | -------------
total_pages  | число | Сервисная информация – сколько всего страниц с выборками у пользователя
ids  | массив чисел | Массив целых чисел – id добавленных заданий<br/>В случае запроса через API содержит одно число – id добавленного задания<br/>Если задание добавилось с ошибкой – возвращает null
errors  | массив строк | Массив описаний ошибок<br/>Если задания добавились успешно – значение null

Из массива ids нужно достать id задания и при помощи запроса Check раз в 5 секунд проверять его статус.

## Проверка статуса задания и получение результата

Адрес, куда слать запросы: http://tools.moab.pro/api/Parse/Check

Пример отправляемых данных:

```javascript
{
  id: task_id,
  api_key: api_key
}
```

Здесь task_id – целое число, идентификатор задания, полученный на этапе добавления задания.

Запрос можно выполнять не чаще 1 раза в 5 секунд.

Возвращаемые данные:

```javascript
{
  status: 0,
  download_zip: null
}
```

Поле  | Тип данных | Описание
------------- | ------------- | -------------
status  | число | Статус проверяемого задания<br>0 – новое<br/>1 – выполняется<br/>2 – завершено<br/>3 – приостановлено пользователем<br/>4 – приостановлено из-за нехватки баланса
download_zip  | строка | При статусах 2, 3 или 4 вернется абсолютный путь на скачивание готовой или приостановленной выборки. Если выборка в статусе «новая» или «выполняется» – вернется null.

## Готовая динамическая библиотека для работы с API (beta)

Библиотека написана на C#, распространяется в виде исходных кодов. Для работы с ней сохраните этот репозиторий в Zip-файл, откройте его в Visual Studio версии не ниже 2015 и скомпилируйте, затем добавьте ссылку на MoabTools.dll в ваш проект.

Зависимости:

* FluentValidation - https://github.com/JeremySkinner/FluentValidation/
* Newtowsoft.Json - https://github.com/JamesNK/Newtonsoft.Json

Возможности библиотеки:

* Создание и отправка задания
* Валидация задания перед отправкой
* Проверка статуса задания
* Наличие тестового проекта, в коде которого можно посмотреть примеры использования библиотеки

### Как пользоваться библиотекой

Чтобы создать и отправить задание, а затем проверить его состояние и скачать готовую выборку, выполните следующий код:

```c#
// создадим задание на парсинг WordstatDeep
// параметры по умолчанию - смотрим конструктор Task
// параметры соответствуют действию человека "зашел в сервис, ввёл фразу, нажал «Получить фразы», не вникая 
// в тонкие настройки сервиса
Task task = new Task();
task.phrases_list.Add("автострахование краснодар");
var task_valid = task.Validate(); // валидируем задание перед отправкой
if (task_valid != null)
{
    // если задание невалидно - дадим знать об этом пользователю
    Console.ForegroundColor = ConsoleColor.Red;
    foreach (var v in task_valid)
    {
        Console.WriteLine(v.ErrorMessage);
    }
    Console.ReadKey();
    return;
}

// если задание валидно - отправляем задание
Console.WriteLine("Отправляем задание");

// создадим запрос на добавление задания
Request req = new Request();
req.task = task;
req.api_key = api_key;
var req_valid = req.Validate(); // валидируем запрос
if (req_valid != null)
{
    // если запрос невалидный - дадим знать об этом пользователю
    Console.ForegroundColor = ConsoleColor.Red;
    foreach (var r in req_valid)
    {
        Console.WriteLine(r.ErrorMessage);
    }
    Console.ReadKey();
    return;
}

// отправляем запрос на добавление задания
RequestAnswer ans;

try
{
    ans = req.Send();
}
catch (WebException ex)
{
    // если вернулся ответ 400 - разберем его и покажем пользователю ошибки
    var resp = new StreamReader(ex.Response.GetResponseStream()).ReadToEnd();
    dynamic err = JsonConvert.DeserializeObject(resp.ToString());
    // todo
    throw;
}

var id = ans.ids[0];
            
// создадим запрос на проверку статуса задания
Check chk = new Check();
chk.id = id;
chk.api_key = api_key;

var chk_valid = chk.Validate();
if (chk_valid != null)
{
    // если запрос невалидный - дадим знать об этом пользователю
    Console.ForegroundColor = ConsoleColor.Red;
    foreach (var c in chk_valid)
    {
        Console.WriteLine(c.ErrorMessage);
    }
    Console.ReadKey();
    return;
}


// раз в 5 сек отправляем запрос на проверку статуса задания
CheckAnswer chk_ans;

while (true)
{

    Thread.Sleep(5000);

    Console.WriteLine($"Проверяем статус задания {id}");

    try
    {
        chk_ans = chk.Send();
    }
    catch (Exception)
    {
        continue;
    }

    if (chk_ans.status == 2 || chk_ans.status == 3 || chk_ans.status == 4)
    {
        Console.WriteLine("Скачиваем готовую выборку");
        // todo
        break;
    }

}
```

По всем вопросам, связанным с интеграцией API MOAB Tools в вашу систему, вы можете обращаться в техподдержку сервиса http://tools.moab.pro/Support
