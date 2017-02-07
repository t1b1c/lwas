using System;
using LWAS.Extensible.Exceptions;
using LWAS.Extensible.Interfaces.Monitoring;

namespace LWAS.WebParts
{
    public class GenericWebPart : BindableWebPart, IReporter
    {
        public IMonitor Monitor { get; set; }

        public override void Initialize()
        {
            if (null == this.Configuration) throw new MissingProviderException("Configuration");
            if (null == this.ConfigurationParser) throw new MissingProviderException("Configuration parser");
            if (null == this.Binder) throw new MissingProviderException("Binder");
            if (null == base.ValidationManager) throw new MissingProviderException("ValidationManager");
            if (null == base.ExpressionsManager) throw new MissingProviderException("ExpressionsManager");
            if (null == this.Monitor) throw new MissingProviderException("Monitor");

            base.Initialize();
        }
    }
}
