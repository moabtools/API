using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace MoabTools
{
    class ExtendedWebClient : WebClient
    {
        private int _timeout;

        public ExtendedWebClient(int timeout)
        {
            this.Encoding = Encoding.UTF8;
            this._timeout = timeout;
        }
        protected override WebRequest GetWebRequest(Uri address)
        {
            WebRequest wr = base.GetWebRequest(address);
            wr.Timeout = _timeout;
            wr.ContentType = "application/json";
            return wr;
        }
    }
}
