using System;
using System.Net;
using System.Threading.Tasks;
using MaxRev.Servers.Core.Http;
using MaxRev.Servers.Core.Modules.Parts;
using MaxRev.Servers.Interfaces;
using MaxRev.Utils.Methods;

namespace VstupInfoParser
{
    internal class RequestInfoLogModule_Sample : AbstractModule
    {
        protected override Task<IResponseInfo> InvokeAsync(ModuleContext context)
        {
            var hr = (HttpRequest)context.HttpRequest;
            var q = context.FileSystemContext.Query;
            Console.WriteLine($"New request: {context.Client.ConnectionId}\n" +
                              $"required syspath: {context.FileSystemContext.SysPath}\n" +
                              $"method: {hr.Method}\n" +
                              $"isApi: {context.FileSystemContext.IsApi}\n" +
                              $"isCancelled: {context.FileSystemContext.Cancellation.IsCancellationRequested}\n" +
                              $"path: {WebUtility.UrlDecode(hr.Path)}\n" + // don't forget to decode url)
                              $"path without query: {WebUtility.UrlDecode(q.RequestWithoutQuery)}\n" +
                              $"length: {hr.ContentLength}\n");

            // do other things ...

            // non default result is returned to user
            // but by default it's task with null ref result

            // default server modules are 
            /* - Request Core Handler + Logger || File Handler
               - Authorization 
               - Url Shortener
               - Redirect Module  
               - File Uploader Handler
               - Api Handling
               - File Handling
             */

            return Default;
        }
    }
}