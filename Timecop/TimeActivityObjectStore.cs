using Blazor.ClientStorage;

namespace Timecop
{
    public class TimeActivityObjectStore : AutoIncrementObjectStore<TimeActivity>
    {
        public TimeActivityObjectStore(IBlazorClientStorage blazorClientStorage)
            : base(blazorClientStorage)
        {
        }

        public override string Name => nameof(TimeActivity);

        public override ObjectStoreDescriptor GetObjectStoreDescriptor() =>
            new()
            {
                Name = this.Name,
                Options = new()
                {
                    autoIncrement = true
                },

                Indices = new[] {
                    new IndexDescriptor()
                    {
                        Name="keyDay",
                        Options = new()
                        {
                            unique = false
                        },
                    },
                    new IndexDescriptor()
                    {
                        Name="keyMonth",
                        Options = new()
                        {
                            unique = false
                        },
                    }
                }
            };
    }

    public class TimeActivity : IObjectStoreModel<int>
    {
        public int key { get; set; }

        public string keyDay { get; set; }

        public string keyMonth { get; set; }

        public DateTime Start { get; set; }

        public DateTime Stop { get; set; }

        public TimeSpan Duration { get; set; }
    }
}
