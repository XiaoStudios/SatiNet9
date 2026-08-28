$(document).ready(function () {
    SatiTrader.Init();
})

var SatiTrader = {
    Init: () => {
        const dropdown = document.getElementById('symbolDropdown');
        const config = window.satiTraderConfig || {};
        const currentSymbol = config.selectedSymbol || '';
        const userId = config.userId || '';

        window.currentUserId = userId;
        window.backendApiBaseUrl = config.backendApiBaseUrl || 'http://localhost:5289/';

        const initRealtimeIfAllowed = () => {
            if (!userId) return;

            fetch(`${window.backendApiBaseUrl}api/metatrader/TrackSymbolsForUser?userId=${encodeURIComponent(userId)}`, {
                method: 'POST'
            })
            .then(async response => {
                const payload = await response.json().catch(() => ({}));
                if (!response.ok || !payload.success) {
                    console.warn(payload.message || 'No se pudo activar tracking realtime para el usuario.');
                    return;
                }

                if (dropdown && payload.selectedSymbol) {
                    dropdown.value = payload.selectedSymbol;
                    window.currentSelectedSymbol = payload.selectedSymbol;
                    document.getElementById('chartSymbol').textContent = payload.selectedSymbol;
                }

                if (payload.selectedSymbol) {
                    ChartsJS.InitSatiTrader();
                }
            })
            .catch(err => {
                console.warn('No se pudo validar el acceso al realtime en la API.', err);
            });
        };

        if (dropdown) {
            if (currentSymbol) {
                dropdown.value = currentSymbol;
                window.currentSelectedSymbol = currentSymbol;
                document.getElementById('chartSymbol').textContent = currentSymbol;
            }

            dropdown.addEventListener('change', function () {
                const symbol = this.value;
                if (!symbol) return;

                window.currentSelectedSymbol = symbol;
                document.getElementById('chartSymbol').textContent = symbol;

                fetch('/Home/SetSelectedSymbol?symbol=' + encodeURIComponent(symbol), {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/x-www-form-urlencoded; charset=UTF-8' }
                });

                initRealtimeIfAllowed();
            });
        }

        if (currentSymbol) {
            initRealtimeIfAllowed();
        }

        const updateLastTimestamp = () => {
            const updateElement = document.getElementById('lastUpdate');
            if (!updateElement || !window.lastRealtimeTimestamp) return;

            const diffSeconds = Math.max(0, Math.floor((Date.now() - window.lastRealtimeTimestamp) / 1000));
            updateElement.textContent = `Hace ${diffSeconds}s`;
        };

        window.lastRealtimeTimestamp = Date.now();
        updateLastTimestamp();

        setInterval(updateLastTimestamp, 1000);
    }
}