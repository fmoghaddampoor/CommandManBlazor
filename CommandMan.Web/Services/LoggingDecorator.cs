using System;
using System.Reflection;
using Microsoft.Extensions.Logging;
using Serilog.Context;

namespace CommandMan.Web.Services
{
    public class LoggingDecorator<T> : DispatchProxy
    {
        private T _decorated;
        private ILogger<T> _logger;

        protected override object? Invoke(MethodInfo? targetMethod, object?[]? args)
        {
            if (targetMethod == null)
            {
                return null;
            }

            using (LogContext.PushProperty("LogSource", "Backend")) 
            {
                try
                {
                    _logger.LogInformation("Entering {MethodName} with args: {@Args}", targetMethod.Name, args);
                    
                    var result = targetMethod.Invoke(_decorated, args);

                    if (result is System.Threading.Tasks.Task task)
                    {
                        task.ContinueWith(t => {
                            if (t.IsFaulted)
                            {
                                _logger.LogError(t.Exception, "Async error in {MethodName}: {Message}", targetMethod.Name, t.Exception?.InnerException?.Message ?? t.Exception?.Message);
                            }
                            else if (t.IsCompletedSuccessfully)
                            {
                                _logger.LogInformation("Exiting {MethodName} (Async) completed successfully", targetMethod.Name);
                            }
                        }, System.Threading.Tasks.TaskScheduler.Default);
                    }
                    else
                    {
                        _logger.LogInformation("Exiting {MethodName} with result: {@Result}", targetMethod.Name, result);
                    }
                    
                    return result;
                }
                catch (TargetInvocationException ex)
                {
                    _logger.LogError(ex.InnerException ?? ex, "Sync error in {MethodName}", targetMethod.Name);
                    throw ex.InnerException ?? ex;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Sync error in {MethodName}", targetMethod.Name);
                    throw;
                }
            }
        }

        public static T Create(T decorated, ILogger<T> logger)
        {
            object proxy = Create<T, LoggingDecorator<T>>();
            ((LoggingDecorator<T>)proxy).SetParameters(decorated, logger);
            return (T)proxy;
        }

        private void SetParameters(T decorated, ILogger<T> logger)
        {
            _decorated = decorated;
            _logger = logger;
        }
    }
}
