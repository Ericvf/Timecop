namespace Timecop.Services
{

    //public interface IAuthorizationService
    //{
    //    Task StoreAccessToken(string accessToken);

    //    Task<string> GetAccessToken();
    //}

    //public class AuthorizationService : IAuthorizationService
    //{
    //    private readonly CheckInStateObjectStore checkInStateObjectStore;

    //    public AuthorizationService(CheckInStateObjectStore checkInStateObjectStore)
    //    {
    //        this.checkInStateObjectStore = checkInStateObjectStore;
    //    }

    //    public async Task<string> GetAccessToken()
    //    {
    //        var state = await checkInStateObjectStore.Get("root");
    //        return state.AccessToken;
    //    }

    //    public async Task StoreAccessToken(string accessToken)
    //    {
    //        var state = await checkInStateObjectStore.Get("root");
    //        state.AccessToken = accessToken;
    //        await checkInStateObjectStore.Put(state);
    //    }
    //}

    public class CheckInOutService : ICheckInOutService
    {
        private readonly CheckInStateObjectStore checkInStateObjectStore;
        private readonly TimeActivityObjectStore timeActivityObjectStore;

        public CheckInOutService(CheckInStateObjectStore checkInStateObjectStore, TimeActivityObjectStore timeActivityObjectStore)
        {
            this.checkInStateObjectStore = checkInStateObjectStore;
            this.timeActivityObjectStore = timeActivityObjectStore;
        }

        public Task<CheckInState> GetState()
        {
            return checkInStateObjectStore.Get("root");
        }

        public async Task Checkin(CheckInState checkInState)
        {
            checkInState.CheckInDate = DateTime.Now;
            checkInState.IsCheckedIn = true;

            await checkInStateObjectStore.Put(checkInState);
        }

        public async Task Checkout(CheckInState checkInState)
        {
            checkInState.LastCheckoutDate = DateTime.Now;
            checkInState.IsCheckedIn = false;

            await checkInStateObjectStore.Put(checkInState);

            await timeActivityObjectStore.Add(new TimeActivity()
            {
                keyDay = checkInState.CheckInDate.ToString("yyyyMMdd"),
                keyMonth = checkInState.CheckInDate.ToString("yyyyMM"),
                Start = checkInState.CheckInDate,
                Stop = checkInState.LastCheckoutDate,
                Duration = checkInState.LastCheckoutDate - checkInState.CheckInDate
            });
        }
    }

    public interface ICheckInOutService
    {
        Task<CheckInState> GetState();

        Task Checkin(CheckInState checkInState);

        Task Checkout(CheckInState checkInState);
    }
}
