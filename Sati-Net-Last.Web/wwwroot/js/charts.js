// Variables globales para History MT
let historyChart = null;
let historyCandleSeries = null;

// Variables globales para Sati Trader
let traderChart = null;
let traderCandleSeries = null;
let lastCandle = null;
let signalRConnection = null;
const getCurrentSelectedSymbol = () => {
    const symbol = (window.currentSelectedSymbol || document.getElementById('traderSymbolDropdown')?.value || document.getElementById('userSymbolDropdown')?.value || '');
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
    
    if (!container) {
        console.error('traderChartContainer not found');
        return;
    }

    if (document.getElementById('traderSymbolDropdown')) {
        document.getElementById('traderSymbolDropdown').value = selectedSymbol;
    }

    // Destruir gráfico anterior si existe
    if (traderChart) {
        traderChart.remove();
        traderChart = null;
    }
    
    // Crear gráfico
    traderChart = LightweightCharts.createChart(container, {
        width: 900,
        height: 500,
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

    // Inicializar SignalR
    if (signalRConnection) {
        signalRConnection.stop();
    }

    signalRConnection = new signalR.HubConnectionBuilder()
        .withUrl("https://localhost:5100/metatraderhub")
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
    });

    signalRConnection.start()
        .then(() => {
            console.log("SignalR connected successfully");
        })
        .catch(err => {
            console.error("SignalR connection error:", err);
        });

    // Cargar historial inicial del símbolo activo del usuario
    fetch(`https://localhost:5100/api/metatrader/GetPriceHistory?symbol=${encodeURIComponent(selectedSymbol)}`, {
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
                traderChart.timeScale().fitContent();
            }
        })
        .catch(err => console.error("Error loading initial history:", err));
}

/**
 * Actualiza el gráfico de trader con nuevos datos de SignalR
 */
function updateTraderChart(newData) {
    if (!traderCandleSeries) return;

    const time = getUnixTimeStampForTrader(newData.time);
    const candleInterval = 60; // 1 minuto en segundos
    const candleTime = time - (time % candleInterval); // Redondear al minuto

    if (!lastCandle) {
        lastCandle = {
            time: candleTime,
            open: newData.bid,
            high: newData.bid,
            low: newData.bid,
            close: newData.bid
        };
        traderCandleSeries.update(lastCandle);
        return;
    }

    if (candleTime >= lastCandle.time + candleInterval) {
        // Nueva vela
        lastCandle = {
            time: candleTime,
            open: newData.bid,
            high: newData.bid,
            low: newData.bid,
            close: newData.bid
        };
        traderCandleSeries.update(lastCandle);
    } else {
        // Actualizar vela existente
        lastCandle = {
            ...lastCandle,
            close: newData.bid,
            high: Math.max(lastCandle.high, newData.bid),
            low: Math.min(lastCandle.low, newData.bid)
        };
        traderCandleSeries.update(lastCandle);
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