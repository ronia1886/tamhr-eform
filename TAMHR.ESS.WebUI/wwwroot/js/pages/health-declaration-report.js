(function ($, app) {
    let latestSubmissionDate = "";

    const Toast = Swal.mixin({
        toast: true,
        position: 'top-end',
        showConfirmButton: false,
        timer: 3000,
        onOpen: (toast) => {
            toast.addEventListener('mouseenter', Swal.stopTimer)
            toast.addEventListener('mouseleave', Swal.resumeTimer)
        }
    });

    function getGrid() {
        return $("#grid").data("kendoGrid");
    }

    function getSelectedDate() {
        return kendo.toString($("#SubmissionDate").data("kendoDatePicker").value(), 'yyyy-MM-dd');
    }

    $.onSwitchChange = function (el) {
        let $el = $(el),
            checked = $el.is(":checked"),
            val = $el.val();

        if (checked) {
            $.post(app.resolveUrl("~/api/health-declaration-report/change-status"), { Value: val }, function () {
                $el.prop("disabled", true);

                Toast.fire({
                    type: 'success',
                    title: 'Update status successfully'
                });
            });
        } else {
            app.error("Cannot update this status because already checked");
        }
    };

    $.download = function () {
        window.open(app.resolveUrl("~/api/health-declaration-report/download?submissionDate=" + getSelectedDate()), "_blank");
    };

    $.getData = function () {
        return {
            "submissionDate": kendo.toString($("#SubmissionDate").data("kendoDatePicker").value(), 'yyyy-MM-dd')
        };
    };

    $.onDataBound = function () {
        let $grid = this,
            submissionDate = kendo.toString($("#SubmissionDate").data("kendoDatePicker").value(), "yyyy-MM-dd");

        $grid.element.find(".sick-group").closest("tr").addClass("bg-yellow-crusta font-white");
        $grid.element.find(".has-remarks").closest("tr").addClass("bg-grey-mint font-white");

        if (latestSubmissionDate !== submissionDate) {
            if ($("#chart").length > 0 && $("#secondChart").length > 0) {
                $("#chart").data("kendoChart").dataSource.read();
                $("#secondChart").data("kendoChart").dataSource.read();
            }

            latestSubmissionDate = submissionDate;
        }

        $.initFixedHeader.call(this);
    };

    $.filterChange = function () {
        var $grid = getGrid();

        if (typeof $grid === 'undefined' || typeof $grid.dataSource === 'undefined') return;

        var dataSource = $grid.dataSource,
            name = $("#search").val(),
            workTypeCode = $("#WorkType").val(),
            type = $("#Type").val(),
            filters = [{
                field: 'WorkTypeCode',
                operator: 'eq',
                value: workTypeCode === '%' ? '' : workTypeCode
            }];

        if (type !== "") {
            if (type === "need-monitoring") {
                filters.push({
                    field: 'HealthTypeCode',
                    operator: 'eq',
                    value: 'ht-sick'
                });
            } else if (type === "not-submitted") {
                filters.push({
                    field: 'HasSubmitForm',
                    operator: 'eq',
                    value: false
                });
            } else if (type === "healthy") {
                filters.push({
                    field: 'HealthTypeCode',
                    operator: 'eq',
                    value: 'ht-not-sick'
                });
            } else if (type === "with-notes") {
                filters.push({
                    field: 'HasRemarks',
                    operator: 'eq',
                    value: true
                });
            } else if (type === "submitted") {
                filters.push({
                    field: 'HasSubmitForm',
                    operator: 'eq',
                    value: true
                });
            }
        }

        filters.push({
            logic: "or",
            filters: [
                {
                    field: 'Name',
                    operator: 'contains',
                    value: name
                },
                {
                    field: 'Division',
                    operator: 'contains',
                    value: name
                },
                {
                    field: 'Department',
                    operator: 'contains',
                    value: name
                },
                {
                    field: 'Section',
                    operator: 'contains',
                    value: name
                },
                {
                    field: 'PostName',
                    operator: 'contains',
                    value: name
                },
                {
                    field: 'JobName',
                    operator: 'contains',
                    value: name
                },
                {
                    field: 'Email',
                    operator: 'contains',
                    value: name
                },
                {
                    field: 'PhoneNumber',
                    operator: 'contains',
                    value: name
                }
            ]
        });

        dataSource.filter(filters);
    };

    $('#search').on('keyup', function (e) {
        let $this = $(this),
            value = $this.val();

        if (e.keyCode === 13) {
            $.filterChange();

            e.preventDefault();
        }
    });
})(jQuery, app);