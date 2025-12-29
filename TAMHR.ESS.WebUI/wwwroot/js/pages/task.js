(function ($, app, kendo, undefined) {
    var selections = [];

    app.registerHandler("executeHandler", function () {
        app.confirm(app.translate("Are you sure to approve selected documents?"), function (d) {
            if (!d.value) return;

            app.blockUI({ boxed: true });
            $.post(app.resolveUrl("~/api/workflow/multiple/approve"), selections, function () {
                app.flash(app.translate("Success approve selected documents"));
                window.location.reload();
                app.unblockUI();
            });
        });
    });

    app.registerHandler("checkAllHandler", function () {
        var $this = $(this),
            checked = $this.is(":checked"),
            $cardBody = $this.closest(".card").find(".card-body"),
            $cbs = $cardBody.find("input:enabled[type='checkbox']");

        $cbs.prop("checked", checked);

        selections = [];

        if (checked) {
            $("#executeBtn").removeClass("disabled");

            $.each($cbs, function (key, val) {
                var value = $(val).val();

                selections.push(value);
            });
        } else {
            $("#executeBtn").addClass("disabled");
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

        if (selections.length > 0) {
            $("#executeBtn").removeClass("disabled");
        } else {
            $("#executeBtn").addClass("disabled");
        }
    });
})(jQuery, app, kendo);