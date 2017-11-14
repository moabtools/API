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
    public class RunPauseTask
    {
        public int task_id { get; set; }
        public string api_key { get; set; }

        public RequestAnswer Send(RunPauseEnum action)
        {
            if (Validate() != null)
            {
                throw (new Exception("Запрос не прошел валидацию"));
            }

            ExtendedWebClient wc = new ExtendedWebClient(5000);

            string s = "";
            try
            {
                if (action == RunPauseEnum.Pause)
                {
                    s = wc.UploadString("http://tools.moab.pro/api/Parse/PauseTask", this.ToString());
                } else if ( action == RunPauseEnum.Run )
                {
                    s = wc.UploadString("http://tools.moab.pro/api/Parse/RunTask", this.ToString());
                }
            }
            catch (Exception)
            {
                throw;
            }

            if(string.IsNullOrWhiteSpace(s))
            {
                // вернулась пустая строка - всё в порядке
                return null;
            }

            RequestAnswer answer;
            try
            {
                answer = JsonConvert.DeserializeObject<RequestAnswer>(JToken.Parse(s).ToString());
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

        public IList<ValidationFailure> Validate()
        {
            RunPauseValidator validator = new RunPauseValidator();
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

    public enum RunPauseEnum
    {
        Run,
        Pause
    }

}
