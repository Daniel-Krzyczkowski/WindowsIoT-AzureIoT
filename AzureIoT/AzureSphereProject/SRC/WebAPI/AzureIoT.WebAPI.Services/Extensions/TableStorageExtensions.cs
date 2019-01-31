using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AzureIoT.WebAPI.Services.Extensions
{
    public static class TableStorageExtensions
    {
        public static async Task<IList<T>> ExecuteQueryAsync<T>(this CloudTable table, TableQuery<T> query, CancellationToken ct = default(CancellationToken), Action<IList<T>> onProgress = null) where T : ITableEntity, new()
        {
            var items = new List<T>();
            TableContinuationToken token = null;

            do
            {
                TableQuerySegment<T> seg = await table.ExecuteQuerySegmentedAsync<T>(query, token);
                token = seg.ContinuationToken;
                items.AddRange(seg);
                if (onProgress != null) onProgress(items);

            } while (token != null && !ct.IsCancellationRequested);

            return items;
        }
    }
}
