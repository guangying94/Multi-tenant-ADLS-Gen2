using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Configuration;
using System.Collections.Generic;
using System.Linq;

namespace ADLSAccess
{
    public static class CheckAccess
    {
        [FunctionName("CheckAccess")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "check/{userId}")] HttpRequest req,
            [CosmosDB(
                databaseName:"access_db",
                containerName: "access_list",
                Connection = "CosmosDBConnection",
                SqlQuery = "SELECT c.userId, c.allowedFolder FROM c WHERE c.userId = {userId}")]
                IEnumerable<UserAccessobject> accessObjects,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string folder = req.Query["folder"];

            if (folder != null)
            {
                bool folderCanAccess = false;

                foreach (UserAccessobject accessobject in accessObjects)
                {
                    if (accessobject.allowedFolder.Contains(folder))
                    {
                        folderCanAccess = true;
                    }
                }

                AccessResult accessResult = new AccessResult()
                {
                    UserId = accessObjects.FirstOrDefault().userId,
                    Folder = folder,
                    canAccess = folderCanAccess
                };
                return new OkObjectResult(accessResult);
            }
            else
            {
                return new NotFoundObjectResult("Please put in folder as query in this request!");
            }
        }
    }

    public class AccessResult
    {
        public string UserId { get; set; }
        public string Folder { get; set; }
        public bool canAccess { get; set; }
    }

    public class UserAccessobject
    {
        public string userId { get; set; }
        public string[] allowedFolder { get; set; }
    }
}
