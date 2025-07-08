window.supabaseRealtime = {
    channel: null,

    async startRealtime(supabaseUrl, supabaseToken, gameId, dotNetRef) {
        const client = window.supabase.createClient(supabaseUrl, supabaseToken);

        this.channel = client
            .channel('public:games')
            .on('postgres_changes', {
                event: 'UPDATE',
                schema: 'public',
                table: 'games',
                filter: `id=eq.${gameId}`
            }, payload => {
                dotNetRef.invokeMethodAsync('OnGameStateChanged', JSON.stringify(payload.new));
            })
            .subscribe();
    },

    async stopRealtime() {
        if (this.channel) {
            await this.channel.unsubscribe();
            this.channel = null;
        }
    }
};