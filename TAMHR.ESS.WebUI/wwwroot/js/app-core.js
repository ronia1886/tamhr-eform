/*!
  * App Core v1.0.0
  * Copyright 2018 by Hensem Brian
  */
(function ($, window, signalR, undefined) {
    let app = window.app = window.app || { version: "1.0.0" },
        baseUrl = "",
        uploadUrl = "",
        localData = {},
        handlers = {},
        extend = $.extend,
        each = $.each,
        isArray = $.isArray,
        proxy = $.proxy,
        noop = $.noop,
        hubInstance = {},
        objectCounter = {},
        localizer = {},
        confirmHandler = function (title, message, callback) {
            if (confirm(title + "\n" + message)) {
                callback.apply(this);
            }
        },
        alertHandler = function (type, message) {
            alert(type + ": " + message);
        },
        alertTypes = {
            error: "error",
            warning: "warning",
            success: "success"
        },
        methods = {
            post: "POST",
            get: "GET",
            put: "PUT",
            delete: "DELETE"
        },
        dataTypes = {
            json: "json",
            xml: "xml",
            html: "html",
            text: "text"
        },
        kendoNames = {
            grid: "kendoGrid",
            listview: "kendoListView"
        },
        defaultContentType = "application/json; charset=utf-8",
        iOS = !!navigator.platform && /iPad|iPhone|iPod/.test(navigator.platform);

    function setupHub(url) {
        if (isNullOrEmpty(url)) {
            throw `Url cannot be empty`;
        }

        hubInstance = new signalR.HubConnectionBuilder()
            .withUrl(app.resolveUrl(url))
            .configureLogging(signalR.LogLevel.Information)
            .build();

        return hubInstance;
    }

    function getHub(url = "") {
        if (!isEmptyOrUndefined(url)) return setupHub(url);

        return !isEmptyOrUndefined(hubInstance) ? hubInstance : null;
    }

    function getAttributes($el, prefix) {
        let attributes = {},
            checkPrefix = prefix !== undefined;

        if ($el.length) {
            each($el[0].attributes, function (index, attr) {
                if (!checkPrefix || (checkPrefix && attr.name.startsWith(prefix))) {
                    attributes[attr.name.substring(checkPrefix ? prefix.length : 0)] = attr.value;
                }
            });
        }

        return !$.isEmptyObject(attributes)
            ? attributes
            : undefined;
    }

    function isEmptyOrUndefined(obj) {
        return obj === undefined || $.isEmptyObject(obj);
    }

    function isNullOrEmpty(str) {
        return str === null || str === "";
    }

    function toDate(dateStr) {
        if (dateStr != null && dateStr != "") {
            const [day, month, year] = dateStr.split("/");

            let hour = 0,
                minute = 0;

            const [y, time, t] = year.split(" ");

            if (time) {
                const [h, m] = time.split(":");

                hour = parseInt(h);
                hour = hour == 12 && t == "AM" ? 0 : (hour < 12 && t == "PM" ? hour + 12 : hour);

                minute = parseInt(m);
            }

            return new Date(year.substring(0, 4), month - 1, day, hour, minute, 0);
        } else {
            return new Date();
        }
    }

    function isDate(str) {
        if (isEmptyOrUndefined(str) || isNullOrEmpty(str) || /^[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}$/.test(str)) {
            return false;
        }

        var parms = str.toString().split(/[\.\-\/]/);
        var yyyy = parseInt((parms[2] || "").substring(0, 4), 10);
        var mm = parseInt(parms[1], 10);
        var dd = parseInt(parms[0], 10);
        var date = new Date(yyyy, mm - 1, dd, 0, 0, 0, 0);

        return mm === (date.getMonth() + 1) && dd === date.getDate() && yyyy === date.getFullYear();
    }

    function parseDate(obj) {
        for (var key in obj) {
            if (typeof obj[key] === "object") {
                parseDate(obj[key]);
            } else {
                if (isDate(obj[key])) {
                    obj[key] = kendo.toString(toDate(obj[key]),"yyyy-MM-dd HH:mm tt");
                }
            }
        }
    }

    function setupAjax(method, url, data, callback, dataType, contentType) {
        parseDate(data);

        return $.ajax({
            type: method,
            contentType: contentType,
            url: app.resolveUrl(url),
            dataType: dataType,
            data: isEmptyOrUndefined(dataType) ? data : JSON.stringify(data),
            success: function (response) {
                if (typeof callback === 'function') {
                    callback(response);
                }
            }
        });
    }

    function isFunction(func) {
        return typeof func === 'function';
    }

    function isString(obj) {
        return typeof obj === 'string';
    }

    function getHandler(handlerName) {
        if (handlers.hasOwnProperty(handlerName)) {
            return handlers[handlerName];
        }

        return undefined;
    }

    function showAlert(type, message, title) {
        let defaultTitle = title || "";

        alertHandler(type, defaultTitle, message);
    }

    function generateCounter(type) {
        if (!objectCounter.hasOwnProperty(type)) {
            objectCounter[type] = 0;
        }

        return ++objectCounter[type];
    }

    function getJqueryObject(obj) {
        if (obj === undefined) return undefined;

        return (isString(obj))
            ? $('#' + obj)
            : (obj.jquery ? obj : $(obj));
    }

    function isJqueryObject(obj) {
        return obj !== undefined && getJqueryObject(obj).length > 0;
    }

    function unescapeHtml(safe) {
        return safe.replace(/&amp;/g, '&')
            .replace(/&lt;/g, '<')
            .replace(/&gt;/g, '>')
            .replace(/&quot;/g, '"')
            .replace(/&#039;/g, "'");
    }

    function generateModal(element) {
        let $this = getJqueryObject(element),
            modalId = $this.data("modal"),
            iconClass = $this.data("modalIcon"),
            title = $this.data("modalTitle") || $this.data("title"),
            htmlTitle = $this.data("htmlTitle") || false,
            modalType = $this.data("modalType") || "lg",
            loadByAjax = $this.data("modalAjax") || false,
            ajaxUrl = app.resolveUrl($this.data("ajaxUrl") || ""),
            callbackName = $this.data("modalCallback") || "",
            modalExist = isJqueryObject(modalId);

        if (isEmptyOrUndefined(modalId)) {
            modalId = "app-modal-instance-" + generateCounter("modal");
            $this.data("modal", modalId);
        }

        if (!modalExist) {
            let template = `
                    <div id="${modalId}" class="modal fade bs-modal-${modalType}"  role="dialog" aria-hidden="true" style="display: none;">
                        <div class="modal-dialog modal-${modalType}" role="document">
                            <div class="modal-content">
                                <div class="modal-header">
                                    <h5 class="modal-title">${title}</h5>
                                    <button type="button" class="close" data-dismiss="modal" aria-label="Close">
                                        <span aria-hidden="true">&times;</span>
                                    </button>
                                </div>
                                <div class="modal-body">
                                    <div class="loader"></div>
                                </div>
                                <div class="modal-footer">
                                    <button type="button" class="btn dark btn-outline btn-modal-close" data-dismiss="modal">${app.translate("Close")}</button>
                                </div>
                            </div>
                        </div>
                    </div>`;

            $("body").append(template);
        }

        let $modal = getJqueryObject(modalId),
            callback = getHandler(callbackName),
            $modalTitle = $modal.find('.modal-title');

        if (isEmptyOrUndefined(iconClass)) {
            if (htmlTitle) {
                $modalTitle.html(title);
            }
            else {
                $modalTitle.text(title);
            }
        } else {
            $modalTitle.html(`<i class="${iconClass}"></i> ` + title);
        }

        if (loadByAjax && !isNullOrEmpty(ajaxUrl)) {
            let $loader = $modal.find(".loader") || $modal.find(".modal-body"),
                ajaxParameters = $this.data("ajaxParameters") || $this.data("parameters") || getAttributes($this, "data-parameters-"),
                ajaxHandlerName = $this.data("ajaxHandler") || "",
                parameters = ajaxParameters;

            $loader.html("");

            app.blockUI({ message: app.translate("Loading content..."), boxed: true, showOverlay: false, target: $loader });

            if (isEmptyOrUndefined(parameters)) {
                let ajaxHandler = getHandler(ajaxHandlerName);

                parameters = isFunction(ajaxHandler) ? ajaxHandler.apply($this[0]) : {};
            }

            $loader.load(ajaxUrl, parameters, function (response, status, xhr) {
                if (status === "error") return;

                let $modalFooter = $modal.find(".modal-footer");
                $modalFooter.find(".actions").remove();
                let actions = $modal.find(".modal-body .actions").detach();
                actions.data("sender", $this);

                if (actions.length > 0) {
                    $modalFooter.prepend(actions);
                }

                if (callback !== undefined) {
                    callback.apply($modal);
                }

                app.unblockUI($modal);
            });
        } else {
            if (callback !== undefined) {
                $modal.on("show.bs.modal", proxy(callback, $modal));
            }
        }

        $modal.modal("show");
    }

    function invokeHandler(element) {
        let $this = getJqueryObject(element),
            handlerName = $this.data("handler") || "",
            parameters = $this.data("parameters") || getAttributes($this, "data-parameters-") || {},
            handler = getHandler(handlerName);

        if (isFunction(handler)) {
            return handler.apply($this[0], [parameters]);
        }

        return undefined;
    }

    function getFormSubmitValue($element) {
        let $form = getJqueryObject($element.data("targetForm")) || $element.closest("form"),
            name = $element.attr('name'),
            value = $element.val(),
            el = $form.find("input[name='" + name + "']");

        if (el === undefined || el === null || el.length === 0) {
            $form.append("<input type='hidden' name='" + name + "' value ='" + value + "' />");
        } else {
            $form.find("input[name='" + name + "']").val(value);
        }

        return $form;
    }

    app.submitForm = function (form, callback) {
        var $form = getJqueryObject(form),
            url = $form.attr("action"),
            method = ($form.attr("method") || methods.post).toLowerCase(),
            data = $form.serializeObject(),
            handler = $[method];

        if (!isFunction(handler)) return;

        handler(url, data, function (response) {
            callback.apply($form[0], [response]);
        })
        .fail(function (jqXHR, textStatus, errorThrown) {
            var errors = jqXHR.responseJSON || {};

            if (!showError($form, errors)) {
                $form.validate(errors);
            }
        });
    };

    function submit(element) {
        let $el = getJqueryObject(element),
            $modal = $el.closest(".modal"),
            $form = $modal.length > 0 ? $modal.find("form") : getFormSubmitValue($el),
            method = $el.data("method") || ( ($form.attr("method") || methods.post).toLowerCase() ),
            data = $form.serializeObject(),
            url = $el.data("url") || $form.attr("action"),
            interceptorName = $el.data("interceptor") || "",
            preCallbackName = $el.data("precallback") || "",
            errorHandlerName = $el.data("errorHandler") || "",
            callbackName = $el.data("callback") || "",
            interceptor = getHandler(interceptorName),
            preCallback = getHandler(preCallbackName),
            errorHandler = getHandler(errorHandlerName),
            confirmation = $el.data("confirmation"),
            isDraft = $el.data("draft") || false,
            confirmationmsg = $el.data("confirmationmsg");

        if ($el.data("submitted") === true) {
            return;
        } else {
            $el.data("submitted", true);
        }

        if (data.Object !== undefined && data.Object !== null)
            data.Object.IsDraft = isDraft;

        //cek progress kendo upload
        var onProgressFile = 0;
        var errorFile = 0;
        if ($(".k-file") != undefined || $(".k-file") != null) {
            $(".k-file").find(".k-progress").each(function (index) {
                var $this = $(this),
                    $file = $this.closest(".k-file"),
                    $downloadBtn = $file.find(".ajax-file-upload-green");

                if (!$downloadBtn.is(":visible") && $downloadBtn.closest(".k-upload").is(":visible")) {
                    onProgressFile += 1;
                }
            });
        }
        if ($(".k-file").find(".ajax-file-upload-error").length > 0) {
            errorFile += 1;
        }

        if (onProgressFile > 0) {
            if (errorFile > 0) { app.warning('Uploading file error.'); }
            else { app.warning('Uploading file still on progress.'); }
        } else {
            if (confirmation === true) {
                app.confirm(confirmationmsg, function (d) {
                    if (!d.value) {
                        $el.data("submitted", false);
                        $el.data("confirm", false);

                        return;
                    }

                    if ($el.data("confirm") === true) {
                        return;
                    } else {
                        $el.data("confirm", true);
                    }
                    doSubmit($el, $modal, $form, method, data, url, callbackName, preCallback, element, interceptor, errorHandler);
                });
            } else {
                doSubmit($el, $modal, $form, method, data, url, callbackName, preCallback, element, interceptor, errorHandler);
            }
        }
    }

    function doSubmit($el, $modal, $form, method, data, url, callbackName, preCallback, element, interceptor, errorHandler) {
        var parameters = $el.data("parameters") || getAttributes($el, "data-parameters-") || {};
        $el.prop("disabled", true);

        if (isFunction(interceptor)) {
            data = interceptor.apply(element, [data]);
        }

        if (!isFunction(preCallback) || preCallback.apply(element, [parameters]) === true) {
            var handler = $[method];

            if (!isFunction(handler)) return;

            handler(url, data, function (response) {
                
                let callback = getHandler(callbackName);

                if (callback !== undefined) {
                    callback.apply(element, [response, parameters]);
                } else {
                    app.warning("Warning", "Callback must be defined");
                }
            })
                .fail(function (jqXHR, textStatus, errorThrown) {                
                    var errors = jqXHR.responseJSON.errors || {};

                    if (jqXHR.responseText.includes("Request Rejected")) {
                        app.error(jqXHR.responseText);
                    }
                    

                    if (!isFunction(errorHandler)) {
                        if (!showError($form, errors)) {
                        $form.validate(errors);
                    }
                }
                    else {
                        //alert("else");
                    errorHandler.apply(element, [$form, errors]);
                }
            })
            .always(function () {
                $el.prop("disabled", false);
                $el.data("confirm", false);
                $el.data("submitted", false);
            });
        } else {
            $el.prop("disabled", false);
            $el.data("confirm", false);
            $el.data("submitted", false);
        }
    }

    function merge(source, defaultOptions) {
        return !isEmptyOrUndefined(source) ? $.extend({}, source, defaultOptions || {}) : source;
    }

    function makeObject(input, fieldValue) {
        if (typeof (input) === 'undefined') return {};
        if ($.isPlainObject(input)) return input;

        let obj = {};

        obj[fieldValue] = input;

        return obj;
    }

    function filter(element) {
        let $el = getJqueryObject(element),
            $filterWrapper = $el.closest("[data-role='filter']"),
            $li = $el.closest("li"),
            $ul = $el.closest("ul"),
            clear = $el.data("clear") || "",
            sort = merge(makeObject($el.data("sort"), "field"), { dir: 'asc' }),
            filter = merge({ operator: 'eq' }, makeObject($el.data("filter"), "field")),
            query = $filterWrapper.data("query") || {};

        query.sort = !isEmptyOrUndefined(sort) ? sort : (clear == 'sort' ? {} : query.sort);
        query.filter = !isEmptyOrUndefined(filter) ? filter : (clear == 'filter' ? {} : query.filter);

        $ul.find("li").removeClass("active");

        if (isNullOrEmpty(clear)) {
            $li.removeClass("active").addClass("active");
        }

        let $els = $("[data-filter='" + $filterWrapper.attr("id") + "']");

        each($els, (key, el) => {
            let $el = $(el),
                ds = $el.data(kendoNames[$el.data("role")]).dataSource;

            if (!isEmptyOrUndefined(query.sort) && !isEmptyOrUndefined(ds._sort) && ds._sort.length > 0 && ds._sort[0].field == query.sort.field) {
                query.sort.dir = ds._sort[0].dir == 'asc' ? 'desc' : 'asc';
            }

            let executeQuery = $.extend({}, query, { page: ds.options.page, pageSize: ds.options.pageSize });

            ds.query(executeQuery);
        });

        $ul.find("i.float-right").remove();

        if (!isEmptyOrUndefined(sort)) {
            if (query.sort.dir == 'asc') {
                $el.append("<i class='ft-arrow-up float-right font-red-sunglo' style='font-size: 0.76rem; margin-top: 0.25rem;'></i>");
            } else {
                $el.append("<i class='ft-arrow-down float-right font-red-sunglo' style='font-size: 0.76rem; margin-top: 0.25rem;'></i>");
            }
        }

        $filterWrapper.data("query", query);
    }

    function showError($wrapper, errors) {
        if (!errors.hasOwnProperty("Exception")) return false;

        $wrapper.find("[role='alert']").removeClass("hidden").show();
        $wrapper.find(".alert-title").text(errors.Title);
        $wrapper.find(".alert-message").text(errors.Message);

        return true;
    }

    function triggerEvent(triggerName, element) {
        switch (triggerName) {
            case "modal": generateModal(element);
                break;
            case "handler": invokeHandler(element);
                break;
            case "submit": submit(element);
                break;
            case "filter": filter(element);
                break;
        }
    }

    app.getHub = getHub;

    app.localize = function (obj) {
        localizer = $.extend({}, localizer, obj);
    };

    app.translate = function (key) {
        return localizer.hasOwnProperty(key) ? localizer[key] : key;
    };

    app.fillData = function (el, data) {
        var $el = getJqueryObject(el),
            $fieldEls = $el.find("[data-field]");

        each($fieldEls, function (key, val) {
            var $val = $(val),
                field = $val.data("field");

            if (typeof data[field] !== 'undefined') {
                $val.text(data[field]);
            }
        });
    };

    app.toHierarchy = function (data, idField, parentField, childrenField) {
        var roots = [];
        var all = {};

        data.forEach(function (item) {
            all[item[idField]] = item;
        });

        Object.keys(all).forEach(function (idField) {
            var item = all[idField];

            if (item[parentField] === null) {
                roots.push(item);
            } else if (item[parentField] in all) {
                var p = all[item[parentField]];

                if (!(childrenField in p)) {
                    p[childrenField] = [];
                }

                p[childrenField].push(item);
            }
        });

        return roots;
    };

    app.refreshControl = function (el) {
        if (!el) return;

        var $el = getJqueryObject(el);

        if ($el.length == 0) return;

        var role = $el.data("role"),
            dataSource = $el.data(kendoNames[role]).dataSource;

        if (dataSource) {
            dataSource.read();
        }
    };

    app.formatValue = function formatValue(itemText, text) {
        var textMatcher = new RegExp(text, "ig");

        return itemText.replace(textMatcher, function (match) {
            return "<strong>" + match + "</strong>";
        });
    };

    app.set = function (key, value) {
        localData[key] = value;
    };

    app.get = function (key) {
        if (localData.hasOwnProperty(key)) return localData[key];

        return null;
    };

    app.resolveImageUrl = function (key) {
        var imageHandlerUrl = app.get("imageHandlerUrl") || "";

        return kendo.format(imageHandlerUrl, key);
    };

    app.resolveUrl = function (url) {
        return url.replace("~", baseUrl);
    };

    app.init = function (properties) {
        var data = properties.data || {};
        app.set("imageHandlerUrl", properties.imageHandlerUrl || '');
        baseUrl = properties.baseUrl;
        uploadUrl = properties.uploadUrl;
        localizer = properties.localizer;

        if (!isNullOrEmpty(properties.hubUrl || '')) {
            setupHub(properties.hubUrl);
        }

        if (!isEmptyOrUndefined(data)) {
            for (var key in data) {
                app.set(key, data[key]);
            }
        }

        app.init = noop;
    };

    app.invokeHandler = function (handlerName, args) {
        if (!app.hasHandler(handlerName)) {
            throw `Handler with name '${handlerName}' is not registered`;
        }

        var handler = app.getHandler(handlerName);
        return handler(args);
    };

    app.interceptHandler = function (handlerName, conditional, interceptorHandler) {
        var handler = app.getHandler(handlerName);

        app.registerHandler(handlerName, function (parameters) {
            var context = this;

            if (conditional(parameters)) {
                interceptorHandler.apply(context, [parameters, function () { handler.apply(context, [parameters]); }]);
            } else {
                handler.apply(context, [parameters]);
            }
        });
    };

    app.hasHandler = function (handlerName) {
        return handlers.hasOwnProperty(handlerName);
    };

    app.getHandler = function (handlerName) {
        if (!app.hasHandler(handlerName)) {
            throw `Handler with name '${handlerName}' is not found`;
        }

        return handlers[handlerName];
    };

    app.registerHandler = function (handlerName, handlerDelegate) {
        if (!isFunction(handlerDelegate)) {
            throw `'${handlerName}' is not a function`;
        }

        handlers[handlerName] = handlerDelegate;
    };

    app.setAlertHandler = function (handler) {
        if (isFunction(handler)) {
            alertHandler = handler;
        }
    };

    app.setConfirmHandler = function (handler) {
        if (isFunction(handler)) {
            confirmHandler = handler;
        }
    };

    app.confirm = function (title, message, handler) {
        if (isFunction(message)) {
            handler = message;
            message = "";
        }

        confirmHandler(title, message, handler);
    };

    app.warning = function (message, title) {
        showAlert(alertTypes.warning, message, title);
    };

    app.success = function (message, title) {
        showAlert(alertTypes.success, message, title);
    };

    app.error = function (message, title) {
        showAlert(alertTypes.error, message, title);
    };

    app.close = function (element) {
        let $this = getJqueryObject(element),
            $modal = $this.hasClass("modal")
                ? $this
                : getJqueryObject($this.data("modal")) || $this.closest(".modal");

        if ($modal.length > 0) {
            $modal.modal("hide");
        }
    };

    app.flash = function (message, key = "flashMessage") {
        window.sessionStorage.setItem(key, message);
    };

    function filterInput(el) {
        let $el = getJqueryObject(el),
            logicalOperator = $el.data("operator") || 'or',
            filters = $el.data("filters"),
            $filterWrapper = $el.closest("[data-role='filter']"),
            currentFilters = [],
            $els = $("[data-filter='" + $filterWrapper.attr("id") + "']");

        each(filters, function (idx, config) {
            let field = typeof config === 'string' ? config : config.field,
                operator = config.operator || 'contains',
                fieldValue = $el.val();

            if (fieldValue != '' && fieldValue != null) {
                currentFilters.push({
                    field: field,
                    operator: operator,
                    value: fieldValue
                });
            }
        });

        each($els, function (key, el) {
            let $el = $(el),
                ds = $el.data(kendoNames[$el.data("role")]).dataSource;

            ds.filter({
                logic: logicalOperator,
                filters: currentFilters
            });
        });
    }

    app.initJqueryEvents = function () {
        $("body").on("click", "[data-trigger='modal'],[data-trigger='handler'],[data-trigger='submit'],a[data-trigger='filter']", function (event) {
            let $this = $(this),
                triggerName = $this.data("trigger");

            triggerEvent(triggerName, $this);

            if (event.preventDefault && !$this.is(':checkbox')) {
                //event.stopImmediatePropagation();
                event.preventDefault();
            }
        });

        $("body").on("keyup", "input[data-trigger='filter']", function (event) {
            if (event.keyCode === 13) {
                let $this = $(this),
                    triggerName = $this.data("trigger");

                filterInput($this);

                if (event.preventDefault && !$this.is(':checkbox')) {
                    //event.stopImmediatePropagation();
                    event.preventDefault();
                }
            }
        });

        app.initJqueryEvents = noop;
    };

    function doupload($this) {
        let form = $this.closest("form"),
            file = $this.files[0],
            fileName = file.name,
            category = $this.name,
            $inputElement = $("input[type='text'][name='" + $this.name + "']"),
            $formGroup = $($this).closest(".form-group");

        var formData = new FormData();
        formData.append(file.name, file);

        $formGroup.find(".progress").remove();
        var $progressElement = $formGroup.append('<div class="progress" style="height: 4px;"><div class="progress-bar progress-bar-striped bg-warning" role="progressbar" aria-valuenow="0" aria-valuemin="0" aria-valuemax="0" style="width:0%;"></div></div>');
        var $progressBarElement = $progressElement.find(".progress-bar");

        var jqxhr = $.ajax({
            url: uploadUrl,
            type: "POST",
            contentType: false,
            processData: false,
            data: formData,
            cache: false,
            async: true,
            xhr: function () {
                var xhr = new window.XMLHttpRequest();
                xhr.upload.addEventListener("progress",
                    function (evt) {
                        if (evt.lengthComputable) {
                            var progress = Math.round((evt.loaded / evt.total) * 100);
                            $progressBarElement.css("width", progress + "%");
                        }
                    },
                    false);
                return xhr;
            }
        })
        .done(function (data, textStatus, jqXhr) {
            if (data.files !== undefined || data.files !== null) {
                $.each(data.files, function (i, o) {
                    if (o.Id !== null) {
                        $('.common-file-' + category).remove();  
                        $(form).append("<div class='common-file-" + category + "' data-id='" + o.Id + "' data-filename='" + o.FileName +"'>");
                        $(form).append("<input type='hidden' name='Attachments[][CommonFileId]' value ='" + o.Id + "'>");
                        $(form).append("<input type='hidden' name='Attachments[][FieldCategory]' value ='" + category + "'>");
                        $(form).append("<input type='hidden' name='Attachments[][FileName]' value ='" + o.FileName + "'>");
                        $(form).append("</div>");

                        $inputElement.val(fileName);
                    } else {
                        showAlert(alertTypes.error, "Failed to upload attachment, please try again.", "Upload Failed");
                    }
                });
            }
        })
        .fail(function (jqXhr, textStatus, errorThrown) {
            if (errorThrown === "abort") {
                app.error("Uploading was aborted");
            } else {
                app.error("Uploading failed");
            }
        })
        .always(function (data, textStatus, jqXhr) { });
    }

    $.fn.extend({
        upload: function () {
            var $this = $(this)[0];
            
            doupload($this);
        }
    });

    app.parseDate = function (input) {
        return new Date(input.replace(/-/g, '/').replace('T', ' '));
    };

    if (typeof (Number.prototype.isBetween) === "undefined") {
        Number.prototype.isBetween = function (min, max, notBoundaries) {
            var between = false;
            if (notBoundaries) {
                if ((this < max) && (this > min)) between = true;
            } else {
                if ((this <= max) && (this >= min)) between = true;
            }
            return between;
        };
    }

    app.clearValidation = function (el) {
        var $form = getJqueryObject(el);

        $form.find("span.invalid[data-validation-for]").text("");
        $form.find(".is-invalid").removeClass("is-invalid");
    };

    function validateForm($form, errors) {
        var el = $form[0].nodeName.toLowerCase();

        if (el === "form") {
            var $firstInvalidElement = null;
            $form.find("span.invalid[data-validation-for]").text("");

            each($form.find("[data-validation-for]"), function (index, validationElement) {
                var $validationElement = $(validationElement),
                    validationProperty = $validationElement.data("validationFor"),
                    $input = $form.find("[name='" + validationProperty + "']"),
                    $formControl = $input.parent().closest(".form-control"),
                    isInputElement = $.inArray($validationElement[0].nodeName.toLowerCase(), ["input", "select", "textarea"]) !== -1;

                $formControl.removeClass("is-invalid");
                $input.removeClass("is-invalid");
                 
                if (!errors.hasOwnProperty(validationProperty)) return;

                if ($firstInvalidElement == null) {
                    $firstInvalidElement = $input;
                }

                var error = errors[validationProperty];

                if (!$formControl.hasClass("is-invalid")) {
                    $formControl.addClass("is-invalid");
                }

                if (!$input.hasClass("is-invalid")) {
                    $input.addClass("is-invalid");
                }

                if (isInputElement && !$validationElement.hasClass("is-invalid")) {
                    $validationElement.addClass("is-invalid");
                }

                if ($validationElement[0] !== $input[0] && !isInputElement) {
                    $validationElement.text(error[0]);
                }
            });

            each($form.find(".validation-summaryinput[type=radio][id=Object_TypeShift]"), function (index, validationSummaryElement) {
                var $validationSummaryElement = $(validationSummaryElement);

                $validationSummaryElement.removeClass("hidden").show();
            });

            each($form.find("span[class*='validation-summary']"), function (index, validationSummaryElement) {
                var $validationSummaryElement = $(validationSummaryElement);
                var name = $validationSummaryElement.attr("name");
                var error = errors[name];
                $validationSummaryElement.text(error);
                $validationSummaryElement.attr("style", "color:rgb(255, 0, 0);");
                $validationSummaryElement.removeClass("hidden").show();
            });

            if ($firstInvalidElement != null) {
                var inModal = $form.closest(".modal").length > 0,
                    $wrapper = inModal ? $form.closest(".modal") : $('html,body'),
                    $formGroup = $firstInvalidElement.closest(".form-group"),
                    offset = 0;

                if ((inModal && $formGroup.position() !== undefined) || $formGroup.offset() !== undefined)
                    offset = inModal ? $formGroup.position().top + 100 : $formGroup.offset().top - 70;

                if (offset > 0) {
                    $wrapper.animate({ scrollTop: offset }, 400, function () { });
                }
            }
        } else {
            app.error("Validation only work on form element");
        }
    }

    app.formatNumber = function (num) {
        num = parseFloat(num);
        return num.toString()
            .replace(/\B(?=(\d{3})+(?!\d))/g, ",");
    };

    app.blockUI = function (options) {
        let html = '';

        options = $.extend(true, {}, options);

        if (options.animate) {
            html = '<div class="loading-message ' + (options.boxed ? 'loading-message-boxed' : '') + '">' + '<div class="block-spinner-bar"><div class="bounce1"></div><div class="bounce2"></div><div class="bounce3"></div></div>' + '</div>';
        } else if (options.iconOnly) {
            html = '<div class="loading-message ' + (options.boxed ? 'loading-message-boxed' : '') + '"><img src="' + app.resolveUrl("~/img/loading-spinner-grey.gif") + '" align=""></div>';
        } else if (options.textOnly) {
            html = '<div class="loading-message ' + (options.boxed ? 'loading-message-boxed' : '') + '"><span>&nbsp;&nbsp;' + (options.message ? options.message : 'LOADING...') + '</span></div>';
        } else {
            html = '<div class="loading-message ' + (options.boxed ? 'loading-message-boxed' : '') + '"><img src="' + app.resolveUrl("~/img/loading-spinner-grey.gif") + '" align=""><span>&nbsp;&nbsp;' + (options.message ? options.message : 'LOADING...') + '</span></div>';
        }

        if (options.target) {
            var $el = getJqueryObject(options.target);

            $el.block({
                message: html,
                baseZ: options.zIndex ? options.zIndex : 1000,
                css: {
                    border: '0',
                    padding: '0',
                    backgroundColor: 'none'
                },
                overlayCSS: {
                    backgroundColor: options.overlayColor ? options.overlayColor : '#555',
                    opacity: options.boxed ? 0.05 : 0.1,
                    cursor: 'wait'
                }
            });
        } else {
            $.blockUI({
                message: html,
                baseZ: options.zIndex ? options.zIndex : 1000,
                css: {
                    border: '0',
                    padding: '0',
                    backgroundColor: 'none'
                },
                overlayCSS: {
                    backgroundColor: options.overlayColor ? options.overlayColor : '#555',
                    opacity: options.boxed ? 0.05 : 0.1,
                    cursor: 'wait'
                }
            });
        }
    };

    app.unblockUI = function (target) {
        if (target) {
            var $target = getJqueryObject(target);
            $target.unblock({
                onUnblock: function () {
                    $target.css('position', '');
                    $target.css('zoom', '');
                }
            });
        } else {
            $.unblockUI();
        }
    };

    $.fn.validate = function (errors) {
        validateForm(this, errors);
    };
    
    extend(FormSerializer.patterns, {
        validate: /^[a-z][a-z0-9_-]*(?:\.[a-z0-9_]+)*(?:\[\])*(?:\[(?:\d*|[a-z0-9_-]+)\])*$/i,
        key: /[a-z0-9_-]+|(?=\[\])/gi,
        named: /^[a-z0-9_-]+$/i
    });

    $.toDefault = function (data, defaultValue) {
        return typeof data === 'undefined' || data === null || data === '' ? defaultValue : data;
    };

    $.post = function (url, data, callback) {
        return setupAjax(methods.post, url, data, callback, dataTypes.json, defaultContentType);
    };

    $.put = function (url, data, callback) {
        return setupAjax(methods.put, url, data, callback, dataTypes.json, defaultContentType);
    };

    $.delete = function (url, data, callback) {
        return setupAjax(methods.delete, url, data, callback);
    };

    $.printWords = function (input) {
        var words = ["", "satu", "dua", "tiga", "empat", "lima", "enam", "tujuh", "delapan", "sembilan", "sepuluh", "sebelas"],
            floorInput = Math.floor(input);

        if (floorInput < 12)
            return " " + words[floorInput];
        else if (floorInput < 20)
            return $.printWords(floorInput - 10) + " belas";
        else if (floorInput < 100)
            return $.printWords(floorInput / 10) + " puluh" + $.printWords(floorInput % 10);
        else if (floorInput < 200)
            return " seratus" + $.printWords(floorInput - 100);
        else if (floorInput < 1000)
            return $.printWords(floorInput / 100) + " ratus" + $.printWords(floorInput % 100);
        else if (floorInput < 2000)
            return " seribu" + $.printWords(floorInput - 1000);
        else if (floorInput < 1000000)
            return $.printWords(floorInput / 1000) + " ribu" + $.printWords(floorInput % 1000);
        else if (floorInput < 1000000000)
            return $.printWords(floorInput / 1000000) + " juta" + $.printWords(floorInput % 1000000);
        else if (floorInput < 1000000000000)
            return $.printWords(floorInput / 1000000000) + " milyar" + $.printWords(floorInput % 1000000000);
    };

    if (window.applicationConfiguration !== undefined) {
        app.init(window.applicationConfiguration);
    } else {
        throw 'Config must be define';
    }

    app.unique = function (list) {
        var result = [];
        $.each(list, function (i, e) {
            if ($.inArray(e, result) == -1) result.push(e);
        });
        return result;
    };
})(jQuery, window, signalR);

$(function () {
    app.initJqueryEvents();
});