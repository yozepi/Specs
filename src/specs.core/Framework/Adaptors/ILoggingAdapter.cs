using System;

namespace specs.core.Framework.Adaptors
{
    public interface ILoggingAdapter
    {
        void Log(string message);

        void LogAssertFailure(Exception exception);

        void LogAssertSuccess(string message);

        void LogException(Exception exception);
    }
}
