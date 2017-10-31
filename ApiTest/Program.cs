using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MoabTools;
using System.IO;
using System.Net;
using Newtonsoft.Json;
using System.Threading;

namespace ApiTest
{
    class Program
    {
        static string api_key = ""; // ваш API-ключ

        static void Main(string[] args)
        {

            TasksList();
            CreateTask1();
            //CreateTask2();
            //CreateTask3();
        }


        static void TasksList()
        {

            // получим список выборок
            TasksList list = new TasksList();
            list.api_key = api_key;
            list.type = TaskType.WordstatDeep;
            list.page = 1;

            var list_valid = list.Validate(); // валидируем запрос
            if (list_valid != null)
            {
                // если запрос невалиден - дадим знать об этом пользователю
                Console.ForegroundColor = ConsoleColor.Red;
                foreach (var r in list_valid)
                {
                    Console.WriteLine(r.ErrorMessage);
                }
                Console.ReadKey();
                return;
            }

            // отправляем запрос на получение списка заданий
            TasksListAnswer ans;

            try
            {
                ans = list.List();
            }
            catch (WebException ex)
            {
                // если вернулся ответ 400 - разберем его и покажем пользователю ошибки
                var resp = new StreamReader(ex.Response.GetResponseStream()).ReadToEnd();
                dynamic err = JsonConvert.DeserializeObject(resp.ToString());
                // todo
                throw;
            }

            // покажем пользователю задания
            foreach(Task task in ans.tasks_list)
            {
                Console.WriteLine($"{task.full_name} - {task.status}");
            }

            Console.ReadLine();
        }

        static void CreateTask1()
        {

            // создадим задание на парсинг WordstatDeep
            // параметры по умолчанию - смотрим конструктор Task
            // параметры соответствуют действию человека "зашел в сервис, ввёл фразу, нажал «Получить фразы», 
            // не вникая в тонкие настройки сервиса
            Task task = new Task();
            task.phrases_list.Add("автострахование питер");
            
            // если задание валидно - отправляем задание
            Console.WriteLine("Отправляем задание");

            // создадим запрос на добавление задания
            Request req = new Request();
            req.task = task;
            req.api_key = api_key;
            var req_valid = req.Validate(); // валидируем запрос и задание
            if (req_valid != null)
            {
                // если запрос или задание невалидны - дадим знать об этом пользователю
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
            
            if(ans.exists_ids != null)
            {
                // задание с такими параметрами уже существует у пользователя 
                // в массиве exists_ids присутствует id
                return;
            }
            
            var id = ans.added_ids[0];
            
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
            
            Console.ReadKey();

        }
        
        static void CreateTask2()
        {
            // создадим задание на проверку частоты
            
            Task task = new Task();
            task.phrases_list.Add("агентство недвижимости");
            task.phrases_list.Add("агентство недвижимости что это такое");
            task.phrases_list.Add("агентство недвижимости для вас");
            task.phrases_list.Add("агентство недвижимости для всех");
            task.phrases_list.Add("агентство недвижимости москва");
            task.phrases_list.Add("сайт агентства недвижимости");
            task.phrases_list.Add("агентство недвижимости моя квартира");
            task.phrases_list.Add("агентство недвижимости наш дом");
            task.phrases_list.Add("агентство недвижимости город");
            task.phrases_list.Add("рейтинг агентств недвижимости");

            task.type = TaskType.DirectCheck;
            task.regions = "1,2"; // Москва и область, Санкт-Перербург
            task.syntax = Syntax.QuotesAndExclamation; // синтаксиси "!слово1 !слово2"
            task.depth = 1;
            task.also_suggests = false;
            task.also_check = false;

            Console.WriteLine("Отправляем задание");

            Request req = new Request();
            req.task = task;
            req.api_key = api_key;
            var req_valid = req.Validate();
            if (req_valid != null)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                foreach (var r in req_valid)
                {
                    Console.WriteLine(r.ErrorMessage);
                }
                Console.ReadKey();
                return;
            }

            RequestAnswer ans;

            try
            {
                ans = req.Send();
            }
            catch (WebException ex)
            {
                var resp = new StreamReader(ex.Response.GetResponseStream()).ReadToEnd();
                dynamic err = JsonConvert.DeserializeObject(resp.ToString());

                throw;
            }

            if (ans.exists_ids != null)
            {
                // задание с такими параметрами уже существует у пользователя 
                // в массиве exists_ids присутствует id
                return;
            }

            foreach (int id in ans.added_ids)
            {

                Check chk = new Check();
                chk.id = id;
                chk.api_key = api_key;

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

                    if (!string.IsNullOrWhiteSpace(chk_ans.download_zip))
                    {
                        Console.WriteLine("Скачиваем готовую выборку");
                        // todo
                        break;
                    }

                }
            }

            Console.ReadKey();
        }
        
        static void CreateTask3()
        {

            // создадим задание на парсинг посказок

            Task task = new Task();
            task.phrases_list.Add("агентство недвижимости");
            task.phrases_list.Add("агентство недвижимости что это такое");
            task.phrases_list.Add("агентство недвижимости для вас");
            task.phrases_list.Add("агентство недвижимости для всех");
            task.phrases_list.Add("агентство недвижимости москва");
            task.phrases_list.Add("сайт агентства недвижимости");
            task.phrases_list.Add("агентство недвижимости моя квартира");
            task.phrases_list.Add("агентство недвижимости наш дом");
            task.phrases_list.Add("агентство недвижимости город");
            task.phrases_list.Add("рейтинг агентств недвижимости");

            task.type = TaskType.Suggests;
            task.depth = 1;
            task.also_suggests = false;
            task.also_check = false;
            task.suggests_depth = 2;
            task.suggests_types.Add(SuggestType.PhraseAndDigits); // по умолчанию там уже есть PhraseAndSpace, см. конструктор

            Console.WriteLine("Отправляем задание");

            Request req = new Request();
            req.task = task;
            req.api_key = api_key;
            var req_valid = req.Validate();
            if (req_valid != null)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                foreach (var r in req_valid)
                {
                    Console.WriteLine(r.ErrorMessage);
                }
                Console.ReadKey();
                return;
            }

            RequestAnswer ans;

            try
            {
                ans = req.Send();
            }
            catch (WebException ex)
            {
                var resp = new StreamReader(ex.Response.GetResponseStream()).ReadToEnd();
                dynamic err = JsonConvert.DeserializeObject(resp.ToString());

                throw;
            }

            if (ans.exists_ids != null)
            {
                // задание с такими параметрами уже существует у пользователя 
                // в массиве exists_ids присутствует id
                return;
            }

            foreach (int id in ans.added_ids)
            {

                Check chk = new Check();
                chk.id = id;
                chk.api_key = api_key;

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

                    if (!string.IsNullOrWhiteSpace(chk_ans.download_zip))
                    {
                        Console.WriteLine("Скачиваем готовую выборку");
                        // todo
                        break;
                    }

                }
            }

            Console.ReadKey();
        }
    }
}
