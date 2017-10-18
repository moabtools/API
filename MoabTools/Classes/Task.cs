using FluentValidation;
using FluentValidation.Results;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MoabTools
{
    public class Task
    {

        public Task()
        {
            phrases_list = new List<string>();
            regions = String.Empty;
            syntax = Syntax.NoQuotes;
            depth = 2;
            db = Db.All;
            group_id = null;
            also_suggests = true;
            also_check = true;
            fix_words_order = false;
            type = TaskType.WordstatDeep;
            minus_words = new List<string>();
            suggests_types = new List<SuggestType>(new SuggestType[] { SuggestType.PhraseAndSpace });
            suggests_depth = 1;
        }

        public List<string> phrases_list { get; set; }
        public string regions { get; set; }
        public Syntax syntax { get; set; }
        public int depth { get; set; }
        public Db db { get; set; }
        public Nullable<int> group_id { get; set; }
        public bool also_suggests { get; set; }
        public bool also_check { get; set; }
        public bool fix_words_order { get; set; }
        public TaskType type { get; set; }
        public List<string> minus_words { get; set; }
        public List<SuggestType> suggests_types { get; set; }
        public int suggests_depth { get; set; }

        public IList<ValidationFailure> Validate()
        {
            TaskValidator validator = new TaskValidator();
            ValidationResult results = validator.Validate(this);
            if (!results.IsValid)
            {
                return results.Errors;
            }
            else
            {
                return null;
            }
        }
    }

}
