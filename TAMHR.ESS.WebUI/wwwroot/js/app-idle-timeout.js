(function ($, app) {
    var $countdown,
        idleAfter = app.get("timeout_idle_after"),
        timeout = app.get("timeout_interval"),
        pollingInterval = app.get("timeout_polling_interval"),
        failedRequests = app.get("timeout_failed_requests");

    function redirect() {
        app.warning("Your session has been expired, you will be redirecting to login page");
        setTimeout(function () {
            window.location = app.resolveUrl("~/core/account/logout");
        }, 1000);
    }

    $(function () {
        $.idleTimeout('#idle-timeout-modal', '.modal-content button:last', {
            idleAfter: idleAfter,
            timeout: timeout,
            pollingInterval: pollingInterval,
            failedRequests: failedRequests,
            keepAliveURL: app.resolveUrl("~/api/common/keepalive"),
            onTimeout: function () {
                redirect();
            },
            onAbort: function () {
                redirect();
            },
            onIdle: function () {
                $('#idle-timeout-modal').modal('show');
                $countdown = $('#idle-timeout-counter');

                $('#idle-timeout-modal-keepalive').on('click', function () {
                    $('#idle-timeout-modal').modal('hide');
                });

                $('#idle-timeout-modal-logout').on('click', function () {
                    $('#idle-timeout-modal').modal('hide');
                    $.idleTimeout.options.onTimeout.call(this);
                });
            },
            onCountdown: function (counter) {
                $countdown.html(counter);
            }
        });
    });
})(jQuery, app);