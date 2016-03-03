using System;
using System.Web.Http;
using System.Collections.Generic;

namespace RallyCat.WebApi.Controllers
{
    public class ValuesController : ApiController
    {
        // GET api/values
        public IEnumerable<String> Get()
        {
            return new[]
            {
                "value1", "value2"
            };
        }

        // GET api/values/5
        public String Get(Int32 id)
        {
            return "value";
        }

        // POST api/values
        public void Post([FromBody] String value)
        {
        }

        // PUT api/values/5
        public void Put(Int32 id, [FromBody] String value)
        {
        }

        // DELETE api/values/5
        public void Delete(Int32 id)
        {
        }
    }
}