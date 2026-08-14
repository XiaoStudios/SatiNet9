// Variables globales para History MT
let historyChart = null;
let historyCandleSeries = null;

// Variables globales para Sati Trader
let traderChart = null;
let traderCandleSeries = null;
let lastCandle = null;
let signalRConnection = null;
let currentJoinedSymbol = null;
let traderResizeHandlerBound = false;
let traderFollowLatest = true;
let traderResizeObserver = null;
let traderIsAtLive = true;

const getBackendApiBaseUrl = () => {
    const configured = String(window.backendApiBaseUrl || '').trim();
    const fallback = 'http://localhost:5289/';
    const base = configured || fallback;
    return base.endsWith('/') ? base : `${base}/`;
};
const getCurrentSelectedSymbol = () => {
    const symbol = (window.currentSelectedSymbol || document.getElementById('symbolDropdown')?.value || document.getElementById('traderSymbolDropdown')?.value || document.getElementById('userSymbolDropdown')?.value || '');
    return String(symbol).trim();
};

/**
 * Dibuja el gráfico de historial para History MT
 */
function drawHistoryChart(containerId, candles) {
    const container = document.getElementById(containerId);
    
    if (!container) {
        console.error(`Container ${containerId} not found`);
        return;
    }

    // Verificar que LightweightCharts esté disponible
    if (typeof LightweightCharts === 'undefined') {
        console.error('LightweightCharts is not loaded');
        return;
    }

    // Limpiar contenedor
    container.innerHTML = '';
    
    // Destruir gráfico anterior si existe
    if (historyChart) {
        try {
            historyChart.remove();
        } catch (e) {
            console.warn('Error removing previous chart:', e);
        }
        historyChart = null;
        historyCandleSeries = null;
    }
    
    try {
        // Crear gráfico
        historyChart = LightweightCharts.createChart(container, {
            width: container.clientWidth || 900,
            height: container.clientHeight || 500,
            layout: {
                background: { color: '#181a20' },
                textColor: '#d1d4dc'
            },
            grid: {
                vertLines: { color: '#222' },
                horzLines: { color: '#222' }
            },
            crosshair: {
                mode: LightweightCharts.CrosshairMode.Normal
            },
            rightPriceScale: {
                borderColor: '#444'
            },
            timeScale: {
                borderColor: '#444',
                timeVisible: true,
                secondsVisible: false
            }
        });

        console.log('Chart created successfully:', historyChart);

        // Verificar que el gráfico se creó correctamente
        if (!historyChart || typeof historyChart.addCandlestickSeries !== 'function') {
            console.error('Chart created but addCandlestickSeries is not available');
            console.log('Chart object:', historyChart);
            return;
        }

        // Crear serie de velas
        historyCandleSeries = historyChart.addCandlestickSeries({
            upColor: '#26a69a',
            downColor: '#ef5350',
            borderVisible: false,
            wickUpColor: '#26a69a',
            wickDownColor: '#ef5350',
            priceFormat: {
                type: 'price',
                precision: 5,
                minMove: 0.00001
            }
        });

        console.log('Candlestick series created successfully');

        // Ordenar y establecer datos
        if (candles && candles.length > 0) {
            // Validar y limpiar datos
            const validCandles = candles.filter(c => 
                c.time && 
                !isNaN(c.open) && 
                !isNaN(c.high) && 
                !isNaN(c.low) && 
                !isNaN(c.close)
            );

            console.log(`Valid candles: ${validCandles.length} of ${candles.length}`);

            if (validCandles.length > 0) {
                validCandles.sort((a, b) => a.time - b.time);
                historyCandleSeries.setData(validCandles);
                historyChart.timeScale().fitContent();
                console.log('Chart data set successfully');
            } else {
                console.warn('No valid candles to display');
            }
        } else {
            console.warn('No candles data provided');
        }
    } catch (error) {
        console.error('Error creating chart:', error);
        container.innerHTML = '<div style="color: red; padding: 20px;">Error al crear el gráfico: ' + error.message + '</div>';
    }
}

/**
 * Inicializa Sati Trader con gráfico en tiempo real y SignalR
 */
