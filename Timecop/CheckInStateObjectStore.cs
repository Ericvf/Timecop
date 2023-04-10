using Blazor.ClientStorage;

namespace Timecop
{
    public class CheckInStateObjectStore : KeyPathObjectStore<CheckInState, string>
    {
        public CheckInStateObjectStore(IBlazorClientStorage blazorClientStorage) 
            : base(blazorClientStorage)
        {
        }

        public override string Name => nameof(CheckInState);

        public override ObjectStoreDescriptor GetObjectStoreDescriptor() =>
            new()
            {
                Name = this.Name,
                Options = new()
                {
                    keyPath = nameof(CheckInState.key)
                }
            };
    }

    public class CheckInState : IObjectStoreModel<string>
    {
        public string key { get; set; }

        public bool IsCheckedIn { get; set; }

        public DateTime CheckInDate { get; set; }
        public DateTime LastCheckoutDate { get; set; }
    }
}
