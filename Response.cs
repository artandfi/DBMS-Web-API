using System.Collections.Generic;

namespace DBMS_Web_API {
    public class Response<T> {
        public T Value { get; set; }

        public IDictionary<string, string> Links { get; set; }
    }
}
