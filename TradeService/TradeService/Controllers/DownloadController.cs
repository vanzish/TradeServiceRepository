using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using NLog;

namespace TradeService.Controllers
{
    public class DownloadController : ApiController
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
        private Entities db = new Entities();
        [HttpGet]
        public async Task<IHttpActionResult> Download(string url)
        {
            Log.Info("Request for {0}", Request.RequestUri);
            using(var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    Guid guidFromUrl;
                    if(!Guid.TryParse(url, out guidFromUrl))
                    {
                        Log.Warn("Wrong request format. Wrong Guid: {0}", url);
                        return BadRequest("Wrong format of last Url parameter");
                    }

                    var tradeFile = db.TradeFiles.AsNoTracking().FirstOrDefault(f => f.URL.Equals(url));

                    if(tradeFile == null)
                    {
                        Log.Warn("File by such Guid doesn't exist anymore.");
                        return BadRequest("File by such Url doesn't exist anymore");
                    }

                    FileHttpResponseMessage result = GetFileResponse(tradeFile.Name);
                    var res = ResponseMessage(result);

                    db.TradeFiles.Attach(tradeFile);
                    db.Entry(tradeFile).State = EntityState.Deleted;
                    await db.SaveChangesAsync();
                    transaction.Commit();

                    return res;
                }
                catch(Exception ex)
                {
                    transaction.Rollback();
                    Log.Error("Exception message {0}. Stack Trace: {1}", ex.Message, ex.StackTrace);
                    return ResponseMessage(new HttpResponseMessage(HttpStatusCode.InternalServerError));
                }
            }
        }

        private FileHttpResponseMessage GetFileResponse(string fileName)
        {
            FileHttpResponseMessage result = new FileHttpResponseMessage(fileName);
            result.StatusCode = HttpStatusCode.OK;
            var stream = new FileStream(fileName, FileMode.Open);
            result.Content = new StreamContent(stream);
            result.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment");
            result.Content.Headers.ContentDisposition.FileName = Path.GetFileName(fileName);
            result.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
            result.Content.Headers.ContentLength = stream.Length;
            return result;
        }

        protected override void Dispose(bool disposing)
        {
            if(disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);

        }

        public class FileHttpResponseMessage : HttpResponseMessage
        {
            private string filePath;

            public FileHttpResponseMessage(string filePath)
            {
                this.filePath = filePath;
            }

            protected override void Dispose(bool disposing)
            {
                base.Dispose(disposing);

                Content.Dispose();

                File.Delete(filePath);
                Log.Info("File '{0}' was deleted.", filePath);
            }
        }
    }
}
