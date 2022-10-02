using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using SpotiApiModels;
using System.Net.Http.Json;

namespace SpotiView.Shared
{
    public class WebApp : ComponentBase
    {
        [Inject]
        private IJSRuntime _jsRuntime { get; set; } = default!;
        [Inject]
        private HttpClient _httpClient { get; set; } = default!;

        protected override async Task OnInitializedAsync()
        {
            await _jsRuntime.InvokeVoidAsync("Telegram.WebApp.ready");
            await _jsRuntime.InvokeVoidAsync("Telegram.WebApp.expand");
            //var initData = await _jsRuntime.InvokeAsync<string>("Telegram.WebApp.initDataFunction");
            var initData = "query_id=AAH1FQwFAAAAAPUVDAVIOGlr&user=%7B%22id%22%3A84678133%2C%22first_name%22%3A%22Laurens%22%2C%22last_name%22%3A%22%22%2C%22language_code%22%3A%22nl%22%7D&auth_date=1664541959&hash=b0180e2b306050daa3fb100778460bf42aa9a5f0061f519fcd92795d2914e208";

            if (string.IsNullOrEmpty(initData))
                return;

            var response = await _httpClient.GetAsync("/api/InitData?" + initData);

            if (!response.IsSuccessStatusCode || response.StatusCode == System.Net.HttpStatusCode.NoContent)
                return;

            var result = await response.Content.ReadFromJsonAsync<InitDataResult>();
        }
    }
}