function initSatiTrader() {
    const container = document.getElementById('traderChartContainer');
    const selectedSymbol = getCurrentSelectedSymbol();
    const userId = Number(window.currentUserId || 0);
    const backendBaseUrl = getBackendApiBaseUrl();
    
    if (!container) {
        console.error('traderChartContainer not found');
        return;
    }

    // Avoid duplicated visual blocks when reinitializing the realtime chart.
    container.innerHTML = '';

    if (document.getElementById('symbolDropdown')) {
        document.getElementById('symbolDropdown').value = selectedSymbol;
    }

    // Destruir gráfico anterior si existe
    if (traderChart) {
        traderChart.remove();
        traderChart = null;
    }
    
    const chartWidth = container.clientWidth > 0 ? container.clientWidth : 900;
    const chartHeight = container.clientHeight > 0 ? container.clientHeight : 500;

    // Crear gráfico
    traderChart = LightweightCharts.createChart(container, {
        width: chartWidth,
        height: chartHeight,
        layout: {
            background: { color: '#181a20' },
            textColor: '#d1d4dc'
        },
        grid: {
            vertLines: { color: '#222' },
            horzLines: { color: '#222' }
        },
        crosshair: {
            mode: LightweightCharts.CrosshairMode.Normal
        },
        rightPriceScale: {
            borderColor: '#444'
        },
        timeScale: {
            borderColor: '#444',
            timeVisible: true,
            secondsVisible: false
        }
    });

    // Crear serie de velas
    traderCandleSeries = traderChart.addCandlestickSeries({
        upColor: '#26a69a',
        downColor: '#ef5350',
        borderVisible: false,
        wickUpColor: '#26a69a',
        wickDownColor: '#ef5350',
        priceFormat: {
            type: 'price',
            precision: 5,
            minMove: 0.00001
        }
    });

    traderFollowLatest = true;
    traderIsAtLive = true;
    const returnLiveBtn = document.getElementById('returnToLiveBtn');
    if (returnLiveBtn) returnLiveBtn.style.display = 'none';

    if (traderChart && typeof traderChart.timeScale === 'function') {
        traderChart.timeScale().subscribeVisibleTimeRangeChange(() => {
            const data = traderCandleSeries ? traderCandleSeries.data() : [];
            if (!data || data.length < 2) return;

            const latestIndex = data.length - 1;
            const visibleRange = traderChart.timeScale().getVisibleLogicalRange();
            if (!visibleRange || typeof visibleRange.to !== 'number') return;

            const isNearEnd = visibleRange.to >= latestIndex - 5;
            traderFollowLatest = isNearEnd;
            traderIsAtLive = isNearEnd;

            const returnLiveBtn = document.getElementById('returnToLiveBtn');
            if (returnLiveBtn) {
                returnLiveBtn.style.display = isNearEnd ? 'none' : 'block';
            }

            const liveIndicator = document.getElementById('traderLiveIndicator');
            if (liveIndicator) {
                liveIndicator.style.opacity = isNearEnd ? '1' : '0.3';
            }
        });
    }

    // Inicializar SignalR
    if (signalRConnection) {
        signalRConnection.stop();
    }

    signalRConnection = new signalR.HubConnectionBuilder()
        .withUrl(`${backendBaseUrl}metatraderhub`)
        .withAutomaticReconnect()
        .build();

    signalRConnection.on("ReceiveMetaTraderLogin", (newData) => {
        console.log("Received MetaTrader Login Data:", newData);
    });

    signalRConnection.on("ReceiveMetaTraderData", (newData) => {
        const selectedSymbol = getCurrentSelectedSymbol();
        const incomingSymbol = (newData && (newData.SYMBOL || newData.symbol || newData.Symbol || newData.symbolStr)) || '';

        if (!selectedSymbol || !incomingSymbol || incomingSymbol.toUpperCase() !== selectedSymbol.toUpperCase()) {
            return;
        }

        updateTraderChart(newData);

        const bid = Number(newData.BID ?? newData.bid ?? 0);
        const ask = Number(newData.ASK ?? newData.ask ?? 0);
        const spread = ask > 0 && bid > 0 ? ((ask - bid) * 10000).toFixed(1) + ' pips' : '---';

        const bidEl = document.getElementById('bidValue');
        const askEl = document.getElementById('askValue');
        const spreadEl = document.getElementById('spreadValue');
        const volumeEl = document.getElementById('volumeValue');
        const updateEl = document.getElementById('lastUpdate');

        if (bidEl) bidEl.textContent = bid > 0 ? bid.toFixed(5) : '---';
        if (askEl) askEl.textContent = ask > 0 ? ask.toFixed(5) : '---';
        if (spreadEl) spreadEl.textContent = spread;
        if (volumeEl) volumeEl.textContent = String(newData.VOLUME ?? newData.volume ?? '---');
        if (updateEl) {
            window.lastRealtimeTimestamp = Date.now();
            updateEl.textContent = 'Hace 0s';
        }
    });

    signalRConnection.start()
        .then(() => {
            console.log("SignalR connected successfully");

            if (selectedSymbol && userId > 0) {
                return signalRConnection.invoke("JoinSymbol", userId, selectedSymbol)
                    .then(() => {
                        currentJoinedSymbol = selectedSymbol;
                        console.log("Joined symbol group:", selectedSymbol);
                    });
            }

            return Promise.resolve();
        })
        .catch(err => {
            console.error("SignalR connection error:", err);
        });

    if (!traderResizeHandlerBound) {
        const resizeTraderChartLayout = () => {
            const chartContainer = document.getElementById('traderChartContainer');
            if (!traderChart || !chartContainer) return;

            const width = chartContainer.clientWidth > 0 ? chartContainer.clientWidth : 900;
            const height = chartContainer.clientHeight > 0 ? chartContainer.clientHeight : 500;
            traderChart.applyOptions({ width, height });
        };

        if (typeof ResizeObserver !== 'undefined') {
            traderResizeObserver = new ResizeObserver(() => {
                requestAnimationFrame(resizeTraderChartLayout);
            });
            traderResizeObserver.observe(container);
        }

        window.addEventListener('resize', resizeTraderChartLayout, { passive: true });
        traderResizeHandlerBound = true;
    }

    // Cargar historial inicial del símbolo activo del usuario
    fetch(`${backendBaseUrl}api/metatrader/GetPriceHistory?symbol=${encodeURIComponent(selectedSymbol)}`, {
        method: 'GET',
        headers: {
            'Accept': 'application/json'
        }
    })
        .then(res => res.json())
        .then(data => {
            if (data && data.length > 0) {
                const candles = data.map(d => ({
                    time: getUnixTimeStampForTrader(d.timE_MTAPI || d.time || d.TIME),
                    open: Number(d.open ?? d.OPEN ?? 0),
                    high: Number(d.high ?? d.HIGH ?? 0),
                    low: Number(d.low ?? d.LOW ?? 0),
                    close: Number(d.close ?? d.CLOSE ?? 0)
                }));
                candles.sort((a, b) => a.time - b.time);
                traderCandleSeries.setData(candles);
                lastCandle = candles[candles.length - 1];

                const visibleWindow = 20 * 60;
                const lastTime = candles[candles.length - 1].time;
                const startTime = lastTime - visibleWindow;
                const startIdx = candles.findIndex(c => c.time >= startTime);
                const finalStartIdx = startIdx >= 0 ? startIdx : Math.max(0, candles.length - 30);

                const logicalRange = {
                    from: finalStartIdx,
                    to: candles.length
                };
                traderChart.timeScale().setVisibleLogicalRange(logicalRange);
            }
        })
        .catch(err => console.error("Error loading initial history:", err));
}

