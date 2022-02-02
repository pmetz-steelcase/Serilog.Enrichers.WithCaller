using System;
using System.Reflection;
using System.Diagnostics;

using Serilog.Core;
using Serilog.Events;
using System.Text;

namespace Serilog.Enrichers.WithCaller
{
    public class CallerEnricher : ILogEventEnricher
    {
        private readonly bool _includeFileInfo;
        private readonly int _maxDepth;
        private Predicate<MethodBase> _filter;

        public CallerEnricher()
            : this(false, 1)
        {
            // added default constructor again so one can use the generic Enrich.With<CallerEnricher>() method
        }

        public CallerEnricher(bool? includeFileInfo, int maxDepth)
            : this(includeFileInfo, maxDepth, method => method.DeclaringType.Assembly == typeof(Log).Assembly)
        {
        }

        public CallerEnricher(bool? includeFileInfo, int maxDepth, Predicate<MethodBase> filter)
        {
            _includeFileInfo = includeFileInfo ?? false;    // Ignored - adjust outputTemplate accordingly
            _maxDepth = Math.Max(1, maxDepth);
            _filter = filter;
        }

        public static int SkipFramesCount { get; set; } = 3;
        public static int MaxFrameCount { get; set; } = 128;

        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            int foundFrames = 0;
            StringBuilder caller = new StringBuilder();

            int skipFrames = SkipFramesCount;
            while (skipFrames < MaxFrameCount)
            {
                StackFrame stack = new StackFrame(skipFrames, _includeFileInfo);
                if (!stack.HasMethod())
                {
                    logEvent.AddPropertyIfAbsent(new LogEventProperty("Caller", new ScalarValue("<unknown method>")));
                    return;
                }

                MethodBase method = stack.GetMethod();

                if (_filter(method))
                {
                    skipFrames++;
                    continue;
                }

                if (foundFrames > 0)
                {
                    caller.Append(" at ");
                }

                var callerType = $"{method.DeclaringType.FullName}";
                var callerMethod = $"{method.Name}";
                if (!(stack.GetFileName() is string callerFileName))
                {
                    callerFileName = "";
                }

                var callerLineNo = stack.GetFileLineNumber();
                var callerParameters = GetParameterFullNames(method.GetParameters());

                caller.Append($"{callerType}.{callerMethod}({callerParameters})");
                if (!string.IsNullOrEmpty(callerFileName))
                {
                    caller.Append($" {callerFileName}:{callerLineNo}");
                }

                foundFrames++;

                if (foundFrames == 1)
                {
                    logEvent.AddPropertyIfAbsent(new LogEventProperty("CallerType", new ScalarValue(callerType)));
                    logEvent.AddPropertyIfAbsent(new LogEventProperty("CallerMethod", new ScalarValue(callerMethod)));
                    logEvent.AddPropertyIfAbsent(new LogEventProperty("CallerParameters", new ScalarValue(callerParameters)));
                    logEvent.AddPropertyIfAbsent(new LogEventProperty("CallerFileName", new ScalarValue(callerFileName)));
                    logEvent.AddPropertyIfAbsent(new LogEventProperty("CallerLineNo", new ScalarValue(callerLineNo)));
                }

                if (_maxDepth <= foundFrames)
                {
                    logEvent.AddPropertyIfAbsent(new LogEventProperty("Caller", new ScalarValue(caller.ToString())));
                    return;
                }

                skipFrames++;
            }
        }

        private string GetParameterFullNames(ParameterInfo[] parameterInfos, string separator = ", ")
        {
            int len = parameterInfos?.Length ?? 0;
            var sb = new StringBuilder();
            for (int i = 0; i < len; i++)
            {
                sb.Append(parameterInfos[i].ParameterType.FullName);
                if (i < len - 1)
                    sb.Append(separator);
            }
            return sb.ToString();
        }
    }
}
