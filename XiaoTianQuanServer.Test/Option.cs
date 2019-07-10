using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Options;

namespace XiaoTianQuanServer.Test
{
    class Option<T> : IOptions<T> where T : class, new()
    {
        public Option(T t)
        {
            Value = t;
        }

        public T Value { get; }
    }
}
