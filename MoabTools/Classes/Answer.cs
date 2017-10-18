using FluentValidation.Results;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoabTools
{

    public class Request
    {
        public Task task { get; set; }
        public string api_key { get; set; }

        public RequestAnswer Send()
        {
            if (Validate() != null)
            {
                throw (new Exception("Запрос не прошел валидацию"));
            }

            if (task.Validate() != null)
            {
                throw (new Exception("Переданное задание не прошло валидацию"));
            }

            ExtendedWebClient wc = new ExtendedWebClient(5000);

            string s;
            try
            {
                s = wc.UploadString("http://tools.moab.pro/api/Parse/AddTasks", this.ToString());
            }
            catch (Exception)
            {
                throw;
            }

            RequestAnswer answer;
            try
            {
                answer = JsonConvert.DeserializeObject<RequestAnswer>(s);
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
            RequestValidator validator = new RequestValidator();
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

    public class RequestAnswer
    {
        public int total_pages { get; set; }
        public List<int> ids { get; set; }
        public List<string> errors { get; set; }
    }
}
