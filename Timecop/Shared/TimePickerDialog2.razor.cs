using MudBlazor;

namespace Timecop.Shared
{
    public partial class TimePickerDialog2
    {
        public static async Task<TimeSpan?> ShowDialog(IDialogService dialogService, TimeSpan timeSpan)
        {
            var parameters = new DialogParameters();
            parameters.Add("Time", timeSpan);

            var options = new DialogOptions { CloseOnEscapeKey = true };
            var dialog = dialogService.Show<TimePickerDialog2>(null, parameters, options);

            var dialogResult = await dialog.Result;

            if (!dialogResult.Canceled)
            {
                return  (TimeSpan)dialogResult.Data;
            }

            return null;
        }
    }
}