using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace Two10.AzureTraceListener
{
    public class TableTraceListener : TraceListener
    {
        readonly ConcurrentQueue<TraceEntity> queue;
        readonly Timer timer;
        CloudTable table;
        readonly string identity;

        protected override string[] GetSupportedAttributes()
        {
            return new[] { "connectionString", "tableName" };
        }

        public TableTraceListener()
        {
            queue = new ConcurrentQueue<TraceEntity>();
            identity = System.Environment.MachineName;
            var flushInterval = 60 * 1000;
            timer = new Timer((_) => Flush(), null, flushInterval, flushInterval);
        }

        protected override void Dispose(bool disposing)
        {
            timer.Dispose();
            base.Dispose(disposing);
        }

        public override bool IsThreadSafe
        {
            get
            {
                return true;
            }
        }

        CloudTable Table
        {
            get
            {
                if (null != table) return table;
                var account = CloudStorageAccount.Parse(this.Attributes["connectionString"]);
                var tableClient = account.CreateCloudTableClient();
                var tableName = this.Attributes["tableName"] ?? "trace";
                table = tableClient.GetTableReference(tableName);
                table.CreateIfNotExists();
                return table;
            }

        }

        public override void Write(string message)
        {
            this.Push(message);
        }

        public override void WriteLine(string message)
        {
            this.Push(message);
        }

        public override void WriteLine(string message, string category)
        {
            this.Push(message, category);
        }

        void Push(string message, string category = null)
        {
            var now = DateTime.UtcNow;
            var entity = new TraceEntity
            {
                PartitionKey = string.Format("{0}-{1:yyyy-MM-dd}", identity, now),
                RowKey = (DateTime.MaxValue.Ticks - now.Ticks).ToString("d19"),
                Value = message,
                Category = category
            };
            Console.WriteLine(entity.PartitionKey + " " + entity.RowKey);

            this.queue.Enqueue(entity);
            if (this.queue.Count > 75) Flush();
        }

        public override void Flush()
        {
            while (this.queue.Count > 0)
            {
                this.Send(GetBatch().Take(100));
            }
            base.Flush();
        }

        IEnumerable<TraceEntity> GetBatch()
        {
            TraceEntity entity;
            while (this.queue.TryDequeue(out entity))
            {
                yield return entity;
            }
        }

        void Send(IEnumerable<TraceEntity> entities)
        {
            try
            {
                var batch = new TableBatchOperation();
                foreach (var entity in entities)
                {
                    batch.InsertOrReplace(entity);
                }
                this.Table.ExecuteBatch(batch);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }
}
