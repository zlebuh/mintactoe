using Microsoft.JSInterop;

namespace Zlebuh.MinTacToe.UI.Services
{
    public interface ICanReactOnGameStateChange
    {
        Task OnGameStateChanged(string json);
    }

    public interface ISupabaseRealtime
    {
        Task StartRealtime<T>(string gameId, T obj) where T : class, ICanReactOnGameStateChange;
        Task StopRealtime(object obj);
    }

    public class SupabaseRealtime : ISupabaseRealtime
    {
        private readonly IJSRuntime js;
        private readonly string url;
        private readonly string key;
        private readonly Dictionary<object, IDisposable> dotnetReferenceMaps = [];
        public SupabaseRealtime(IJSRuntime js, IConfiguration configuration)
        {
            this.js = js;
            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration), "Configuration cannot be null.");
            }
            url = configuration["Supabase:Url"]
                ?? throw new InvalidDataException("Missing Supabase URL in appsettings.json.");
            key = configuration["Supabase:SubscriptionKey"]
                ?? throw new InvalidDataException("Missing Supabase Key in appsettings.json.");
        }

        public async Task StopRealtime(object obj)
        {
            await js.InvokeVoidAsync("supabaseRealtime.stopRealtime");
            if (obj != null)
            {
                if (dotnetReferenceMaps.TryGetValue(obj, out IDisposable? value))
                {
                    value.Dispose();
                    dotnetReferenceMaps.Remove(obj);
                }
            }
        }

        public async Task StartRealtime<T>(string gameId, T obj) where T : class, ICanReactOnGameStateChange
        {
            if (string.IsNullOrEmpty(gameId))
            {
                throw new ArgumentException("Game ID cannot be null or empty.", nameof(gameId));
            }
            if (obj == null)
            {
                throw new ArgumentNullException(nameof(obj), "Self reference cannot be null.");
            }
            DotNetObjectReference<T> selfRef = DotNetObjectReference.Create(obj);
            if (dotnetReferenceMaps.ContainsKey(obj))
            {
                dotnetReferenceMaps[obj].Dispose();
                dotnetReferenceMaps.Remove(obj);
            }
            dotnetReferenceMaps[obj] = selfRef;
            await js.InvokeVoidAsync("supabaseRealtime.startRealtime", url, key, gameId, selfRef);
        }
    }
}
