using System;

namespace Rebel.Cms.Web
{
    /// <summary>
    /// Represents the result of a secured method execution with return value.
    /// </summary>
    /// <typeparam name="TResultType">The type of the result type.</typeparam>
    public class SecuredMethodResult<TResultType> : SecuredMethodResult
    {
        public static readonly SecuredMethodResult<TResultType> False = new SecuredMethodResult<TResultType>();

        private SecuredMethodResult()
            : base(false, false)
        {

        }

        public SecuredMethodResult(bool authorized, bool success, TResultType result)
            : base(authorized, success)
        {
            Result = result;
        }

        public SecuredMethodResult(Exception error)
            : base(error)
        { }

        public TResultType Result { get; protected set; }
    }

    /// <summary>
    /// Represents the result of a secured method execution.
    /// </summary>
    public class SecuredMethodResult
    {
        public static readonly SecuredMethodResult False = new SecuredMethodResult();

        private SecuredMethodResult()
            : this(false, false)
        { }

        public SecuredMethodResult(bool authorized, bool success)
        {
            Authorized = authorized;
            Success = success;
        }

        public SecuredMethodResult(Exception executionError)
            : this(true, false)
        {
            ExecutionError = executionError;
        }

        public bool Authorized { get; protected set; }
        public bool Success { get; protected set; }
        public Exception ExecutionError { get; protected set; }
    }
}