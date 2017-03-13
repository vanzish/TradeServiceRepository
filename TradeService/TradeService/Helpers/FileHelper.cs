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
            structHelper.Open(FileMode.Create, FileAccess.ReadWrite, FileShare.None);
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

        public static async Task<TradeRecordsWithHeader> ReadBinaryFile(string fileName)
        {
            TradeRecordsWithHeader tradeRecordsWithHeader = new TradeRecordsWithHeader();
            StructHelper structHelper = new StructHelper(fileName);
            if(TryOpen(fileName, 3))
            {
                structHelper.Open(FileMode.Open, FileAccess.Read, FileShare.None);
            }
            else
            {
                throw new FileNotFoundException(string.Format("File {0} was not found or not accessible", fileName));
            }

            var header = (Header)structHelper.GetNextStructureValue(typeof(Header));
            tradeRecordsWithHeader.Header = header;
            var tradeRecords = new List<object>();

            await Task.Run(() =>
            {
                tradeRecords = structHelper.GetAllStrucureValues(typeof(StructModels.TradeRecord));
            }).ConfigureAwait(false);

            foreach(var record in tradeRecords)
            {
                if(record is StructModels.TradeRecord)
                {
                    var recStruct = (StructModels.TradeRecord)record;
                    var recObj = new TradeRecord
                    {
                        Id = recStruct.id,
                        FileName = fileName,
                        Account = recStruct.account,
                        Volume = recStruct.volume,
                        Comment = recStruct.comment,
                        Version = header.version,
                        Type = header.type
                    };
                    tradeRecordsWithHeader.TradeRecords.Add(recObj);
                }
            }

            return tradeRecordsWithHeader;
        }

        public static string GenerateCSV(TradeRecordsWithHeader tradeRecordsWithHeader)
        {
            var csv = new StringBuilder();
            csv.AppendLine(string.Format("{0},{1}", tradeRecordsWithHeader.Header.version,
                tradeRecordsWithHeader.Header.type));
            foreach(var tradeRecord in tradeRecordsWithHeader.TradeRecords)
            {
                csv.AppendLine(string.Format("{0},{1},{2},{3}", tradeRecord.Id, tradeRecord.Account, tradeRecord.Volume,
                    tradeRecord.Comment));
            }

            return csv.ToString();
        }

        public static string CreateFileAndUrl(string filename, string csv)
        {
            File.WriteAllText(filename, csv);
            return Guid.NewGuid().ToString();
        }
    }
}
