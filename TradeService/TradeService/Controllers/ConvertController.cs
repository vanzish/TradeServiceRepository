using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using NLog;
using TradeService.Models;

namespace TradeService
{
    public class ConvertController : ApiController
    {
        private readonly int numberOfBulkSave = 100;
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
        private Entities db;

        [HttpGet]
        [ResponseType(typeof(Url))]
        public async Task<IHttpActionResult> CSV(string fileName)
        {
            Log.Info("Request for {0}", Request.RequestUri);
            var csvFileName = Path.ChangeExtension(fileName, "csv");
            string url = Guid.NewGuid().ToString();

            using(db = new Entities())
            {
                try
                {
                    await Task.Run(() =>
                    {
                        FileHelper.ExecuteProcessCSV(fileName, csvFileName);
                        //Thread.Sleep(3000);
                    }).ConfigureAwait(false);
                }
                catch(FileNotFoundException ex)
                {
                    Log.Warn("Exception message {0}. Stack Trace: {1}", ex.Message, ex.StackTrace);
                    return BadRequest(ex.Message);
                }
                catch(Exception ex)
                {
                    File.Delete(csvFileName);
                    Log.Error("Exception message {0}. Stack Trace: {1}", ex.Message, ex.StackTrace);
                    return InternalServerError(ex);
                }

                var tradeFile = new TradeFile { Name = csvFileName, URL = url };
                db.TradeFiles.Add(tradeFile);
                db.SaveChanges();
            }

            return Ok(new Url { UrlAddress = "api/download/" + url });
        }

        [HttpGet]
        public async Task<IHttpActionResult> DB(string fileName)
        {
            Log.Info("Request for {0}", Request.RequestUri);
            db = new Entities();
            using(var transaction = db.Database.BeginTransaction())
            using(db)
            {
                db.Configuration.AutoDetectChangesEnabled = false;
                try
                {
                    await Task.Run(() =>
                    {
                        StructHelper structHelper = new StructHelper(fileName);
                        structHelper.Open(FileAccess.Read, FileShare.None);

                        var header = (Header)structHelper.GetNextStructureValue(typeof(Header));

                        int count = 0;
                        object record;
                        while((record = structHelper.GetNextStructureValue(typeof(StructModels.TradeRecord))) != null)
                        {
                            var rec = (StructModels.TradeRecord)record;
                            var recObj = new TradeRecord
                            {
                                Id = rec.id,
                                FileName = fileName,
                                Account = rec.account,
                                Volume = rec.volume,
                                Comment = rec.comment,
                                Version = header.version,
                                Type = header.type
                            };

                            ++count;
                            db = AddToContext(db, recObj, count, numberOfBulkSave, true);
                        }

                        //Thread.Sleep(3000);
                    }).ConfigureAwait(false);

                    db.SaveChanges();
                    transaction.Commit();
                }
                catch(FileNotFoundException ex)
                {
                    transaction.Rollback();
                    Log.Warn("Exception message {0}. Stack Trace: {1}", ex.Message, ex.StackTrace);
                    return BadRequest(ex.Message);
                }
                catch(DbUpdateException ex)
                {
                    transaction.Rollback();
                    Log.Error("Exception message {0}. Stack Trace: {1}", ex.Message, ex.StackTrace);
                    return InternalServerError(ex);
                }
            }
            return Ok();
        }

        private Entities AddToContext(Entities context, TradeRecord entity, int count, int commitCount, bool recreateContext)
        {
            context.Set<TradeRecord>().Add(entity);

            if(count % commitCount == 0)
            {
                context.SaveChanges();
                if(recreateContext)
                {
                    context.Dispose();
                    context = new Entities();
                    context.Configuration.AutoDetectChangesEnabled = false;
                }
            }

            return context;
        }

        protected override void Dispose(bool disposing)
        {
            if(disposing)
            {
                db?.Dispose();
            }
            base.Dispose(disposing);

        }
    }
}
