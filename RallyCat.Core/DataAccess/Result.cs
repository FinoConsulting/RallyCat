using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RallyCat.Core.DataAccess
{
    public class Result <T>
    {
        public Boolean Success      { get; set; }
        public String  ErrorMessage { get; set; }
        public T       Object       { get; set; }
    }
}
