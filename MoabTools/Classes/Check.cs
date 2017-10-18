using FluentValidation.Results;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoabTools
{

    public class Check
    {
        public int id { get; set; }
        public string api_key { get; set; }

        public CheckAnswer Send()
        {
            if (Validate() != null)
            {
                throw (new Exception("Запрос не прошел валидацию"));
            }

            ExtendedWebClient wc = new ExtendedWebClient(5000);

            string s;
            try
            {
                s = wc.UploadString("http://tools.moab.pro/api/Parse/Check", this.ToString());
            }
            catch (Exception)
            {
                throw;
            }

            CheckAnswer answer;
            try
            {
                answer = JsonConvert.DeserializeObject<CheckAnswer>(s);
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
            CheckValidator validator = new CheckValidator();
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


    public class CheckAnswer
    {
        public int status { get; set; }
        public string download_zip { get; set; }
    }
}
