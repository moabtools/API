using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;
using System.Text.RegularExpressions;

namespace MoabTools
{

    public class RunPauseValidator : AbstractValidator<RunPauseTask>
    {
        public RunPauseValidator()
        {
            RuleFor(chk => chk.task_id).GreaterThan(0).WithMessage("ID задания должно быть больше 0");
            RuleFor(chk => chk.api_key).NotNull().WithMessage("API-ключ не может быть пустым");
            RuleFor(chk => chk.api_key).Must(BeAValidGuid).WithMessage("API-ключ передан в неверном формате");
        }

        private bool BeAValidGuid(string guid)
        {
            Guid result;
            return Guid.TryParse(guid, out result);
        }
    }

    public class RequestValidator : AbstractValidator<Request>
    {
        public RequestValidator()
        {
            RuleFor(req => req.task).NotNull().WithMessage("Задание не может быть пустым");
            RuleFor(req => req.task).SetValidator(new TaskValidator());

            RuleFor(req => req.api_key).NotNull().WithMessage("API-ключ не может быть пустым");
            RuleFor(req => req.api_key).Must(BeAValidGuid).WithMessage("API-ключ передан в неверном формате");
        }

        private bool BeAValidGuid(string guid)
        {
            Guid result;
            return Guid.TryParse(guid, out result);
        }
    }

    public class TasksListValidator : AbstractValidator<TasksList>
    {
        public TasksListValidator()
        {
            RuleFor(list => list.api_key).NotNull().WithMessage("API-ключ не может быть пустым");
            RuleFor(list => list.api_key).Must(BeAValidGuid).WithMessage("API-ключ передан в неверном формате");
        }

        private bool BeAValidGuid(string guid)
        {
            Guid result;
            return Guid.TryParse(guid, out result);
        }

    }

    public class CheckValidator : AbstractValidator<Check>
    {
        public CheckValidator()
        {
            RuleFor(chk => chk.id).GreaterThan(0).WithMessage("ID задания должно быть больше 0");
            RuleFor(chk => chk.api_key).NotNull().WithMessage("API-ключ не может быть пустым");
            RuleFor(chk => chk.api_key).Must(BeAValidGuid).WithMessage("API-ключ передан в неверном формате");

        }

        private bool BeAValidGuid(string guid)
        {
            Guid result;
            return Guid.TryParse(guid, out result);
        }
    }

    public class TaskValidator : AbstractValidator<Task>
    {
        public TaskValidator()
        {
            CascadeMode = CascadeMode.StopOnFirstFailure;

            RuleFor(task => task.phrases_list).NotEmpty().WithMessage("Список исходных фраз не может быть пустым");
            RuleFor(task => task.phrases_list).Must(list => list.Count == 1).When(task => task.type == TaskType.WordstatDeep).WithMessage("Список исходных фраз должен содержать минимум одну фразу");
            RuleFor(task => task.phrases_list).Must(list => list.Count <= 10000).When(task => task.type == TaskType.DirectCheck || task.type == TaskType.Suggests).WithMessage("Список исходных фраз не может содержать более 10 000 фраз"); 
            RuleFor(task => task.phrases_list).SetCollectionValidator(new PhrasesListValidator());

            RuleFor(task => task.regions).Must(BeValidRegion).WithMessage("Ошибка в списке регионов");

            RuleFor(task => task.syntax).IsInEnum().WithMessage("Неверный синтаксис");
            RuleFor(task => task.syntax).Equal(Syntax.NoQuotes).When(task => task.type == TaskType.WordstatDeep || task.type == TaskType.Suggests).WithMessage("В заданиях типа Wordstat Deep и Подсказки нужно указывать синтаксис 1 - без кавычек");

            RuleFor(task => task.depth).GreaterThanOrEqualTo(1).When(task => task.type == TaskType.WordstatDeep).WithMessage("Глубина не может быть меньше 1");
            RuleFor(task => task.depth).LessThanOrEqualTo(2).When(task => task.type == TaskType.WordstatDeep).WithMessage("Глубина не может быть больше 2");
            RuleFor(task => task.depth).Equal(1).When(task => task.type == TaskType.DirectCheck || task.type == TaskType.Suggests).WithMessage("Глубина должна равнятся 1");
            
            
            RuleFor(task => task.db).IsInEnum().WithMessage("Неверный тип устройств Wordstat");

            RuleFor(task => task.group_id).Null().WithMessage("Поле group_id пока не поддерживается системой");

            RuleFor(task => task.fix_words_order).Equal(false).WithMessage("Поле fix_words_order пока не поддерживается системой");
            RuleFor(task => task.type).IsInEnum().WithMessage("Неверный тип задания");

            RuleFor(task => task.also_suggests).Equal(false).When(task => task.type != TaskType.WordstatDeep).WithMessage("Поле also_suggests должно быть равно false, если тип задание не Wordstat Deep");
            RuleFor(task => task.also_check).Equal(false).When(task => task.also_suggests == false).WithMessage("Поле also_check должно быть равно false, если поле also_suggests равно false");

            RuleFor(task => task.minus_words).Empty().When(task => task.type == TaskType.DirectCheck || task.type == TaskType.Suggests).WithMessage("Список минус-слов должен быть пустым для заданий типа Direct Check или Подсказки");
            RuleFor(task => task.minus_words).Must(list => list.Count <= 100).When(task => task.type == TaskType.WordstatDeep).WithMessage("Список минус-слов содержит больше 100 слов");
            RuleFor(task => task.minus_words).SetCollectionValidator(new MinusWordsValidator());
            RuleFor(task => task.minus_words).Must((t, l) => t.phrases_list.Except(l).ToList().Count != t.phrases_list.Count).When(task => task.minus_words.Count > 0).WithMessage("Список исходных фраз содержит одно или несколько минус-слов из списка минус-слов");

            RuleFor(task => task.suggests_types).NotEmpty().When(task => (task.type == TaskType.WordstatDeep && task.also_suggests) || task.type == TaskType.Suggests).WithMessage("Не заданы способы перебора подсказок");
            RuleFor(task => task.suggests_depth).GreaterThanOrEqualTo(1).When(task => (task.type == TaskType.WordstatDeep && task.also_suggests) || task.type == TaskType.Suggests).WithMessage("Глубина перебора подсказок не может быть меньше 1");
            RuleFor(task => task.suggests_depth).LessThanOrEqualTo(3).When(task => (task.type == TaskType.WordstatDeep && task.also_suggests) || task.type == TaskType.Suggests).WithMessage("Глубина перебора подсказок не может быть больше 3");

        }

