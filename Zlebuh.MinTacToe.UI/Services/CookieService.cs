using Microsoft.JSInterop;

namespace Zlebuh.MinTacToe.UI.Services
{
    public interface ICookieService
    {
        Task<string> GetCookie(string name);
        Task SetCookie(string name, string value, int days);
    }

    public class CookieService(IJSRuntime js) : ICookieService
    {
        private readonly IJSRuntime js = js;

        public async Task SetCookie(string name, string value, int days)
        {
            await js.InvokeVoidAsync("setCookie", name, value, days);
        }

        public async Task<string> GetCookie(string name)
        {
            return await js.InvokeAsync<string>("getCookie", name) ?? string.Empty;
        }
    }
}
