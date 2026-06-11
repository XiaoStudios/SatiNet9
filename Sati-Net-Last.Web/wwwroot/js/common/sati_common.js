$(document).ready(function() {
    CommonSatiUI.InitLoadingOverlay();
})

var CommonSatiUI = {
    InitLoadingOverlay: () => {
        if ($('#satiLoadingOverlay').length === 0) {
            const overlayHtml = `
                <div id="satiLoadingOverlay" class="sati-loading-overlay">
                    <div class="sati-loading-content">
                        <div class="sati-spinner"></div>
                        <p class="sati-loading-text" id="satiLoadingText">Cargando...</p>
                    </div>
                </div>
            `;
            $('body').append(overlayHtml);
        }
    },
    ShowLoading: (message = 'Cargando datos...') => {
        $('#satiLoadingText').text(message);
        $('#satiLoadingOverlay').fadeIn(200);
    },
    HideLoading: () => {
        $('#satiLoadingOverlay').fadeOut(200);
    },
    InitFullDataTable: (tableId, customOptions = {}) => {
        CommonSatiUI.DestroyDataTable(tableId);

        const defaults = {
            ordering: false,
            paging: true,
            pageLength: 20,
            searching: true,
            dom: 'Bfrtip',
            info: true,
            autoWidth: false,
            scrollX: true,
            language: CommonSatiUI.GetSpanishLanguage(),
            buttons: []
        }

        return $(tableId).DataTable({ ...defaults, ...customOptions });
    },
    InitRowsDataTable: (tableId, data, columns, customOptions = {}) => {
        CommonSatiUI.DestroyDataTable(tableId);

        const defaults = {
            data: data,
            columns: columns,
            ordering: false,
            paging: true,
            pageLength: 20,
            searching: true,
            dom: 'frtip', // Sin botones, solo filtro + tabla + paginación
            info: true,
            autoWidth: false,
            scrollX: true,
            language: CommonSatiUI.GetSpanishLanguage(),
            // ✅ NO renderizar headers, usar los del HTML
            headerCallback: null,
            drawCallback: function(settings) {
                // Opcional: Callbacks después de renderizar
            }
        };
        
        return $(tableId).DataTable({ ...defaults, ...customOptions });
    },
    InitHeadersDataTable: (tableHeaderId, headersHtml) => {
        $(tableHeaderId).html(headersHtml);
    },
    GetSpanishLanguage: () => ({
        emptyTable: "No hay datos disponibles",
        zeroRecords: "No se encontraron resultados",
        search: "Buscar:",
        lengthMenu: "Mostrar _MENU_ registros",
        info: "Mostrando _START_ a _END_ de _TOTAL_ registros",
        infoEmpty: "Mostrando 0 a 0 de 0 registros",
        infoFiltered: "(filtrado de _MAX_ registros totales)",
        paginate: {
            first: "Primero",
            last: "Último",
            next: "Siguiente",
            previous: "Anterior"
        }
    }),
    DestroyDataTable: (tableId) => {
        if ($.fn.DataTable.isDataTable(tableId)) {
            $(tableId).DataTable().clear().destroy();
            $(tableId + ' tbody').empty();
        }
    },
    GetOriginalTimestamp: (timeStr) => {
        /**
         * Convierte fecha a timestamp Unix sin ajustes de zona horaria.
         * Interpreta la fecha tal cual como viene del servidor.
         * Usar para: datos históricos, gráficos del pasado, tablas de datos.
         * Formato esperado: "yyyy.MM.dd HH:mm:ss" o "yyyy-MM-dd HH:mm:ss"
         */
        if (!timeStr) {
            console.warn('GetOriginalTimestamp: timeStr es null o undefined');
            return 0;
        }

        try {
            let dateStr = timeStr;
            
            // Normalizar formato: "yyyy.MM.dd HH:mm:ss" → "yyyy-MM-ddTHH:mm:ss"
            if (typeof dateStr === "string") {
                dateStr = dateStr.replace(/\./g, "-").replace(" ", "T");
            }
            
            // Parsear componentes manualmente para evitar conversión de zona horaria
            const parts = dateStr.split("T");
            const dateParts = parts[0].split("-");
            const timeParts = parts[1] ? parts[1].split(":") : ["00", "00", "00"];
            
            // Crear fecha UTC directamente (sin ajuste de zona horaria local)
            const timestamp = Date.UTC(
                parseInt(dateParts[0]),     // año
                parseInt(dateParts[1]) - 1, // mes (0-indexed)
                parseInt(dateParts[2]),     // día
                parseInt(timeParts[0]),     // hora
                parseInt(timeParts[1]),     // minuto
                parseInt(timeParts[2])      // segundo
            ) / 1000;
            
            return timestamp;
        } catch (error) {
            console.error('Error en GetOriginalTimestamp:', error, timeStr);
            return 0;
        }
    },
    GetRealtimeTimestamp: (timeStr) => {
        /**
         * Convierte fecha a timestamp Unix para datos en tiempo real.
         * Aplica conversión de zona horaria del navegador.
         * Usar para: datos en vivo, feeds en tiempo real, operaciones actuales.
         * Formato esperado: "yyyy.MM.dd HH:mm:ss" o "yyyy-MM-dd HH:mm:ss"
         */
        if (!timeStr) {
            console.warn('GetRealtimeTimestamp: timeStr es null o undefined');
            return 0;
        }

        try {
            let dateStr = timeStr;
            
            // Normalizar formato
            if (typeof dateStr === "string" && dateStr.includes(".")) {
                dateStr = dateStr.replace(/\./g, "-").replace(" ", "T");
            }
            
            // Convertir con zona horaria local del navegador
            const timestamp = Math.floor(new Date(dateStr).getTime() / 1000);
            return timestamp;
        } catch (error) {
            console.error('Error en GetRealtimeTimestamp:', error, timeStr);
            return 0;
        }
    },
    GetUnixTimeStamp: (timeStr) => {
        /**
         * @deprecated Usar GetOriginalTimestamp o GetRealtimeTimestamp
         */
        console.warn('GetUnixTimeStamp está deprecado. Usar GetOriginalTimestamp o GetRealtimeTimestamp');
        return CommonSatiUI.GetRealtimeTimestamp(timeStr);
    }
}