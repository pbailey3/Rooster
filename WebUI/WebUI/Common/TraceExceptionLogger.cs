using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.Http.ExceptionHandling;
using System.Web.Http.Filters;
using Microsoft.ApplicationInsights;

namespace WebUI.Common
{
    class TraceExceptionLogger : ExceptionLogger
    {
        public override void LogCore(ExceptionLoggerContext context)
        {
            if (context != null && context.Exception != null)
            {//or reuse instance (recommended!). see note above 
                var ai = new TelemetryClient();
                ai.TrackException(context.Exception);
                Trace.TraceError(context.ExceptionContext.Exception.ToString());
            }
           
        }
    }
}