(function ($, app) {
    var selections = [];

    function getGrid() {
        return $("#grid").data("kendoGrid");
    }

    function getChart() {
        return $("#chart").data("kendoChart");
    }
    
    app.registerHandler("downloadReportHandler", function (parameters) {
        var startDate = kendo.toString($("#StartDate").data("kendoDatePicker").value() || new Date(1800, 0, 1), "yyyy-MM-dd"),
            endDate = kendo.toString($("#EndDate").data("kendoDatePicker").value() || new Date(9999, 12, 31), "yyyy-MM-dd"),
            type = parameters.type,
            url = {
                "xlsx": "~/api/maternity-leave-report/download-report?startDate=" + startDate + "&endDate=" + endDate
            };

        window.open(app.resolveUrl(url[type]), "_blank");
    });

    function getPeriod() {
        var startDate = kendo.toString($("#StartDate").data("kendoDatePicker").value() || new Date(1800, 0, 1), "yyyy-MM-dd"),
            endDate = kendo.toString($("#EndDate").data("kendoDatePicker").value() || new Date(9999, 12, 31), "yyyy-MM-dd");

        return [{
            field: "CreatedOn",
            operator: "gte",
            value: startDate
        }, {
            field: "CreatedOn",
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
            documentStatus = $("#DocumentStatus").val(),
            filters = $.merge([{
                field: 'Name',
                operator: 'contains',
                value: name
            }, {
                field: 'DocumentStatusCode',
                operator: 'eq',
                value: documentStatus === '%' ? '' : documentStatus
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

        $.stuff = function (value, index, length) {
            return (value || "").substring(index, index + length);
        };
    });

})(jQuery, app);