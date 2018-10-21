using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MQTTServer.Core
{
    internal class EmbeddedFileMiddleware
    {
        private string _indexPath;
        private string _resourcePath;
        private RequestDelegate _nextDelegate;

        public EmbeddedFileMiddleware(RequestDelegate nextDelegate, string indexPath, string resourcePath)
        {
            _indexPath = indexPath;
            _resourcePath = resourcePath;
            _nextDelegate = nextDelegate;
        }

        public async Task Invoke(HttpContext context)
        {
            if (context.Request.Path.HasValue && context.Request.Path.Value.Trim('/').StartsWith(_indexPath.Trim('/'), StringComparison.OrdinalIgnoreCase))
            {
                var stream = typeof(EmbeddedFileMiddleware).GetType().Assembly.GetManifestResourceStream(_resourcePath);
                using (var reader = new StreamReader(stream))
                {
                    context.Response.Headers["Content-Type"] = "text/html; charset=utf-8";
                    context.Response.StatusCode = 200;
                    await context.Response.WriteAsync(await reader.ReadToEndAsync());
                }
            }
            else
                await _nextDelegate(context);
        }

    }
}
