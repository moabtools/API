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
        static string api_key = "0fa37306-4c79-4c87-9d68-79868abefe36"; // ваш API-ключ

        static void Main(string[] args)
        {

            CreateTask1();
            //CreateTask2();
            //CreateTask3();
        }

        static void CreateTask1()
        {

            // создадим задание на парсинг WordstatDeep
            // параметры по умолчанию - смотрим конструктор Task
            // параметры соответствуют действию человека "зашел в сервис, ввёл фразу, нажал «Получить фразы», 
            // не вникая в тонкие настройки сервиса
            Task task = new Task();
            task.phrases_list.Add("автострахование краснодар!");
            
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

            var task_valid = task.Validate();
            if (task_valid != null)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                foreach (var v in task_valid)
                {
                    Console.WriteLine(v.ErrorMessage);
                }
                Console.ReadKey();
                return;
            }

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


            foreach (int id in ans.ids)
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

            var task_valid = task.Validate();
            if (task_valid != null)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                foreach (var v in task_valid)
                {
                    Console.WriteLine(v.ErrorMessage);
                }
                Console.ReadKey();
                return;
            }

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


            foreach (int id in ans.ids)
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
