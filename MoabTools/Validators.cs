using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;
using System.Text.RegularExpressions;

namespace MoabTools
{


    public class RequestValidator : AbstractValidator<Request>
    {
        public RequestValidator()
        {
            RuleFor(req => req.task).NotNull();
            RuleFor(req => req.task).SetValidator(new TaskValidator());

            RuleFor(req => req.api_key).NotNull();
            RuleFor(req => req.api_key).Must(BeAValidGuid);
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
            RuleFor(chk => chk.id).GreaterThan(0);
            RuleFor(chk => chk.api_key).NotNull();
            RuleFor(chk => chk.api_key).Must(BeAValidGuid);

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

            RuleFor(task => task.phrases_list).NotEmpty();
            RuleFor(task => task.phrases_list).Must(list => list.Count > 0);
            RuleFor(task => task.phrases_list).Must(list => list.Count == 1).When(task => task.type == TaskType.WordstatDeep);
            RuleFor(task => task.phrases_list).Must(list => list.Count <= 10000).When(task => task.type == TaskType.DirectCheck || task.type == TaskType.Suggests);
            RuleFor(task => task.phrases_list).SetCollectionValidator(new PhrasesListValidator());

            RuleFor(task => task.regions).Must(BeValidRegion);

            RuleFor(task => task.syntax).IsInEnum();
            RuleFor(task => task.syntax).Equal(Syntax.NoQuotes).When(task => task.type == TaskType.WordstatDeep || task.type == TaskType.Suggests);

            RuleFor(task => task.depth).GreaterThanOrEqualTo(1).LessThanOrEqualTo(2);
            RuleFor(task => task.depth).Equal(1).When(task => task.type == TaskType.DirectCheck || task.type == TaskType.Suggests);

            RuleFor(task => task.db).IsInEnum();

            RuleFor(task => task.group_id).Null();

            RuleFor(task => task.fix_words_order).Equal(false);
            RuleFor(task => task.type).IsInEnum();

            RuleFor(task => task.also_suggests).Equal(false).When(task => task.type != TaskType.WordstatDeep);
            RuleFor(task => task.also_check).Equal(false).When(task => task.also_suggests == false);

            RuleFor(task => task.minus_words).Empty().When(task => task.type == TaskType.DirectCheck || task.type == TaskType.Suggests);
            RuleFor(task => task.minus_words).Must(list => list.Count <= 100).When(task => task.type == TaskType.WordstatDeep);
            RuleFor(task => task.minus_words).SetCollectionValidator(new MinusWordsValidator());
            RuleFor(task => task.minus_words).Must((t, l) => t.phrases_list.Except(l).ToList().Count != t.phrases_list.Count).When(task => task.minus_words.Count > 0);

            RuleFor(task => task.suggests_types).NotEmpty().When(task => (task.type == TaskType.WordstatDeep && task.also_suggests) || task.type == TaskType.Suggests);
            RuleFor(task => task.suggests_depth).GreaterThanOrEqualTo(1).When(task => (task.type == TaskType.WordstatDeep && task.also_suggests) || task.type == TaskType.Suggests);
            RuleFor(task => task.suggests_depth).LessThanOrEqualTo(3).When(task => (task.type == TaskType.WordstatDeep && task.also_suggests) || task.type == TaskType.Suggests);

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
            RuleFor(phrase => phrase).NotEmpty();
            RuleFor(phrase => phrase).Matches("^[a-z0-9а-яіїєґўІЇЄҐЎёЁ\\+\\!\\. ]+$", System.Text.RegularExpressions.RegexOptions.Singleline);
            RuleFor(phrase => phrase).MinimumLength(2).MaximumLength(200);
            RuleFor(phrase => phrase).Must(WordStartsWithOperator);
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
            RuleFor(minus_word => minus_word).NotEmpty();
            RuleFor(minus_word => minus_word).Must(minus_word => !minus_word.Contains(" "));
            RuleFor(minus_word => minus_word).Matches("^[a-z0-9а-яіїєґўІЇЄҐЎёЁ]+$", System.Text.RegularExpressions.RegexOptions.Singleline);
            RuleFor(minus_word => minus_word).MinimumLength(2).MaximumLength(200);
        }
    }

}
