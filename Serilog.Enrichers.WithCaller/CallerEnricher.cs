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

        public CallerEnricher(bool? includeFileInfo, int maxDepth)
        {
            _includeFileInfo = includeFileInfo ?? false;
            _maxDepth = Math.Max(1, maxDepth);
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
                if (method.DeclaringType.Assembly == typeof(Log).Assembly)
                {
                    skipFrames++;
                    continue;
                }
                //else if (method.DeclaringType.Assembly != typeof(Log).Assembly)

                if (foundFrames > 0)
                {
                    caller.Append(" at ");
                }

                caller.Append($"{method.DeclaringType.FullName}.{method.Name}({GetParameterFullNames(method.GetParameters())})");

                if (stack.GetFileName() is string fileName)
                {
                    caller.Append($" {fileName}:{stack.GetFileLineNumber()}");
                }

                foundFrames++;

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
