$(document).ready(function() {
    HistoryJS.Init();
})

var HistoryJS =
{
    // Estado para recordar si se cargó con algoritmo
    lastLoadedWithAlgorithm: false,
    
    Init: () => {
        HistoryJS.ResetFilters();
        HistoryJS.SetupEventHandlers();
    },
    ResetFilters: () => {
        // Resetear símbolo
        $('#symbolDropdown').val('');
        
        // Resetear fecha a hoy (hora local, no UTC)
        const today = new Date();
        const year = today.getFullYear();
        const month = String(today.getMonth() + 1).padStart(2, '0');
        const day = String(today.getDate()).padStart(2, '0');
        const localDate = `${year}-${month}-${day}`;
        $('#dateFilter').val(localDate);

        // WAM period por defecto es 20 (ya está en el HTML con selected)
        $('#wamPeriodDropdown').val('20')
        $('#pmpnPeriod').text($('#wamPeriodDropdown').val());

        // Ocultar botones de Excel
        $('#excelButtonsContainer').hide();
        $('#dataTableContainer').hide();
        
        // Desactivar algoritmo y deshabilitar campos relacionados
        $('#enableAlgorithm').prop('checked', false);
        $('#algorithmFields').css('opacity', '0.5');
        $('#algorithmFields input:not([readonly])').prop('disabled', true);
        $('#visualizationOptions').css('opacity', '0.5');
        $('#visualizationOptions input[type="checkbox"]').prop('disabled', true);

        // Ocultar tabla de datos
        $('#dataTableContainer').hide();
    },
    SetupEventHandlers: () => {
        // Sincronizar WAM period con PMPn period
        $('#wamPeriodDropdown').change(function() { $('#pmpnPeriod').text($(this).val()); });

        // Habilitar/deshabilitar campos de algoritmo y opciones de visualización
        $('#enableAlgorithm').change(function() {
            const isChecked = this.checked;
            // Campos del algoritmo
            $('#algorithmFields').css('opacity', isChecked ? '1' : '0.5');
            $('#algorithmFields input:not([readonly])').prop('disabled', !isChecked);
            // Opciones de visualización (solo relevantes con algoritmo activo)
            $('#visualizationOptions').css('opacity', isChecked ? '1' : '0.5');
            $('#visualizationOptions input[type="checkbox"]').prop('disabled', !isChecked);
            
            // Si hay datos cargados y cambia el checkbox, ocultar tabla y Excel
            // para forzar recarga con la nueva configuración
            if ($('#dataTableContainer').is(':visible')) {
                $('#dataTableContainer').hide();
                $('#excelButtonsContainer').hide();
                alert('El modo de cálculo ha cambiado. Por favor, recarga el historial.');
            }
        });

        $('#btnLoadHistory').click(HistoryJS.LoadPriceHistory);
        $('#operativeDefaultValues').click(HistoryJS.ResetAlgorithmParams);
        $('#btnDownloadExcel').click(HistoryJS.DownloadExcel);
    },
    ResetAlgorithmParams: () => {
        $('#paramPt').val(0.05);
        $('#paramPr').val(0.75);
        $('#paramSigma').val(2.0);
        $('#paramMp').val(1000);
        $('#paramFd').val(0.10);
    },
    LoadPriceHistory: () => {
        let symbol = $('#symbolDropdown').val();
        let dateFilter = $('#dateFilter').val();
        let wamPeriod = $('#wamPeriodDropdown').val();
        let enableAlgorithm = $('#enableAlgorithm').prop('checked');
        
        if (!symbol || !dateFilter) {
            alert('Por favor selecciona símbolo y fecha');
            return;
        }
        
        $('#btnLoadHistory').prop('disabled', true);

        // Construir URL según si hay algoritmo o no
        let loadUrl = '/HistoryMT';
        let postData = {
            dateFilter: dateFilter,
            symbolStr: symbol,
            wamPeriod: wamPeriod
        };
        
        if (enableAlgorithm) {
            loadUrl += '/GetDatePriceHistoryWithAlgorithm';
            // Agregar parámetros del algoritmo
            postData.pt = $('#paramPt').val();
            postData.pr = $('#paramPr').val();
            postData.sigma = $('#paramSigma').val();
            postData.mp = $('#paramMp').val();
            postData.fd = $('#paramFd').val();
        }
        else {
            loadUrl += '/GetDatePriceHistory';
        }
        
        $.ajax({
            type: 'post',
            url: loadUrl,
            data: postData,
            dataType: 'json',
            beforeSend: function () { CommonSatiUI.ShowLoading(); },
            success: function (data) {
                if (data && data.length > 0) {
                    // Dibujar gráfico
                    const candles = data.map(d => ({
                        time: CommonSatiUI.GetOriginalTimestamp(d.time),  // ✅ Timestamp original sin ajustes
                        open: parseFloat(d.open),
                        high: parseFloat(d.high),
                        low: parseFloat(d.low),
                        close: parseFloat(d.close)
                    }));
                    drawHistoryChart('historyChartContainer', candles);
                    
                    HistoryJS.RenderHybridDataTable(data, enableAlgorithm);
                    
                    // Guardar estado del algoritmo
                    HistoryJS.lastLoadedWithAlgorithm = enableAlgorithm;
                    
                    // Mostrar tabla y botón de Excel
                    $('#dataTableContainer').show();
                    $('#excelButtonsContainer').show();
                    $('#wamPeriodLabel').text(`WAM ${wamPeriod}`);
                    $('#chartSymbolLabel').text(symbol);
                    
                    // Habilitar botón
                    $('#btnDownloadExcel').prop('disabled', false);
                }
                else {
                    alert('No hay datos disponibles');
                }
            },
            error: function (xhr, status, error) {
                // console.error('Error al cargar historial:', status, error);
                // alert(`Error al cargar el historial: ${error}`);
            },
            complete: function () {
                CommonSatiUI.HideLoading();
                $('#btnLoadHistory').prop('disabled', false);
            }
        });
    },
    RenderHybridDataTable: (data, withAlgorithm) => {
        CommonSatiUI.InitHeadersDataTable('#priceDataTableHeader', HistoryJS.ReturnTableHeaders(withAlgorithm));
        
        const dataTableOptions = {
            order: [[1, 'asc']],
            pageLength: 20,
            scrollX: false,
            autoWidth: false,
            orderCellsTop: true,
            initComplete: function(settings, json) {
                const $table = $('#priceDataTable');
                if (!$table.parent().hasClass('table-scroll-container')) {
                    $table.wrap('<div class="table-scroll-container"></div>');
                }
            }
        };
        
        if (!withAlgorithm) {
            dataTableOptions.columnDefs = [
                { width: '5%', targets: 0 },
                { width: '15%', targets: 1 },
                { width: '10%', targets: 2 },
                { width: '10%', targets: 3 },
                { width: '10%', targets: 4 },
                { width: '10%', targets: 5 },
                { width: '20%', targets: 6 },
                { width: '20%', targets: 7 }
            ];
        }
        
        CommonSatiUI.InitRowsDataTable(
            '#priceDataTable',
            data,
            HistoryJS.ReturnTableColumns(withAlgorithm),
            dataTableOptions
        );
    },
    ReturnTableHeaders: (withAlgorithm) => {
        return withAlgorithm
        ? 
            `<tr >
                <!-- Columnas básicas -->
                <th rowspan="2" style="padding: 10px; text-align: center; width: 50px;">#</th>
                <th rowspan="2" style="padding: 10px; text-align: left; width: 130px;">Time</th>
                <th rowspan="2" style="padding: 10px; text-align: right;">Open</th>
                <th rowspan="2" style="padding: 10px; text-align: right;">High</th>
                <th rowspan="2" style="padding: 10px; text-align: right;">Low</th>
                <th rowspan="2" style="padding: 10px; text-align: right;">Close</th>

                <!-- Columnas WAM existentes -->
                <th colspan="2" style="padding: 10px; text-align: center; background: #4a55c4; color: white;">Compuesto</th>

                <!-- NUEVAS COLUMNAS DEL ALGORITMO -->
                <th colspan="4" style="padding: 10px; text-align: center; background: #667eea; color: white;">Algoritmo PMPn</th>
                <th colspan="2" style="padding: 10px; text-align: center; background: #f59e0b; color: white;">Rango Primario</th>
                <th rowspan="2" style="padding: 10px; text-align: center; background: #10b981; color: white;">Tendencia</th>
                <th colspan="3" style="padding: 10px; text-align: center; background: #8b5cf6; color: white;">Separación</th>
                <th rowspan="2" style="padding: 10px; text-align: center; background: #ef4444; color: white; min-width: 100px;">
                    Señal</th>
                    <th rowspan="2" style="padding: 10px; text-align: center; background: #06b6d4; color: white;">IMP<sub>i</sub></th>
                </tr>
                <tr>
                    <!-- Subencabezados WAM -->
                    <th style="padding: 8px; background: #4a55c4; color: white;">WAM</th>
                    <th style="padding: 8px; background: #4a55c4; color: white;">%</th>

                    <!-- Subencabezados Algoritmo -->
                    <th style="padding: 8px; background: #667eea; color: white;">PMP<sub>n</sub></th>
                    <th style="padding: 8px; background: #667eea; color: white;">max(P)</th>
                    <th style="padding: 8px; background: #667eea; color: white;">min(P)</th>
                    <th style="padding: 8px; background: #667eea; color: white;">PMP<sub>n-1</sub></th>

                    <!-- Subencabezados Rango -->
                    <th style="padding: 8px; background: #f59e0b; color: white;">RP<sup>+</sup></th>
                    <th style="padding: 8px; background: #f59e0b; color: white;">RP<sup>-</sup></th>

                    <!-- Subencabezados Separación -->
                    <th style="padding: 8px; background: #8b5cf6; color: white;">Dif<sub>n</sub></th>
                    <th style="padding: 8px; background: #8b5cf6; color: white;">Prom</th>
                    <th style="padding: 8px; background: #8b5cf6; color: white;">σ</th>
                </tr>
            `
        :
            `
                <!-- ✅ Primera fila: Encabezados agrupados -->
                <tr>
                    <th rowspan="2" style="padding: 10px; text-align: center; vertical-align: middle;">#</th>
                    <th rowspan="2" style="padding: 10px; text-align: left; vertical-align: middle;">Time</th>
                    <th rowspan="2" style="padding: 10px; text-align: right; vertical-align: middle;">Open</th>
                    <th rowspan="2" style="padding: 10px; text-align: right; vertical-align: middle;">High</th>
                    <th rowspan="2" style="padding: 10px; text-align: right; vertical-align: middle;">Low</th>
                    <th rowspan="2" style="padding: 10px; text-align: right; vertical-align: middle;">Close</th>
                    <th colspan="2" style="padding: 10px; text-align: center; background: #4a55c4; color: white;">Compuesto</th>
                </tr>
                <!-- ✅ Segunda fila: Subcolumnas -->
                <tr>
                    <th style="padding: 8px; text-align: right; background: #4a55c4; color: white;">WAM</th>
                    <th style="padding: 8px; text-align: right; background: #4a55c4; color: white;">%</th>
                </tr>
            `;
    },
    ReturnTableColumns: (withAlgorithm) => {
        let tableColumns = [
            { data: null, orderable: false, render: (d, t, r, meta) => meta.row + 1 },
            { 
                data: 'time',  // ✅ TIME original del broker (sin ajustes)
                render: (data, type, row) => {
                    // Hora correcta del histórico sin ajustes de zona horaria
                    return data || 'N/A';
                }
            },
            { data: 'open', render: $.fn.dataTable.render.number(',', '.', 5) },
            { data: 'high', render: $.fn.dataTable.render.number(',', '.', 5) },
            { data: 'low', render: $.fn.dataTable.render.number(',', '.', 5) },
            { data: 'close', render: $.fn.dataTable.render.number(',', '.', 5) },
            { data: 'calculatedWAM', render: $.fn.dataTable.render.number(',', '.', 5) },
            { 
                data: 'percentageDifference',
                render: (data) => {
                    const val = parseFloat(data);
                    const color = val >= 0 ? '#10b981' : '#ef4444';
                    const sign = val >= 0 ? '+' : '';
                    return `<span style="color: ${color}">${sign}${Math.abs(val).toFixed(4)}%</span>`;
                }
            }
        ];

        if (withAlgorithm) {
            tableColumns.push(
                { data: 'pmPn', render: (d) => d ? $.fn.dataTable.render.number(',', '.', 5).display(d) : '—' },
                { data: 'maxP', render: (d) => d ? $.fn.dataTable.render.number(',', '.', 5).display(d) : '—' },
                { data: 'minP', render: (d) => d ? $.fn.dataTable.render.number(',', '.', 5).display(d) : '—' },
                { data: 'previousPMPn', render: (d) => d ? $.fn.dataTable.render.number(',', '.', 5).display(d) : '—' },
                { data: 'rpPlus', render: (d) => d ? $.fn.dataTable.render.number(',', '.', 4).display(d) + '%' : '—' },
                { data: 'rpMinus', render: (d) => d ? $.fn.dataTable.render.number(',', '.', 4).display(d) + '%' : '—' },
                { 
                    data: 'tendencia',
                    render: (data) => {
                        // Backend usa "ALZA", "BAJA", "NEUTRO"
                        const colors = { 'ALZA': '#28a745', 'BAJA': '#dc3545', 'NEUTRO': '#6c757d' };
                        const icons = { 'ALZA': '↗', 'BAJA': '↘', 'NEUTRO': '→' };
                        const color = colors[data] || '#6c757d';
                        const icon = icons[data] || '→';
                        return `<span style="color: ${color}; font-weight: 600;">${icon} ${data}</span>`;
                    }
                },
                { data: 'difn', render: (d) => d ? $.fn.dataTable.render.number(',', '.', 5).display(d) : '—' },
                { data: 'promDifn', render: (d) => d ? $.fn.dataTable.render.number(',', '.', 5).display(d) : '—' },
                { data: 'sigmaDifn', render: (d) => d ? $.fn.dataTable.render.number(',', '.', 5).display(d) : '—' },
                { 
                    data: 'signal',
                    render: (data) => {
                        if (data === 'COMPRA') {
                            return '<span style="background: #28a745; color: white; padding: 4px 10px; border-radius: 4px; font-weight: 700;">🟢 COMPRA</span>';
                        }
                        if (data === 'VENTA') {
                            return '<span style="background: #dc3545; color: white; padding: 4px 10px; border-radius: 4px; font-weight: 700;">🔴 VENTA</span>';
                        }
                        return '<span style="background: #e2e3e5; color: #383d41; padding: 4px 10px; border-radius: 4px;">⚪ —</span>';
                    }
                },
                { 
                    data: 'importeAcumulacion', 
                    render: (d) => d ? $.fn.dataTable.render.number(',', '.', 2).display(d) : '—'
                }
            );
        }

        return tableColumns;
    },
    DownloadExcel: () => {
        const symbol = $('#symbolDropdown').val();
        const dateFilter = $('#dateFilter').val();
        const wamPeriod = $('#wamPeriodDropdown').val();

        if (!symbol || !dateFilter) {
            alert('Por favor selecciona un símbolo y una fecha');
            return;
        }

        // Deshabilitar botón y mostrar loading
        $('#btnDownloadExcel').prop('disabled', true).text('🔄 Descargando...');
        CommonSatiUI.ShowLoading();

        let url = '/HistoryMT';
        let params = {
            dateFilter: dateFilter,
            symbolStr: symbol,
            wamPeriod: wamPeriod
        };
        
        if (HistoryJS.lastLoadedWithAlgorithm) {
            url += '/GetDatePriceHistoryExcelWithAlgorithm';
            // Agregar parámetros del algoritmo
            params.pt = $('#paramPt').val();
            params.pr = $('#paramPr').val();
            params.sigma = $('#paramSigma').val();
            params.mp = $('#paramMp').val();
            params.fd = $('#paramFd').val();
        } else {
            url += '/GetDatePriceHistoryExcel';
        }

        $.ajax({
            url: url,
            type: 'GET',
            data: params,
            xhrFields: {
                responseType: 'blob'
            },
            success: function(blob) {
                // Crear un link temporal para descargar el blob
                const link = document.createElement('a');
                const blobUrl = window.URL.createObjectURL(blob);
                
                const modeText = HistoryJS.lastLoadedWithAlgorithm ? 'Algoritmo' : 'Basico';
                const fileName = `Historial_${symbol}_${dateFilter.replace(/-/g, '')}_${modeText}.xlsx`;
                
                link.href = blobUrl;
                link.download = fileName;
                document.body.appendChild(link);
                link.click();
                
                // Limpiar
                document.body.removeChild(link);
                window.URL.revokeObjectURL(blobUrl);
                
                console.log('Archivo Excel descargado:', fileName);
            },
            error: function(xhr, status, error) {
                console.error('Error descargando Excel:', error);
                alert('Error al descargar el archivo Excel. Verifica que el servidor esté funcionando.');
            },
            complete: function() {
                // Ocultar loading y rehabilitar botón
                CommonSatiUI.HideLoading();
                $('#btnDownloadExcel').prop('disabled', false).text('📊 Descargar Excel');
            }
        });
    }
}


function getTendenciaIcon(tendencia)
            {
                if (tendencia === "ALZA") return "↗ ALZA";
                if (tendencia === "BAJA") return "↘ BAJA";
                return "→ NEUTRO";
            }

            function getTendenciaStyle(tendencia)
            {
                if (tendencia === "ALZA") return "color: #28a745;";
                if (tendencia === "BAJA") return "color: #dc3545;";
                return "color: #6c757d;";
            }
        
            function getSignalBadge(signal)
            {
                if (signal === "COMPRA") {
                    return '<span style="background: #28a745; color: white; padding: 4px 10px; border-radius: 4px; font-weight: 700;">🟢 COMPRA</span>';
                }
                if (signal === "VENTA") {
                    return '<span style="background: #dc3545; color: white; padding: 4px 10px; border-radius: 4px; font-weight: 700;">🔴 VENTA</span>';
                }
                return '<span style="background: #e2e3e5; color: #383d41; padding: 4px 10px; border-radius: 4px;">⚪ —</span>';
            }