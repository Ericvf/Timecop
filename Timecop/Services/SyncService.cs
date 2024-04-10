using CSVFile;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using System.IO;
using System.Text;
using static Timecop.Services.ISyncService;

namespace Timecop.Services
{
    public class SyncService : ISyncService
    {
        private readonly GraphServiceClient client;
        private readonly TimeActivityObjectStore timeActivityObjectStore;

        public SyncService(GraphServiceClient client, TimeActivityObjectStore timeActivityObjectStore)
        {
            this.client = client;
            this.timeActivityObjectStore = timeActivityObjectStore;
        }

        public class TimeActivityComparer : IEqualityComparer<TimeActivity>
        {
            public bool Equals(TimeActivity x, TimeActivity y)
            {
                if (object.ReferenceEquals(x, y))
                {
                    return true;
                }
                if (object.ReferenceEquals(x, null) || object.ReferenceEquals(y, null))
                {
                    return false;
                }
                return x.key == y.key && x.Start == y.Start && x.Stop == y.Stop;
            }
            public int GetHashCode(TimeActivity obj)
            {
                if (obj == null)
                {
                    return 0;
                }
                return obj.key.GetHashCode() ^ obj.Start.GetHashCode() ^ obj.Stop.GetHashCode();
            }
        }


        public async Task<SyncResult> Synchronize()
        {
            var allTimeActivities = await timeActivityObjectStore.GetAll();
            var loadedTimeActivities = await LoadAll();

           // var newActivities = allTimeActivities.Except(loadedTimeActivities, new TimeActivityComparer());
            //if (newActivities.Any())
            {
                var sb = new StringBuilder();
                CSV.AppendCSVHeader<TimeActivity>(sb);
                foreach (var item in allTimeActivities)
                {
                    CSV.AppendCSVLine(sb, item);
                }

                var driveItem = await client.Me.Drive.GetAsync();
                var userDriveId = driveItem!.Id;
                var driveRequest = client.Drives[userDriveId];
                var rootDrive = await driveRequest.Root.GetAsync();

                if (rootDrive is not null)
                {
                    var itemWithPath = driveRequest.Items[rootDrive.Id].ItemWithPath("TimeCop/TimeActivities.csv");
                    var content = sb.ToString();

                    using var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));
                    var x = await itemWithPath.Content.PutAsync(stream);

                    return new SyncResult(true);
                }
            }

            return new SyncResult(false);
        }

        private async Task<IEnumerable<TimeActivity>> LoadAll()
        {
            var driveItem = await client.Me.Drive.GetAsync();
            var userDriveId = driveItem!.Id;
            var driveRequest = client.Drives[userDriveId];
            var rootDrive = await driveRequest.Root.GetAsync();

            if (rootDrive is not null)
            {
                var itemWithPath = driveRequest.Items[rootDrive.Id].ItemWithPath("TimeCop/TimeActivities.csv");
                try
                {
                    var fileItem = await itemWithPath.GetAsync();
                    var content = await itemWithPath.Content.GetAsync();
                    using (var cr = new CSVReader(new StreamReader(content)))
                    {
                        return cr.Deserialize<TimeActivity>().ToList();
                    }
                }
                catch (Exception ex)
                {
                    return default;
                }
            }
            return default;
        }
    }

    public interface ISyncService
    {
        Task<SyncResult> Synchronize();

        public record SyncResult(bool isSuccess);
    }
}
