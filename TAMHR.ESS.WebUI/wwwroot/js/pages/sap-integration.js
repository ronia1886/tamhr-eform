(function ($, app, kendo, undefined) {
    var selections = [];

    function s2ab(s) {
        var buf = new ArrayBuffer(s.length);
        var view = new Uint8Array(buf);
        for (var i = 0; i != s.length; ++i) view[i] = s.charCodeAt(i) & 0xFF;
        return buf;
    }

    function ajaxDownload(url, data) {
        xhttp = new XMLHttpRequest();
        xhttp.onreadystatechange = function () {
            var a;
            if (xhttp.readyState === 4 && xhttp.status === 200) {
                a = document.createElement('a');
                var fileName = xhttp.getResponseHeader("fileName");
                a.href = window.URL.createObjectURL(xhttp.response);
                a.download = fileName;
                document.body.appendChild(a);
                a.click();
                document.body.removeChild(a);
            }
        };

        xhttp.open("POST", app.resolveUrl(url));
        xhttp.setRequestHeader("Content-Type", "application/json");
        xhttp.setRequestHeader("RequestVerificationToken", REQUEST_VERIFICATION_TOKEN);
        xhttp.responseType = 'blob';
        xhttp.send(JSON.stringify(data));
    }

    app.registerHandler("executeHandler", function () {
        
    });

    app.registerHandler("excelHandler", function (parameters) {
        ajaxDownload("~/api/sapintegration/export", { documentApprovalIds: selections });
    });

    app.registerHandler("singleDownloadHandler", function (parameters) {
        ajaxDownload("~/api/sapintegration/export", { documentApprovalIds: [ parameters.id ] });
    });

    app.registerHandler("checkAllHandler", function () {
        var $this = $(this),
            checked = $this.is(":checked"),
            $cardBody = $this.closest(".card").find(".card-body"),
            $cbs = $cardBody.find("input[type='checkbox']");

        selections = [];

        $cardBody.find("input[type='checkbox']").prop("checked", checked);

        if (checked) {
            $.each($cbs, function (key, val) {
                var value = $(val).val();

                selections.push(value);
            });
        }
    });

    app.registerHandler("checkHandler", function () {
        var $this = $(this),
            value = $this.val();

        if (!$this.is(":checked")) {
            selections.splice($.inArray(value, selections), 1);
        } else {
            selections.push(value);
        }
    });
})(jQuery, app, kendo);