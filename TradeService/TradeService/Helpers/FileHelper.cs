using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TradeService.Models;

namespace TradeService
{
    public static class FileHelper
    {
        public static bool TryOpen(string fileName, int tryCount)
        {
            if(!File.Exists(fileName)) return false;
            for(int i = 0; i < tryCount; i++)
            {
                try
                {
                    var file = File.Open(fileName, FileMode.Open, FileAccess.Read, FileShare.None);
                    file.Close();

                    return true;
                }
                catch(IOException ex)
                {
                    Thread.Sleep(2000);
                }
            }

            return false;
        }

        public static void CreateBinaryFile(string fileName)
        {
            StructHelper structHelper = new StructHelper(fileName);
            structHelper.Create(FileAccess.ReadWrite, FileShare.None);
            var collectionOfStruct = new List<object>();
            var header = new Header { version = 1, type = "type1" };
            collectionOfStruct.Add(header);
            var tradeRecord = new StructModels.TradeRecord { id = 1, volume = 12.1, account = 2, comment = "comment" };
            collectionOfStruct.Add(tradeRecord);
            var tradeRecord1 = new StructModels.TradeRecord { id = 2, volume = 12.1, account = 2, comment = "comment" };
            collectionOfStruct.Add(tradeRecord1);
            structHelper.WriteStructureCollection(collectionOfStruct);
            structHelper.Close();
        }

        public static void ExecuteProcessCSV(string fileName, string csvFileName)
        {
            StructHelper structHelper = new StructHelper(fileName);
            structHelper.Open(FileAccess.Read, FileShare.None);

            using(StreamWriter output = new StreamWriter(csvFileName))
            {
                var header = structHelper.GetNextStructureValue(typeof(Header));
                if(header is Header)
                {
                    var head = (Header)header;
                    output.WriteLine(string.Format("{0},{1}", head.version, head.type));
                }

                object record;
                while((record = structHelper.GetNextStructureValue(typeof(StructModels.TradeRecord))) != null)
                {
                    var rec = (StructModels.TradeRecord)record;
                    output.WriteLine(string.Format("{0},{1},{2},{3}", rec.id, rec.account, rec.volume, rec.comment));
                }
            }
        }
    }
}
