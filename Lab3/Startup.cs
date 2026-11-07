using System;
using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Lab3
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddRouting();
        }
        public void Configure(IApplicationBuilder app, ILoggerFactory loggerFactory)
        {
            string logsFolder = Path.Combine(Directory.GetCurrentDirectory(), "Logs");

            if (!Directory.Exists(logsFolder))
            {
                Directory.CreateDirectory(logsFolder);
            }

            string logFilePath = Path.Combine(logsFolder, "log.txt");

            loggerFactory.AddProvider(new FileLoggerProvider(logFilePath));
            var logger = loggerFactory.CreateLogger("FileLogger");

            app.Use(async (context, next) =>
            {
                string fullUrl = $"{context.Request.Scheme}://{context.Request.Host}{context.Request.Path}{context.Request.QueryString}";
                string ip = context.Connection.RemoteIpAddress?.ToString() ?? "Невідомий IP";
                string time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                logger.LogInformation($"Час: {time} | IP: {ip} | URL: {fullUrl}");

                await next.Invoke();
            });

            app.Run(async (context) =>
            {
                context.Response.ContentType = "text/html; charset=utf-8";
                await context.Response.WriteAsync("Hello World! Логи успішно записуються у папку Logs.");
            });
        }
    }

    public class FileLogger : ILogger
    {
        private readonly string _filePath;
        private static readonly object _lock = new object();

        public FileLogger(string path)
        {
            _filePath = path;
        }

        public IDisposable BeginScope<TState>(TState state) => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (formatter != null)
            {
                lock (_lock)
                {
                    File.AppendAllText(_filePath, formatter(state, exception) + Environment.NewLine);
                }
            }
        }
    }

    public class FileLoggerProvider : ILoggerProvider
    {
        private readonly string _path;

        public FileLoggerProvider(string path)
        {
            _path = path;
        }

        public ILogger CreateLogger(string categoryName) => new FileLogger(_path);

        public void Dispose() { }
    }
}