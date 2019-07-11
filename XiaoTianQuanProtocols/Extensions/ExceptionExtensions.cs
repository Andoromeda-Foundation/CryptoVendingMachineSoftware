using System;
using System.Collections.Generic;
using System.Text;

namespace XiaoTianQuanProtocols.Extensions
{
    public static class ExceptionExtensions
    {
        public static string GetInnerMessages(this Exception e)
        {
            var sb = new StringBuilder();
            var exception = e;
            do
            {
                sb.AppendLine(exception.Message);
                exception = exception.InnerException;
            } while (exception != null);

            return sb.ToString();
        }
    }
}
