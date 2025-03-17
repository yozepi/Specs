using System;

namespace specs.core.Framework.Adapters
{
    public interface ILoggingAdapter
    {
        void Log(string message);

        void LogAssertFailure(Exception exception, string message);

        void LogAssertSuccess(string message);

        void LogException(Exception exception);
    }
}
