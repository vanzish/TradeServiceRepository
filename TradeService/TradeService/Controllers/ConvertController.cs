using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using NLog;
using TradeService.Models;

namespace TradeService
{
    public class ConvertController : ApiController
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
        private Entities db = new Entities();

        [HttpGet]
        [ResponseType(typeof(Url))]
        public async Task<IHttpActionResult> CSV(string fileName)
        {
            Log.Info("Request for {0}", Request.RequestUri);
            string url;
            var csvFileName = Path.ChangeExtension(fileName, "csv");
            try
            {
                TradeRecordsWithHeader tradeRecordsWithHeader = await FileHelper.ReadBinaryFile(fileName);
                var csv = FileHelper.GenerateCSV(tradeRecordsWithHeader);
                url = FileHelper.CreateFileAndUrl(csvFileName, csv);
            }
            catch(FileNotFoundException ex)
            {
                Log.Warn("Exception message {0}. Stack Trace: {1}", ex.Message, ex.StackTrace);
                return BadRequest(ex.Message);
            }
            catch(Exception)
            {
                return InternalServerError();
            }

            var tradeFile = new TradeFile {Name = csvFileName, URL = url};
            db.TradeFiles.Add(tradeFile);
            await db.SaveChangesAsync();
            return Ok(new Url {UrlAddress = "api/download/" + url});
        }

        [HttpGet]
        public async Task<IHttpActionResult> DB(string fileName)
        {
            Log.Info("Request for {0}", Request.RequestUri);
            using(var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    TradeRecordsWithHeader tradeRecordsWithHeader = await FileHelper.ReadBinaryFile(fileName);
                    foreach(var tradeRecord in tradeRecordsWithHeader.TradeRecords)
                    {
                        db.TradeRecords.Add(tradeRecord);
                    }

                    await db.SaveChangesAsync();
                    transaction.Commit();
                }
                catch(FileNotFoundException ex)
                {
                    Log.Warn("Exception message {0}. Stack Trace: {1}", ex.Message, ex.StackTrace);
                    return BadRequest(ex.Message);
                }
                catch(DbUpdateException ex)
                {
                    transaction.Rollback();
                    Log.Error("Exception message {0}. Stack Trace: {1}", ex.Message, ex.StackTrace);
                    return InternalServerError();
                }
            }
            return Ok();
        }

        protected override void Dispose(bool disposing)
        {
            if(disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);

        }
    }
}
