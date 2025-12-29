(function ($, app) {
    var selections = [];

    function getGrid() {
        return $("#grid").data("kendoGrid");
    }

    function getChart() {
        return $("#chart").data("kendoChart");
    }

    app.registerHandler("downloadReportHandler", function (parameters) {
        var startDate = getParameters().startDate,
            endDate = getParameters().endDate;

        window.open(app.resolveUrl("~/api/bdjk-report/download-report?startDate=" + startDate + "&endDate=" + endDate), "_blank");
    });

    app.registerHandler("postHandler", function (parameters) {
        var action = parameters.action,
            workflowUrl = app.resolveUrl("~/api/workflow/multiple/" + action),
            $grid = getGrid(),
            uids = $.map($("#grid").find("tbody input[type='checkbox']:checked"), function (item) {
                var obj = item.value.split("|");

                return {
                    uid: obj[0],
                    documentApprovalId: obj[1]
                };
            }),
            documentApprovalIds = uids.map(x => x.documentApprovalId),
            remarks = null;

        $grid.element.find("tr[data-uid]").removeClass("invalid");

        function callback() {
            app.blockUI({ boxed: true });

            $.post(workflowUrl, documentApprovalIds, function () {
                app.refreshControl($grid.element);
                app.success(app.translate(kendo.format("Success {0} document", app.translate(action))));
                app.unblockUI();
            });
        }

        if (action === 'reject') {
            swal({
                title: app.translate('Reject Remarks'),
                input: 'textarea',
                inputPlaceholder: app.translate('Please input reason for reject'),
                showCancelButton: true,
                inputValidator: (value) => {
                    return !value && app.translate('Remarks must be filled!');
                }
            }).then(function (result) {
                if (!result.value) return;

                remarks = result.value;

                callback();
            });
        }
        else {
            app.confirm(kendo.format(app.translate("Are you sure to {0} this request?"), app.translate(action)), function (d) {
                if (d.value) {
                    callback();
                }
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

        $(".action-btn").prop("disabled", selections.length === 0);
    });

    app.registerHandler("checkAllHandler", function () {
        var $this = $(this),
            checked = $this.is(":checked"),
            $cardBody = $this.closest(".k-grid").find(".k-grid-content-locked tbody"),
            $cbs = $cardBody.find("input:enabled[type='checkbox']");

        $cbs.prop("checked", checked);

        selections = [];

        if (checked) {
            $(".action-btn").prop("disabled", false);

            $.each($cbs, function (key, val) {
                var value = $(val).val();

                selections.push(value);
            });
        } else {
            $(".action-btn").prop("disabled", true);
        }
    });

    function getPeriod() {
        var startDate = kendo.toString($("#StartDate").data("kendoDatePicker").value() || new Date(1800, 0, 1), "yyyy-MM-dd"),
            endDate = kendo.toString($("#EndDate").data("kendoDatePicker").value() || new Date(9999, 12, 31), "yyyy-MM-dd");

        return [{
            field: "WorkingDate",
            operator: "gte",
            value: startDate
        }, {
            field: "WorkingDate",
            operator: "lte",
            value: endDate
        }];
    }

    $.filterChange = function () {
        var $grid = getGrid();

        if (typeof $grid === 'undefined' || typeof $grid.dataSource === 'undefined') return;

        var dataSource = $grid.dataSource,
            name = $("#search").val(),
            period = getPeriod(),
            category = $("#CategorySpkl").val(),
            bdjkCode = $("#BdjkCode").val().replace('bdjk', ''),
            documentStatus = $("#DocumentStatus").val(),
            filters = $.merge([{
                field: 'Name',
                operator: 'contains',
                value: name
            }, {
                field: 'DocumentStatusCode',
                operator: 'eq',
                value: documentStatus === '%' ? '' : documentStatus
            }, {
                field: 'ActivityCode',
                operator: 'eq',
                value: category === '%' ? '' : category
            }, {
                field: 'BdjkCode',
                operator: 'eq',
                value: bdjkCode === '%' ? '' : bdjkCode
            }], period);

        dataSource.filter(filters);
    };

    $(function () {
        $('#search').on('keyup', function (e) {
            var $this = $(this),
                value = $this.val();

            if (e.keyCode === 13) {
                $.filterChange();

                e.preventDefault();
            }
        });
    });
})(jQuery, app);