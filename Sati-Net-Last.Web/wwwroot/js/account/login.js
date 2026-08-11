$(document).ready(function () {
    LoginJS.Init();
});

var LoginJS =
{
    Init: () => {
        LoginJS.BindEvents();
        LoginJS.ClearError();
    },

    BindEvents: () => {
        $("#loginForm").on("submit", LoginJS.SubmitLogin);
    },

    SubmitLogin: (e) => {
        e.preventDefault();
        LoginJS.ClearError();

        const username = $("#Username").val()?.trim();
        const password = $("#Password").val()?.trim();
        const token = $("#loginForm input[name='__RequestVerificationToken']").val();
        const url = $("#loginForm").data("ajax-url");

        if (!username || !password) {
            LoginJS.ShowError("Ingresa tu usuario y contraseña.");
            return;
        }

        $.ajax({
            type: "POST",
            url: url,
            data: {
                __RequestVerificationToken: token,
                Username: username,
                Password: password
            },
            dataType: "json",
            beforeSend: function () {
                $("#btnLogin").prop("disabled", true).text("Ingresando...");
            },
            success: function (result) {
                if (result && result.success) {
                    window.location.href = result.redirectUrl;
                    return;
                }

                LoginJS.ShowError(result?.message || "No fue posible iniciar sesión.");
            },
            error: function (xhr) {
                if (xhr.status === 401) {
                    LoginJS.ShowError("Usuario o contraseña incorrectos.");
                    return;
                }

                if (xhr.status === 400) {
                    LoginJS.ShowError("Solicitud inválida o token antiforgery no válido.");
                    return;
                }

                LoginJS.ShowError("Error al conectar con el servidor.");
            },
            complete: function () {
                $("#btnLogin").prop("disabled", false).text("Ingresar");
            }
        });
    },

    ShowError: (message) => {
        $("#loginError").text(message).show();
    },

    ClearError: () => {
        $("#loginError").hide().text("");
    }
};