/**
 * Actualiza el gráfico de trader con nuevos datos de SignalR
 */
function updateTraderChart(newData) {
    if (!traderCandleSeries) return;

    const rawTime = newData.TIME ?? newData.time ?? new Date().toISOString();
    const rawBid = Number(newData.BID ?? newData.bid ?? 0);
    if (!rawBid || Number.isNaN(rawBid)) return;

    let time = getUnixTimeStampForTrader(rawTime);
    if (!time || Number.isNaN(time) || time <= 0) {
        time = Math.floor(Date.now() / 1000);
    }

    const candleInterval = 60;
    const candleTime = time - (time % candleInterval);

    if (!lastCandle) {
        lastCandle = {
            time: candleTime,
            open: rawBid,
            high: rawBid,
            low: rawBid,
            close: rawBid
        };
        traderCandleSeries.update(lastCandle);

        if (traderChart && traderFollowLatest && typeof traderChart.timeScale === 'function') {
            try {
                traderChart.timeScale().scrollToRealTime();
            } catch (error) {
                console.warn('No se pudo seguir el último candle:', error);
            }
        }
        return;
    }

    if (candleTime > lastCandle.time) {
        lastCandle = {
            time: candleTime,
            open: rawBid,
            high: rawBid,
            low: rawBid,
            close: rawBid
        };
        traderCandleSeries.update(lastCandle);
    } else {
        lastCandle = {
            ...lastCandle,
            close: rawBid,
            high: Math.max(lastCandle.high, rawBid),
            low: Math.min(lastCandle.low, rawBid)
        };
        traderCandleSeries.update(lastCandle);
    }

    if (traderChart && traderFollowLatest && typeof traderChart.timeScale === 'function') {
        try {
            traderChart.timeScale().scrollToRealTime();
        } catch (error) {
            console.warn('No se pudo mantener el último candle visible:', error);
        }
    }
}

/**
 * Convierte una cadena de tiempo a timestamp UNIX (para trader en tiempo real)
 * @deprecated Usar CommonSatiUI.GetRealtimeTimestamp directamente
 */
function getUnixTimeStampForTrader(timeStr) {
    if (!timeStr) return Math.floor(Date.now() / 1000);
    
    // Usar el método centralizado para tiempo real
    return CommonSatiUI.GetRealtimeTimestamp(timeStr);
}

/**
 * Vuelve a la vista en vivo del gráfico
 */
function returnToLiveChart() {
    if (!traderChart || !traderCandleSeries) return;

    traderFollowLatest = true;
    traderIsAtLive = true;

    try {
        traderChart.timeScale().scrollToRealTime();
    } catch (error) {
        console.warn('Error scrolling to real time:', error);
    }

    const returnLiveBtn = document.getElementById('returnToLiveBtn');
    if (returnLiveBtn) {
        returnLiveBtn.style.display = 'none';
    }

    const liveIndicator = document.getElementById('traderLiveIndicator');
    if (liveIndicator) {
        liveIndicator.style.opacity = '1';
    }
}