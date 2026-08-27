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
            // scrollX se define en customOptions según sea necesario
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
         * Convierte la hora del broker sin reescalar la zona del navegador.
         * La referencia del tiempo debe ser la de MetaTrader, no la del cliente.
         */
        if (!timeStr) {
            console.warn('GetOriginalTimestamp: timeStr es null o undefined');
            return 0;
        }

        try {
            let dateStr = String(timeStr).trim();
            if (!dateStr) return 0;

            if (dateStr.includes(".")) {
                const [datePart, timePart = "00:00:00"] = dateStr.split(" ");
                const normalizedDate = datePart.replace(/\./g, "-");
                const normalizedTime = timePart.split(".")[0];
                dateStr = `${normalizedDate}T${normalizedTime}`;
            }

            const parts = dateStr.split("T");
            const dateParts = parts[0].split("-");
            const timeParts = parts[1] ? parts[1].split(":") : ["00", "00", "00"];

            const timestamp = Date.UTC(
                parseInt(dateParts[0]),
                parseInt(dateParts[1]) - 1,
                parseInt(dateParts[2]),
                parseInt(timeParts[0]),
                parseInt(timeParts[1]),
                parseInt(timeParts[2].split('.')[0])
            ) / 1000;

            return timestamp;
        } catch (error) {
            console.error('Error en GetOriginalTimestamp:', error, timeStr);
            return 0;
        }
    }
}