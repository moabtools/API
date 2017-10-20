using FluentValidation.Results;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoabTools
{
    public class TasksList
    {
        public Nullable<int> page { get; set; }
        public TaskType type { get; set; }
        public string api_key { get; set; }
        
        public IList<ValidationFailure> Validate()
        {
            TasksListValidator validator = new TasksListValidator();
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

        public TasksListAnswer List()
        {

            if (Validate() != null)
            {
                throw (new Exception("Запрос не прошел валидацию"));
            }

            ExtendedWebClient wc = new ExtendedWebClient(5000);

            string s;
            try
            {
                s = wc.UploadString("http://tools.moab.pro/api/Parse/TasksList", this.ToString());
            }
            catch (Exception)
            {
                throw;
            }

            TasksListAnswer answer;
            try
            {
                answer = JsonConvert.DeserializeObject<TasksListAnswer>(JToken.Parse(s).ToString());
            }
            catch (Exception)
            {
                throw;
            }

            return answer;

        }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }

    }

    public class TasksListAnswer
    {
        public List<Task> tasks_list { get; set; }
        public int total_pages { get; set; }
    }
}
