(function ($, app) {
    var selections = [],
        saveUrl = app.resolveUrl("~/api/spkl-report/update-multiple");

    function getGrid() {
        return $("#grid").data("kendoGrid");
    }

    function getChart() {
        return $("#chart").data("kendoChart");
    }

    function calculateDiff(diff) {
        if (diff <= 0) return 0;

        var hour = Math.floor(diff / 60),
            remainder = diff % 60,
            adder = remainder >= 30 ? 0.5 : 0;

        return hour + adder;
    }

    function calculateDuration(dataItem) {
        var overtimeInPlan = dataItem.OvertimeIn,
            overtimeOutPlan = dataItem.OvertimeOut,
            normalIn = dataItem.NormalTimeIn,
            normalOut = dataItem.NormalTimeOut,
            overtimeInAdjust = dataItem.OvertimeInAdjust,
            overtimeOutAdjust = dataItem.OvertimeOutAdjust,
            overtimeBreakAdjust = parseInt(dataItem.OvertimeBreakAdjust),
            substractIn = 0,
            substractOut = 0;

        if (normalIn <= overtimeInAdjust && normalOut >= overtimeOutAdjust) return 0;

        //overtimeInAdjust = overtimeInAdjust < overtimeInPlan ? overtimeInPlan : overtimeInAdjust;
        //overtimeOutAdjust = overtimeOutAdjust > overtimeOutPlan ? overtimeOutPlan : overtimeOutAdjust;

        if (overtimeOutAdjust >= normalIn && overtimeInAdjust <= normalOut) {
            substractIn = overtimeInAdjust <= normalIn ? normalIn : overtimeInAdjust;
            substractOut = overtimeOutAdjust >= normalOut ? normalOut : overtimeOutAdjust;
        }

        return calculateDiff((overtimeOutAdjust - overtimeInAdjust) / 60000 - overtimeBreakAdjust - (substractOut - substractIn) / 60000);
    }
    
    function mapData(dataItem) {
        return {
            Id: dataItem.Id,
            OvertimeIn: kendo.toString(dataItem.OvertimeIn, 'yyyy-MM-dd HH:mm'),
            OvertimeOut: kendo.toString(dataItem.OvertimeOut, 'yyyy-MM-dd HH:mm'),
            OvertimeInAdjust: kendo.toString(dataItem.OvertimeInAdjust, 'yyyy-MM-dd HH:mm'),
            OvertimeOutAdjust: kendo.toString(dataItem.OvertimeOutAdjust, 'yyyy-MM-dd HH:mm'),
            NormalTimeIn: kendo.toString(dataItem.NormalTimeIn, 'yyyy-MM-dd HH:mm'),
            NormalTimeOut: kendo.toString(dataItem.NormalTimeOut, 'yyyy-MM-dd HH:mm'),
            OvertimeBreakAdjust: dataItem.OvertimeBreakAdjust,
            DurationAdjust: dataItem.DurationAdjust
        };
    }

    function saveItems(data, dirty, callback) {
        var $grid = getGrid();

        $grid.element.find("tr[data-uid]").removeClass("invalid");

        $.post(saveUrl, data, function () {
            callback();
        })
            .fail(function (xhr, status, error) {
            let response = xhr.responseJSON;
            let errors = {};

                if (response && response.errors) {


                    //for (let key in xhr.responseJSON) {
                    //    let index = key.substring(key.indexOf('[') + 1, key.indexOf(']')),
                    //        field = key.substring(key.indexOf('.') + 1);


                    //    if (!errors.hasOwnProperty(index)) {
                    //        errors[index] = {};
                    //    }

                    //    errors[index][field] = xhr.responseJSON[key][0];
                    //}
                    for (let key in response.errors) {
                        let index = key.substring(key.indexOf('[') + 1, key.indexOf(']'));
                        let field = key.includes('.')
                            ? key.substring(key.indexOf('.') + 1)
                            : key;

                        if (!errors.hasOwnProperty(index)) {
                            errors[index] = {};
                        }
                        errors[index][field] = response.errors[key][0];
                    }

                    for (let key in errors) {
                        let index = parseInt(key),
                            uid = dirty[index].uid;

                        $grid.element.find("tr[data-uid='" + uid + "']").addClass("invalid");
                    }

                    let allMessages = [];
                    for (let i in errors) {
                        allMessages.push(...Object.values(errors[i]));
                    }
                    app.error(allMessages.join(", "));
                } else {
                    app.error("Please make sure that OT in and OT out is not empty, OT out is greater than or equal to OT in, and duration adjust cannot be 0");
                }
        });
    }

    app.registerHandler("downloadReportHandler", function (parameters) {
        var startDate = kendo.toString($("#StartDate").data("kendoDatePicker").value() || new Date(1800, 0, 1), "yyyy-MM-dd"),
            endDate = kendo.toString($("#EndDate").data("kendoDatePicker").value() || new Date(9999, 12, 31), "yyyy-MM-dd"),
            type = parameters.type,
            url = {
                "pdf": "~/timemanagement/spklreport/downloadreport?startDate=" + startDate + "&endDate=" + endDate,
                "xlsx": "~/api/spkl-report/download-report?startDate=" + startDate + "&endDate=" + endDate
            };

        window.open(app.resolveUrl(url[type]), "_blank");
    });

    //app.registerHandler("downloadHandler", function () {
    //    async function showInput(selectHtml) {
    //        const { value: formValues } = await swal({
    //            title: 'Download Parameter',
    //            html: selectHtml + '<select data-id="keyDate" class="swal2-select w-100"></select>',
    //            preConfirm: () => {
    //                return [$("[data-id='parentId']").val(), $("[data-id='keyDate']").val()];
    //            },
    //            onOpen: function () {
    //                $("[data-id='parentId']").trigger("change");
    //            },
    //            showCancelButton: true,
    //            inputValidator: (value) => {
    //                if (!value[0]) return 'Document Number is mandatory!';
    //                if (!value[1]) return 'Date is mandatory!';

    //                return !kendo.parseDate(value[1], ['d/M/yyyy', 'dd/MM/yyyy']) && 'Invalid date format. Correct format is (D/M/YYYY or DD/MM/YYYY)';
    //            }
    //        });

    //        if (formValues) {
    //            window.open(app.resolveUrl("~/timemanagement/spklreport/download?parentId=" + formValues[0] + "&keyDateStr=" + formValues[1]), "_blank");
    //        }
    //    }

    //    let keyDate = kendo.toString($("#StartDate").data("kendoDatePicker").value(), "yyyy-MM");

    //    $.get(app.resolveUrl("~/api/spkl-report/get-documents"), { keyDate: keyDate }, function (d) {
    //        if (d.length === 0) {
    //            app.error("There is no document to download");

    //            return;
    //        }

    //        let sb = '<select data-id="parentId" class="swal2-select w-100" onchange="$.dataChange(this)">';

    //        if (d) {
    //            for (let i = 0; i < d.length; i++) {
    //                sb += '<option value="' + d[i].Id + '">' + d[i].DocumentNumber + '</option>';
    //            }
    //        }

    //        sb += '</select>';

    //        showInput(sb);
    //    });
    //});

    app.registerHandler("updateInputHandler", async function (parameters) {
        var $this = $(this),
            $tr = $this.closest("tr"),
            $grid = getGrid(),
            dataItem = $grid.dataItem($tr),
            defaultField = parameters.defaultfield,
            field = parameters.field,
            tempValue = dataItem.get(field),
            defaultValue = tempValue !== null && typeof tempValue !== 'undefined' ? tempValue : dataItem.get(defaultField),
            inputValue = kendo.toString(defaultValue, !parameters.format ? null : parameters.format);

        const { value: value } = await swal({
            title: 'Enter value',
            input: 'text',
            inputValue: inputValue,
            showCancelButton: true,
            inputValidator: (value) => {
                if (!value) return 'Input is mandatory!';
                if (!parameters.format) return null;

                return !/^$|^(([01][0-9])|(2[0-3])):[0-5][0-9]$/.test(value) && 'Not valid time. Valid time is in (HH:mm) format';
            }
        });

        if (value) {
            dataItem.set(field, parameters.format ? new Date(kendo.toString(defaultValue, 'yyyy/MM/dd') + ' ' + value) : value);

            dataItem.set("DurationAdjust", calculateDuration(dataItem));

            $(".save-btn").prop("disabled", false);
        }
    });

    app.registerHandler("saveHandler", function () {
        var $grid = getGrid(),
            dataSource = $grid.dataSource.data(),
            dirty = $.grep(dataSource, function (item) { return item.dirty; }),
            data = $.map(dirty, mapData);

        saveItems(data, dirty, function () {
            app.success(app.translate("Success update items"));
            app.refreshControl($grid.element);
        });
    });

    app.registerHandler("postHandler", function (parameters) {
        var $this = $(this),
            action = parameters.action,
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
            dataSource = $grid.dataSource.data(),
            dirty = $.grep(dataSource, function (item) { return item.dirty || uids.findIndex(x => x.documentApprovalId === item.DocumentApprovalId) >= 0; }),
            remarks = null,
            data = $.map(dirty, mapData);

        $grid.element.find("tr[data-uid]").removeClass("invalid");

        function callback() {
            app.blockUI({ boxed: true });
            
            $.post(workflowUrl, documentApprovalIds, function () {
                app.refreshControl($grid.element);
                app.success(app.translate(kendo.format("Success {0} document", app.translate(action))));
                app.unblockUI();
            });
        }

        if (action !== 'reject') {
            app.confirm(kendo.format(app.translate("Are you sure to {0} this request?"), app.translate(action)), function (d) {
                if (d.value) {
                    saveItems(data, dirty, callback);
                }
            });
        } else {
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
            field: "OvertimeDate",
            operator: "gte",
            value: startDate
        }, {
            field: "OvertimeDate",
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
                field: 'OvertimeCategoryCode',
                operator: 'eq',
                value: category === '%' ? '' : category
            }], period);

        dataSource.filter(filters);
    };

    $(function () {
        $.dataChange = function (e) {
            var $e = $(e),
                val = $e.val(),
                $target = $("[data-id='keyDate']"),
                sb = '';

            $.get(app.resolveUrl("~/api/spkl-report/get-dates"), { parentId: val }, function (d) {
                $e.closest(".swal2-content")
                    .next(".swal2-actions")
                    .find("button.swal2-confirm")
                    .prop("disabled", d.length === 0);

                for (let i = 0; i < d.length; i++) {
                    sb += '<option value="' + d[i] + '">' + d[i] + '</option>';
                }

                $target.html(sb);
            });
        };

        $('#search').on('keyup', function (e) {
            var $this = $(this),
                value = $this.val();

            if (e.keyCode === 13) {
                $.filterChange();

                e.preventDefault();
            }
        });

        //$('input[name="Category"]').change(function () {
        //    var value = this.value,
        //        $view = $("[data-role='view']");

        //    $view.hide();
        //    $view.filter("[data-value='" + value + "']").show();
        //});

        //$('input:radio[name="Category"]').filter('[value="date"]')
        //    .attr('checked', true)
        //    .trigger('change');

        $.stuff = function (value, index, length) {
            return (value || "").substring(index, index + length);
        };
    });

})(jQuery, app);