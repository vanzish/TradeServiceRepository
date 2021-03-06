﻿using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using NLog;

namespace TradeService.Controllers
{
    public class TradeRecordController : ApiController
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
        private Entities db = new Entities();

        [ResponseType(typeof(TradeRecord))]
        public async Task<IHttpActionResult> GetTradeRecord(int id, string filename)
        {
            Log.Info("Request for {0}", Request.RequestUri);

            TradeRecord tradeRecord =
                await db.TradeRecords.FirstOrDefaultAsync(r => (r.Id == id && string.Equals(r.FileName, filename)));
            if(tradeRecord == null)
            {
                var message = string.Format("Nothing comlies with such record Id and file name. Id: {0}. File Name: {1}.", id, filename);
                Log.Warn(message);
                return BadRequest(message);
            }

            return Ok(tradeRecord);
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
