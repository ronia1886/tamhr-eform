(function ($, app) {
    let tempId = app.get("tempId"),
        documentApprovalId = app.get("documentApprovalId"),
        filterType = app.get("filterType");

    function getGrid() {
        return $("#grid").data("kendoGrid");
    }

    app.registerHandler("downloadHandler", function () {
        async function showPrompt(selectHtml) {
            const { value: formValues } = await swal({
                title: 'Select Date',
                html: selectHtml,
                showCancelButton: true,
                preConfirm: () => {
                    return [$("[data-id='keyDate']").val()];
                }
            });

            if (formValues) {
                window.open(app.resolveUrl("~/timemanagement/form/downloadspklplan?parentId=" + documentApprovalId + "&keyDateStr=" + formValues[0]), "_blank");
            }
        }

        $.get(app.resolveUrl("~/api/spkl-report/get-plan-dates"), { parentId: documentApprovalId }, function (d) {
            if (d.length === 0) {
                app.error("There is no items to download");

                return;
            }

            let sb = '<select data-id="keyDate" class="swal2-select w-100">';

            for (var i = 0; i < d.length; i++) {
                sb += '<option value="' + d[i] + '">' + d[i] + '</option>';
            }

            sb += '</select>';

            showPrompt(sb);
        });
    });

    function uploadSPKLPlan(formData, $fileInput, $fileInputWrapper) {
        app.blockUI({ boxed: true });
        $.ajax({
            url: app.resolveUrl("~/api/spkl-overtime/upload"),
            data: formData,
            cache: false,
            contentType: false,
            processData: false,
            type: 'POST'
        }).done(function (data) {
            if (data.TotalSuccess === data.TotalUpload && data.TotalSuccess > 0) {
                app.success(app.translate("Success upload all data"));
            }
            else if (data.TotalSuccess < data.TotalUpload && data.TotalSuccess > 0) {
                app.warning(kendo.format(app.translate("Success upload {0} of {1} data, with warning"), data.TotalSuccess, data.TotalUpload) + ":<br/>" + data.Messages.join("<br/>"));
            }
            else {
                app.error(app.translate("Failed upload SPKL plan") + ":<br/>" + data.Messages.join("<br/>"));
            }

            $fileInput.val("");
            $fileInputWrapper.val("");
            app.refreshControl("grid");
        });
        app.unblockUI();
    }

    function updateSPKLPlan(formData, $tbl) {
        app.blockUI({ boxed: true });

        $.post(app.resolveUrl("~/api/spkl-overtime/update"), formData)
            .done(function (d) {
                console.log("API Response (Success):", d);

                if (typeof d === "string") {
                    d = JSON.parse(d); 
                }

                app.success(d.message);
            })
            .fail(function (jqXHR) {
                var errors = jqXHR.responseJSON || {};
                console.error("API Response (Fail):", errors);

                // Check if response contains a warning
                if (jqXHR.status === 400 && errors.message) {
                    if (errors.status && errors.status.toLowerCase() === "warning") {
                        app.warning(errors.message);  
                    } else {
                        app.error(errors.message); 
                    }
                } else {
                    app.error("An unexpected error occurred. Please try again.");
                }
            })
            .always(function () {
                app.unblockUI();
                app.refreshControl($tbl);
                $tbl.find("input[type='checkbox']").prop("checked", false);
            });
    }


    app.registerHandler("updateSpklHandler", function () {
        console.log("masuk handler")
        var noregs = [],
            $this = $(this),
            $form = $this.closest("form"),
            $tbl = $("#grid"),
            formData = $form.serializeObject();

        if (!formData.ids || formData.ids.length === 0) {
            formData.ids = [];
            app.error(app.translate("Employee must be selected"));
            return;
        }

        formData.Object.MustBeValidated = true;
        formData.Object.NoRegs = formData.ids;

        app.clearValidation($form);

        app.blockUI({ boxed: true });
        $.post(app.resolveUrl("~/api/spkl-overtime/validate-overtime-duration"), formData, function (d) {
            app.unblockUI();
            var remainingOvertimeDuration = $(".hfRemaining").first().val();
            if (d.TotalOvertimeDuration > parseFloat(remainingOvertimeDuration)) {
                app.confirm(
                    app.translate("Submit SPKL Plan"),
                    app.translate("The submitted total overtime duration (" + (kendo.toString(d.TotalOvertimeDuration, "n2")) + " hours) is more than the remaining overtime duration ("+ (kendo.toString(remainingOvertimeDuration, "n2")) +" hours). Are you sure you want to submit the plan?"),
                    function (d) {
                        if (!d.value) return;

                        updateSPKLPlan(formData, $tbl);
                    }
                );
            } else {
                updateSPKLPlan(formData, $tbl);
            }
            
        }).fail(function (jqXHR, textStatus, errorThrown) {
            var errors = jqXHR.responseJSON || {},
                $form = $this.closest("form");

            $form.validate(errors);
            app.unblockUI();
        });
    });

    app.registerHandler("uploadHandler", function () {
        var $this = $(this),
            allowedExtensions = ['xls', 'xlsx'],
            $fileInputWrapper = $("#Object_UploadExcelPath"),
            $fileInput = $this.closest("form").find("input[type='file']"),
            fileName = $fileInput.val();

        if (fileName.length === 0) {
            app.warning("Mohon pilih file.");

            return;
        }
        else {
            var extension = fileName.replace(/^.*\./, '');

            if ($.inArray(extension, allowedExtensions) === -1) {
                app.warning("Mohon pilih hanya file excel.");

                return;
            }
        }

        var formData = new FormData();
        formData.append("file", $fileInput[0].files[0]);
        formData.append("documentApprovalId", documentApprovalId);
        formData.append("tempId", tempId);

        var remainingOvertimeDuration = $(".hfRemaining").first().val();
        formData.append("remainingOvertimeDuration", parseFloat(remainingOvertimeDuration));

        app.blockUI({ boxed: true });
        $.ajax({
            url: app.resolveUrl("~/api/spkl-overtime/validate-upload"),
            data: formData,
            cache: false,
            contentType: false,
            processData: false,
            type: 'POST'
        }).done(function (data) {
            app.unblockUI();
            if (data.Messages.length > 0) {
                app.error(app.translate("Failed to upload SPKL plan") + ":<br/>" + data.Messages.join("<br/>"));
            } else if (data.RemainingOvertimeDurationExceeded) {
                app.confirm(
                    app.translate("Upload SPKL Plan"),
                    app.translate("The uploaded total overtime duration (" + (kendo.toString(data.TotalOvertimeDuration, "n2")) + " hours) is more than the remaining overtime duration ("+ (kendo.toString(data.RemainingOvertimeDuration, "n2")) +" hours). Are you sure you want to upload the plan?"),
                    function (d) {
                        if (!d.value) return;

                        uploadSPKLPlan(formData, $fileInput, $fileInputWrapper);
                    }
                );
            } else {
                uploadSPKLPlan(formData, $fileInput, $fileInputWrapper);
            }
        });
    });

    $.filterChange = function () {
        var $grid = getGrid();

        if (typeof $grid === 'undefined' || typeof $grid.dataSource === 'undefined') return;

        var dataSource = $grid.dataSource,
            name = $("#search").val(),
            hasPlan = $("#HasPlan").val(),
            filters = [{
                field: 'Name',
                operator: 'contains',
                value: name
            }];

        if (hasPlan !== '') {
            filters.push({
                field: 'Total',
                operator: hasPlan === 'true' ? 'isnotnull' : 'isnull',
                value: hasPlan === 'true' ? 1 : 0
            });
        }

        dataSource.filter(filters);
    };

    $(function () {
        $(".toolbars a").parent().append('<a href="javascript:;" class="btn green-jungle btn-sm" data-trigger="handler" data-handler="downloadHandler"><i class="ft-download"></i></a>');

        $("input[type='file']").change(function () {
            var fileName = $(this).val();
            $("#Object_UploadExcelPath").val(fileName);
        });

        $('#grid').on("change", "#check-all", function () {
            var $this = $(this),
                checked = $this.is(":checked"),
                $tbody = $this.closest("table").children("tbody");

            $tbody.find("> tr > td > input[type='checkbox']").prop("checked", checked);
        });

        $('#search').on('keyup', function (e) {
            var $this = $(this),
                value = $this.val();

            if (e.keyCode === 13) {
                $.filterChange();

                e.preventDefault();
            }
        });

        $('input[name="Object.OvertimeType"]').change(function () {
            var value = this.value,
                $view = $("[data-role='view']");

            $view.hide();
            $view.filter("[data-value='" + value + "']").show();
        });

        $('input:radio[name="Object.OvertimeType"]').filter('[value="' + filterType + '"]')
            .attr('checked', true)
            .trigger('change');
    });
})(jQuery, app);