using System;
using Rebel.Framework.Localization;

namespace Rebel.Framework
{
    [Serializable]
    public class LocalizedNotImplementedException : NotImplementedException
    {        
        public ExceptionHelper Localization { get; private set; }

        public LocalizedNotImplementedException(string key = "Exceptions.NotImplementedException", string defaultMessage = null, object parameters = null, Exception innerException = null)
            : base(defaultMessage, innerException)
        {
            Localization = new ExceptionHelper(this, key, defaultMessage, parameters);
        }

        public override string Message { get { return Localization.GetMessage(base.Message); } }
    }
}