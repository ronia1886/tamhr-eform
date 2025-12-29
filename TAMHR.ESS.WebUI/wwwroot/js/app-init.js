(function ($, window, app, swal) {
    app.setAlertHandler(function (type, title, message) {
        swal(title, message, type);
    });

    app.setConfirmHandler(function (title, message, handler) {
        swal({
            title: title,
            text: message,
            type: "warning",
            showCancelButton: true,
            confirmButtonColor: '#3085d6',
            cancelButtonColor: '#d33',
            confirmButtonText: 'Yes'
        }).then((result) => {
            if (typeof handler === 'function') {
                handler.apply(this, [result]);
            }
        });
    });

    app.registerHandler("agreeHandler", function () {
        var $this = $(this);

        $this.closest(".modal")
            .find("a[data-trigger='handler']")
            .toggleClass("disabled");
    });

    app.registerHandler("submitAgreeHandler", function () {
        app.close(this);
        app.submitForm("mainForm", function (d) {
            var url = app.resolveUrl("~/api/workflow/initiate"),
                remarks = '';

            $.post(url, { DocumentApprovalId: d.id, Remarks: remarks }, function () {
                app.flash(app.translate("Success submit document"));
                window.location.href = app.resolveUrl("~/core/form/index?formKey=" + d.formKey);
            });
        });
    });

    app.registerHandler("redirectHandler", function () {
        var $this = $(this),
            url = app.resolveUrl($this.data("url"));

        window.location.href = url;
    });

    app.registerHandler("deleteHandler", function (parameters) {
        var $el = $(this),
            dataUrl = $el.data("url"),
            $component = $el.closest(".k-widget[data-role]");

        app.confirm(app.translate("Are you sure want to delete this object?"), app.translate("You won't be able to revert this!"), function (d) {
            if (!d.value) return;

            $.delete(dataUrl, { id: parameters.id }, function () {
                app.success("Success delete");
                app.refreshControl($component);
            });
        });
    });

    app.registerHandler("updateHandler", function () {
        var $this = $(this),
            reload = $this.data("reload") || false;

        app.close(this);

        if (reload) {
            window.sessionStorage.setItem("flashMessage", app.translate("Update success"));
            window.location.reload();
        } else {
            app.success(app.translate("Update success"));
            var $actions = $this.closest(".actions");

            if ($actions.length > 0) {
                var $sender = $actions.data("sender");

                if ($sender) {
                    var $el = $sender.data("target") || $sender.closest("[data-role]");

                    app.refreshControl($el);
                }
            }
        }
    });

    app.registerHandler("saveHandler", function (d) {
        if (d !== undefined && d.hasOwnProperty("formKey")) {
            window.sessionStorage.setItem("flashMessage", app.translate("Save success"));
            window.location.href = app.resolveUrl("~/core/form/view?formKey=" + d.formKey + "&id=" + d.id);
        } else {
            window.sessionStorage.setItem("flashMessage", app.translate("Save success"));
            window.location.reload();
        }
    });

    app.registerHandler("submitHandler", function (d, parameters) {
        var url = app.resolveUrl("~/api/workflow/" + parameters.action),
            remarks = null,
            returnUrl = app.resolveUrl(typeof parameters.redirecturl !== 'undefined' && parameters.redirecturl !== null ? parameters.redirecturl : "~/core/form/index?formKey=" + d.formKey);

        app.blockUI({ boxed: true });
        $.post(url, { DocumentApprovalId: d.id, Remarks: remarks }, function () {
            app.flash(app.translate("Success " + app.translate(parameters.action) + " document"));
            window.location.href = returnUrl;
            app.unblockUI();
        });
    });

    app.registerHandler("postHandler", function (parameters) {
        var action = parameters.action,
            reload = parameters.reload || true,
            redirect = typeof parameters.redirecturl !== 'undefined',
            url = app.resolveUrl("~/api/workflow/" + action),
            remarks = null;
        
        function callback() {
            app.blockUI({ boxed: true });

            $.post(url, { DocumentApprovalId: parameters.id, Remarks: remarks }, function (d) {
                if (reload) {
                    window.sessionStorage.setItem("flashMessage", kendo.format(app.translate("Success {0} document"), app.translate(action)));
                    if (redirect) {
                        window.location.href = app.resolveUrl(parameters.redirecturl);
                    } else {
                        window.location.reload();
                    }
                } else {
                    app.success(kendo.format(app.translate("Success {0} document"), app.translate(action)));
                    app.unblockUI();
                }
            });
        }

        if (action == "reject" || action == "cancel" || action == "revise") {
            var titledRemarks = {
                'cancel': {
                    title: app.translate('Drop Request Remarks'),
                    placeholder: app.translate('Please input reason for drop request')
                },
                'revise': {
                    title: app.translate('Revise Remarks'),
                    placeholder: app.translate('Please input reason for revise')
                },
                'reject': {
                    title: app.translate('Reject Remarks'),
                    placeholder: app.translate('Please input reason for reject')
                }
            };

            swal({
                title: titledRemarks.hasOwnProperty(action) ? titledRemarks[action].title : app.translate('Remarks'),
                input: 'textarea',
                inputPlaceholder: titledRemarks.hasOwnProperty(action) ? titledRemarks[action].placeholder : app.translate('Type your remarks here'),
                showCancelButton: true,
                inputValidator: (value) => {
                    return !value && app.translate('Remarks must be filled!')
                }
            }).then(function (result) {
                if (!result.value) return;

                remarks = result.value;

                callback();
            });
        } else {
            app.confirm(kendo.format(app.translate("Are you sure to {0} this request?"), app.translate(action)), function (d) {
                if (d.value) {
                    callback();
                }
            });
        }
    });

    app.registerHandler("proxyAsCallback", function () {
        var $context = this,
            $autoComplete = $context.find("input[data-role='autocomplete']").data("kendoAutoComplete");

        $autoComplete.value("");

        setTimeout(function () {
            $autoComplete.focus();
        }, 500);
    });

    app.registerHandler("impersonateHandler", function (d) {
        $.post(app.resolveUrl("~/api/account/impersonate"), { Username: d.username }, function () {
            window.location.href = app.resolveUrl("~/");
        });
    });

    app.registerHandler("becameSelfHandler", function () {
        $.post(app.resolveUrl("~/api/account/self"), {}, function () {
            window.location.href = app.resolveUrl("~/");
        });
    });

    app.registerHandler("switchOrganizationHandler", function (d) {
        $.post(app.resolveUrl("~/api/account/switch"), { Username: d.postcode }, function () {
            window.location.href = app.resolveUrl("~/");
        });
    });

    app.registerHandler("filterHandler", function () {
        var $this = $(this);

        $this.next("input").toggleClass("hidden").focus();
    });

    app.registerHandler("showAllRequestHandler", function () {
        var $this = $(this),
            $toolbars = $this.closest(".page-toolbars"),
            id = $toolbars.attr("id"),
            $component = $("[data-filter='" + id + "']");

        app.refreshControl($component);
    });

    $.rowNumber = function () {
        let rows = this.items(),
            grid = this,
            counter = 0;

        $(rows).not(".k-grouping-row").each(function () {
            let $this = $(this),
                index = (counter++) + 1 + ((grid.dataSource.pageSize() * (grid.dataSource.page() - 1)) || 0),
                rowLabel = $this.find(".row-number")[0];

            rowLabel.innerHTML = index + '.';
        });
    };

    $.initDraggable = function (selector, callback) {
        let grid = this,
            defaultSelector = selector || ">tbody >tr",
            table = this.table;

        table.kendoSortable({
            filter: defaultSelector,
            hint: function (element) {
                let width = element.width(),
                    table = $('<table style="width: ' + width + 'px;" class="k-grid k-widget"></table>'),
                    hint;

                table.append(element.clone());
                table.css("opacity", 0.7);

                return table;
            },
            cursor: "move",
            placeholder: function (element) {
                return $('<tr colspan="4" class="placeholder"></tr>');
            },
            change: function (e) {
                let skip = grid.dataSource.skip() || 0,
                    newIndex = e.newIndex + skip,
                    dataItem = grid.dataSource.getByUid(e.item.data("uid"));

                if (typeof callback !== 'undefined') {
                    callback.apply(grid, [newIndex, dataItem]);
                } else {
                    grid.dataSource.remove(dataItem);
                    grid.dataSource.insert(newIndex, dataItem);
                }
            }
        });
    };

    $.initFixedHeader = function (marginTop) {
        let wrapper = this.wrapper,
            header = wrapper.find(".k-grid-header"),
            root = wrapper.closest(".k-grid"),
            content = root.find(".k-grid-content"),
            cleanMarginTop = marginTop || 50;

        function resizeFixed() {
            let paddingRight = parseInt(header.css("padding-right"));

            header.css("width", wrapper.width() - paddingRight);
        }

        function scrollFixed() {
            var offset = $(this).scrollTop() + 50,
                tableOffsetTop = wrapper.offset().top,
                tableOffsetBottom = tableOffsetTop + wrapper.height() - header.height();

            if (offset < tableOffsetTop || offset > tableOffsetBottom) {
                header.removeClass("fixed-header");
                content.css("margin-top", 0);
            } else if (offset >= tableOffsetTop && offset <= tableOffsetBottom && !header.hasClass("fixed")) {
                header.addClass("fixed-header");
                content.css("margin-top", cleanMarginTop + "px");
            }
        }

        resizeFixed();

        $(window).resize(resizeFixed);
        $(window).scroll(scrollFixed);
    }

    $(document).ajaxError(function (event, xhr, settings) {
        var errorJson = xhr.responseJSON || {};

        if (errorJson.hasOwnProperty("Exception") && errorJson.Exception === true) {
            app.error(errorJson.Message, errorJson.Title);
        }
        else if (errorJson.hasOwnProperty("Object") && errorJson.Object instanceof Array) {
            app.error(errorJson.Object[0], "");
        }

        app.unblockUI();
    });

    $(function () {
        let layout = new Layout();

        layout.init();

        $('a[data-action="collapse"]').on('click', function (e) {
            let $card = $(this).closest('.card');
            $card.children('.card-content, .card-body').toggleClass('hidden');
            $card.find('[data-action="collapse"] i').toggleClass('ft-minus ft-plus');
            e.preventDefault();
        });

        var message = window.sessionStorage.getItem("flashMessage");
        if (message != null) {
            app.success(message);
            window.sessionStorage.removeItem("flashMessage");
        }
    });
})(jQuery, window, app, swal);

function css(type, status) {
    let dicts = {
        DocumentApproval: {
            inprogress: "bg-warning",
            completed: "bg-success",
            rejected: "bg-danger",
            expired: "bg-dark",
            cancelled: "bg-danger",
            revised: "bg-info",
            autocancel: "bg-danger"
        },
        ApprovalAction: {
            initiate: "bg-blue-sharp",
            approve: "bg-green-haze",
            cancel: "bg-red-haze",
            reject: "bg-red-haze",
            revise: "bg-yellow-crusta",
            acknowledge: "bg-purple"
        }
    };

    return dicts[type][status];
}