        private bool BeValidRegion(string regions)
        {
            if (string.IsNullOrWhiteSpace(regions)) return true;
            var spl = regions.Split(',');
            foreach (var s in spl)
            {
                int result;
                if (!int.TryParse(s.Trim(), out result))
                {
                    return false;
                }
            }
            return true;
        }
    }

    public class PhrasesListValidator : AbstractValidator<string>
    {
        public PhrasesListValidator()
        {
            CascadeMode = CascadeMode.Continue;
            RuleFor(phrase => phrase).NotEmpty().WithMessage("Фраза не должна быть пустой");
            RuleFor(phrase => phrase).Matches("^[a-z0-9а-яіїєґўІЇЄҐЎёЁ\\+\\!\\. ]+$", System.Text.RegularExpressions.RegexOptions.Singleline).WithMessage("Фраза '{PropertyValue}' содержит недопустимые символы");
            RuleFor(phrase => phrase).MinimumLength(2).WithMessage("Фраза '{PropertyValue}' не может быть короче 2 символов");
            RuleFor(phrase => phrase).MaximumLength(200).WithMessage("Фраза '{PropertyValue}' не может быть длинее 200 символов '{PropertyValue}'");
            RuleFor(phrase => phrase).Must(WordStartsWithOperator).WithMessage("Неверное использование оператора во фразе '{PropertyValue}'");
        }

        private bool WordStartsWithOperator(string phrase)
        {
            if (phrase.Contains("!") || phrase.Contains("+") || phrase.Contains("-"))
            {
                var spl = Regex.Split(phrase, "\\s");
                foreach (var s in spl)
                {
                    if (s == "!" || s == "+" || s == "-")
                    {
                        return false;
                    }
                    if ((s.Contains("!") && !s.StartsWith("!")) || (s.Contains("+") && !s.StartsWith("+")) || (s.Contains("-") && !s.StartsWith("-")))
                    {
                        return false;
                    }
                }
            }
            return true;
        }
    }

    public class MinusWordsValidator : AbstractValidator<string>
    {
        public MinusWordsValidator()
        {
            CascadeMode = CascadeMode.Continue;
            RuleFor(minus_word => minus_word).NotEmpty().WithMessage("Минсу-слово не может быть пустым");
            RuleFor(minus_word => minus_word).Must(minus_word => !minus_word.Contains(" ")).WithMessage("Минус-слово '{PropertyValue}' не должно содержать пробел");
            RuleFor(minus_word => minus_word).Matches("^[a-z0-9а-яіїєґўІЇЄҐЎёЁ]+$", System.Text.RegularExpressions.RegexOptions.Singleline).WithMessage("Минус-слово '{PropertyValue}' содержит недопустимые символы");
            RuleFor(minus_word => minus_word).MinimumLength(2).WithMessage("Минус-слово '{PropertyValue}' не может быть короче 2 символов");
            RuleFor(minus_word => minus_word).MaximumLength(200).WithMessage("Минус-слово '{PropertyValue}' не может быть длиннее 200 символов");
        }
    }


}
