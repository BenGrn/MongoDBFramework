using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MongoDBFramework.Exceptions
{
    public class MongoNotFoundException : ApplicationException
    {
        public MongoNotFoundException(string key, object value) : base($"{key} ({value}) was not found")
        {

        }
    }
}
