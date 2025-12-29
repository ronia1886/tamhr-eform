(function ($) {
    if ($.fn.ajaxForm == undefined) {
        $.getScript(app.resolveUrl("~/plugins/jquery.form.js"));
    }
 
    var feature = {};
    feature.fileapi = $("<input type='file'/>").get(0).files !== undefined;
    feature.formdata = window.FormData !== undefined;
    $.fn.uploadFile = function (options) {

        $(this).data('uploadFile', this);

        var fileName = $(this).data('name');
        if (fileName === undefined || fileName === null) {
            fileName = "FileName";
        }

        var docId = $(this).data('doc-id');
        if (docId === undefined || docId === null) {
            docId = "00000000-0000-0000-0000-000000000000";
        }

        var parentForm = $(this).data('parentFormId');
        if (parentForm === undefined || parentForm === null) {
            parentForm = "mainForm";
        }


        var multiple = $(this).data('multiple');
        if (multiple === undefined || multiple === null) {
            multiple = false;
        }

        var isMobile = $(this).data('isMobile');
        if (isMobile === undefined || isMobile === null) {
            isMobile = false;
        }

        var allowedTypes = $(this).data('types');
        if (allowedTypes === undefined || allowedTypes === null || allowedTypes === "") {
            allowedTypes = "jpg,jpeg,gif,png,xls,xlsx,doc,docx,pdf,txt,csv,zip,rar";
        }

        // This is the easiest way to have default options.
        var s = $.extend({
            // These are the defaults.
            url: "",
            method: "POST",
            enctype: "multipart/form-data",
            parentForm: parentForm,
            returnType: null,
            allowDuplicates: true,
            duplicateStrict: false,
            allowedTypes: allowedTypes,
            acceptFiles: "*",
            fileName: fileName,
            formData: false,
            dynamicFormData: false,
            maxFileSize: -1,
            maxFileCount: -1,
            multiple: multiple,
            dragDrop: false,
            autoSubmit: true,
            showCancel: true,
            showAbort: true,
            showDone: false,
            showDelete: true,
            showError: true,
            showStatusAfterSuccess: true,
            showStatusAfterError: true,
            showFileCounter: true,
            fileCounterStyle: "). ",
            showFileSize: true,
            showProgress: false,
            nestedForms: true,
            showDownload: true,
            onLoad: defaultOnLoadCallback,
            onSelect: function (files) {
                return true;
            },
            onSubmit: function (files, xhr) { },
            onSuccess: defaultUploadSuccesCallback,
            onError: function (files, status, message, pd) { },
            onCancel: function (files, pd) { },
            onAbort: function (files, pd) { },
            downloadCallback: defaultDownloadCallback,
            deleteCallback: defaultDeleteCallback,
            afterUploadAll: false,
            serialize: true,
            sequential: false,
            sequentialCount: 2,
            customProgressBar: false,
            abortButtonClass: "ajax-file-upload-abort",
            cancelButtonClass: "ajax-file-upload-cancel",
            dragDropContainerClass: "ajax-upload-dragdrop",
            dragDropHoverClass: "state-hover",
            errorClass: "ajax-file-upload-error",
            uploadButtonClass: "ajax-file-upload",
            dragDropStr: "<span><b>Drag &amp; Drop Files</b></span>",
            uploadStr: app.translate("Choose File"),
            abortStr: app.translate("Abort"),
            cancelStr: app.translate("Cancel"),
            deleteStr: '<i class="fa fa-remove"></i>',
            doneStr: app.translate("Done"),
            multiDragErrorStr: "Multiple File Drag &amp; Drop is not allowed.",
            extErrorStr: '<span style="color: red;"> is not allowed.</span> Allowed extensions: ',
            duplicateErrorStr:  '<span style="color: red;"> is not allowed.</span> File already exists.',
            sizeErrorStr:  '<span style="color: red;"> is not allowed.</span> Allowed Max size: ',
            uploadErrorStr: "Upload is not allowed",
            maxFileCountErrorStr: " is not allowed. Maximum allowed files are:",
            downloadStr: '<i class="fa fa-download"></i>',
            customErrorKeyStr: "jquery-upload-file-error",
            showQueueDiv: false,
            statusBarWidth: 400,
            dragdropWidth: 400,
            showPreview: false,
            previewHeight: "auto",
            previewWidth: "100%",
            extraHTML: false,
            uploadQueueOrder: 'top',
            headers: {}
        }, options);

        this.DocId = docId;
        this.fileName = fileName;
        this.objectName = fileName;
        this.parentForm = parentForm;
        this.IsMobile = (isMobile.toLowerCase() === 'true');

        this.fileCounter = 1;
        this.selectedFiles = 0;
        var formGroup = "ajax-file-upload-" + (new Date().getTime());
        this.formGroup = formGroup;
        this.formId = fileName;
        this.errorLog = $("<div></div>"); //Writing errors
        this.responses = [];
        this.existingFileNames = [];
        this.Files = [];
        if (!feature.formdata) //check drag drop enabled.
        {
            s.dragDrop = false;
        }
        if (!feature.formdata || s.maxFileCount === 1) {
            s.multiple = false;
        }

        $(this).html("");

        var obj = this;
        var btnStr = this.IsMobile ? '<i class="ft-camera"></i>' : '<i class="ft-upload"></i>';
        var uploadLabel = $(
            '<div class= "file-group" > ' +
            '    <div class= "input-group-area"> ' +
            '        <input readonly = "readonly" onfocus = "this.blur()" onclick = "$(\'.file\').click();" placeholder = "' + s.uploadStr +'" type = "text" > ' +
            '    </div > ' +
            '    <div class="input-group-icon" > ' +
            '        <div class="file btn">' + btnStr + '<input class="file" type = "file"></div > ' +
            '    </div > ' +
            '</div>'
        ); 

        $(uploadLabel).addClass(s.uploadButtonClass);

        // wait form ajax Form plugin and initialize
        (function checkAjaxFormLoaded() {
            if ($.fn.ajaxForm) {

                if (s.dragDrop) {
                    var dragDrop = $('<div class="' + s.dragDropContainerClass + '" style="vertical-align:top;"></div>').width(s.dragdropWidth);
                    $(obj).append(dragDrop);
                    $(dragDrop).append(uploadLabel);
                    $(dragDrop).append($(s.dragDropStr));
                    setDragDropHandlers(obj, s, dragDrop);

                } else {
                    $(obj).append(uploadLabel);
                }
                $(obj).append(obj.errorLog);

                if (s.showQueueDiv)
                    obj.container = $("#" + s.showQueueDiv);
                else
                    obj.container = $(
                        '<div class="form-group">'+
                        '<div class= "k-content" > ' +
                        '   <div class="k-widget k-upload k-header k-upload-sync">' +
                        '       <ul class="k-upload-files k-reset">' +
                        '       </ul>' +
                        '   </div>' +
                        '</div></div>').insertAfter($(obj)).hide();

                s.onLoad.call(this, obj);
                createCustomInputFile(obj, formGroup, s, uploadLabel);

            } else window.setTimeout(checkAjaxFormLoaded, 10);
        })();


        this.startUpload = function () {
            $("form").each(function (i, items) {
                if ($(this).hasClass(obj.formGroup)) {
                    mainQ.push($(this));
                }
            });

            if (mainQ.length >= 1)
                submitPendingUploads();

        }

        this.getFileCount = function () {
            return obj.selectedFiles;

        }
        this.stopUpload = function () {
            $("." + s.abortButtonClass).each(function (i, items) {
                if ($(this).hasClass(obj.formGroup)) $(this).click();
            });
            $("." + s.cancelButtonClass).each(function (i, items) {
                if ($(this).hasClass(obj.formGroup)) $(this).click();
            });
        }
        this.cancelAll = function () {
            $("." + s.cancelButtonClass).each(function (i, items) {
                if ($(this).hasClass(obj.formGroup)) $(this).click();
            });
        }
        this.update = function (settings) {
            //update new settings
            s = $.extend(s, settings);

            //We need to update action for already created Form.            
            if (settings.hasOwnProperty('url')) {
                $("form").each(function (i, items) {
                    $(this).attr('action', settings['url']);
                });
            }
        }

        this.clearFiles = function () {
            
            obj.fileCounter = 1;
            obj.selectedFiles = 0;
            obj.errorLog.html("");

            $(obj.Files).each(function (i, o) {
                $("#" + o).remove();
            });

            $(obj.ProgressBar).each(function (i, pd) {
                pd.progressDiv.hide();
                pd.statusbar.remove();
            });

            obj.existingFileNames = [];
            obj.Files = [];
            obj.ProgressBar = [];
            obj.responses = [];
            
        }

        this.enqueueFile = function (file) {
            if (!(file instanceof File)) return;
            var files = [file];
            serializeAndUploadFiles(s, obj, files);
        }

        this.reset = function (removeStatusBars) {
            obj.fileCounter = 1;
            obj.selectedFiles = 0;
            obj.errorLog.html("");
            var count = obj.selectedFiles;

            if (count <= 0) {
                $("input[type='hidden'][name='" + obj.objectName + "']").val("");
            } else {
                $("input[type='hidden'][name='" + obj.objectName + "']").val(obj.objectName);
            }
            //remove all the status bars.
            if (removeStatusBars != false) {
                obj.container.html("");
            }
        }
        this.remove = function () {
            obj.container.html("");
            $(obj).remove();

        }
        //This is for showing Old files to user.
        this.createProgress = function (filename, filepath, filesize, FileId = null) {
            var pd = new createProgressDiv(this, s);
            pd.progressDiv.show();
            pd.progressbar.width('100%');

            var fileNameStr = '';

            if (s.showFileCounter)
                fileNameStr = '<span class="k-file-name" title="' + filename + '">' + obj.fileCounter + s.fileCounterStyle + filename + '</span>';
            else fileNameStr = '<span class="k-file-name" title="' + filename +'">' + filename + '</span>';


            if (s.showFileSize)
                fileNameStr += '<span class="k-file-size">' + getSizeStr(filesize) + '</span>';
            

            pd.filename.html(fileNameStr);
            obj.fileCounter++;
            obj.selectedFiles++;

            var count = obj.selectedFiles;

            if (count <= 0) {
                $("input[type='hidden'][name='" + obj.objectName + "']").val("");
            } else {
                $("input[type='hidden'][name='" + obj.objectName + "']").val(obj.objectName);
            }
            if (s.showPreview) {
                pd.preview.attr('src', filepath);
                pd.preview.show();
            }

            if (s.showDownload) {
                pd.download.show();

                pd.download.click(function (e) {

                    var data = {

                        files : [
                            {
                                Id: FileId,
                                FileName: filename
                            }
                        ]
                    }; 
                    
                    if (s.downloadCallback) s.downloadCallback(this, data, pd, e);
                });
            }
            if (s.showDelete) {
                pd.del.show();
                pd.del.click(function () {
                    pd.statusbar.hide().remove();
                    var files = [];
                    files.push({
                        Id : FileId,
                        FileName : filename
                    });

                    var arr = { files: files };

                    if (s.deleteCallback) s.deleteCallback.call(this, arr, pd);
                    obj.selectedFiles -= 1;
                  
                    updateFileCounter(s, obj);
                });
            }

            return pd;
        }

        this.getResponses = function () {
            return this.responses;
        }
        var mainQ = [];
        var progressQ = []
        var running = false;
        function submitPendingUploads() {
            if (running) return;
            running = true;
            (function checkPendingForms() {

                //if not sequential upload all files
                if (!s.sequential) s.sequentialCount = 99999;

                if (mainQ.length == 0 && progressQ.length == 0) {
                    if (s.afterUploadAll) s.afterUploadAll(obj);
                    running = false;
                }
                else {
                    if (progressQ.length < s.sequentialCount) {
                        var frm = mainQ.shift();
                        if (frm != undefined) {
                            progressQ.push(frm);
                            //Remove the class group.
                            frm.removeClass(obj.formGroup);
                            frm.submit();
                        }
                    }
                    window.setTimeout(checkPendingForms, 100);
                }
            })();
        }

        function setDragDropHandlers(obj, s, ddObj) {
            ddObj.on('dragenter', function (e) {
                e.stopPropagation();
                e.preventDefault();
                $(this).addClass(s.dragDropHoverClass);
            });
            ddObj.on('dragover', function (e) {
                e.stopPropagation();
                e.preventDefault();
                var that = $(this);
                if (that.hasClass(s.dragDropContainerClass) && !that.hasClass(s.dragDropHoverClass)) {
                    that.addClass(s.dragDropHoverClass);
                }
            });
            ddObj.on('drop', function (e) {
                e.preventDefault();
                $(this).removeClass(s.dragDropHoverClass);
                obj.errorLog.html("");
                var files = e.originalEvent.dataTransfer.files;
                if (!s.multiple && files.length > 1) {
                    if (s.showError) $("<div class='" + s.errorClass + "'>" + s.multiDragErrorStr + "</div>").appendTo(obj.errorLog);
                    return;
                }
                if (s.onSelect(files) == false) return;
                serializeAndUploadFiles(s, obj, files);
            });
            ddObj.on('dragleave', function (e) {
                $(this).removeClass(s.dragDropHoverClass);
            });

            $(document).on('dragenter', function (e) {
                e.stopPropagation();
                e.preventDefault();
            });
            $(document).on('dragover', function (e) {
                e.stopPropagation();
                e.preventDefault();
                var that = $(this);
                if (!that.hasClass(s.dragDropContainerClass)) {
                    that.removeClass(s.dragDropHoverClass);
                }
            });
            $(document).on('drop', function (e) {
                e.stopPropagation();
                e.preventDefault();
                $(this).removeClass(s.dragDropHoverClass);
            });

        }

        function getFileExtension(filename) {
            return filename.slice((filename.lastIndexOf(".") - 1 >>> 0) + 2);
        }
        
        function getSizeStr(size) {
            var sizeStr = "";
            var sizeKB = size / 1024;
            if (parseInt(sizeKB) > 1024) {
                var sizeMB = sizeKB / 1024;
                sizeStr = sizeMB.toFixed(2) + " MB";
            } else {
                sizeStr = sizeKB.toFixed(2) + " KB";
            }
            return sizeStr;
        }

        function serializeData(extraData) {
            var serialized = [];
            if (jQuery.type(extraData) == "string") {
                serialized = extraData.split('&');
            } else {
                serialized = $.param(extraData).split('&');
            }
            var len = serialized.length;
            var result = [];
            var i, part;
            for (i = 0; i < len; i++) {
                serialized[i] = serialized[i].replace(/\+/g, ' ');
                part = serialized[i].split('=');
                result.push([decodeURIComponent(part[0]), decodeURIComponent(part[1])]);
            }
            return result;
        }
        function noserializeAndUploadFiles(s, obj, files) {
            var ts = $.extend({}, s);
            var fd = new FormData();
            var fileArray = [];
            var fileName = s.fileName.replace("[]", "");
            var fileListStr = "";

            for (var i = 0; i < files.length; i++) {
                if (!isFileTypeAllowed(obj, s, files[i].name)) {
                    if (s.showError) $("<div><font color='red'><b>" + files[i].name + "</b> " + s.extErrorStr + s.allowedTypes + "</font></div>").appendTo(obj.errorLog);
                    continue;
                }
                if (s.maxFileSize != -1 && files[i].size > s.maxFileSize) {
                    if (s.showError) $("<div><font color='red'><b>" + files[i].name + "</b> " + s.sizeErrorStr + getSizeStr(s.maxFileSize) + "</font></div>").appendTo(obj.errorLog);
                    continue;
                }
                fd.append(fileName + "[]", files[i]);
                fileArray.push(files[i].name);
                fileListStr += obj.fileCounter + "). " + files[i].name + "<br>";
                obj.fileCounter++;
            }
            if (fileArray.length == 0) return;

            var extraData = s.formData;
            if (extraData) {
                var sData = serializeData(extraData);
                for (var j = 0; j < sData.length; j++) {
                    if (sData[j]) {
                        fd.append(sData[j][0], sData[j][1]);
                    }
                }
            }


            ts.fileData = fd;
            var pd = new createProgressDiv(obj, s);
            pd.filename.html(fileListStr);
            var form = $("<form style='display:block; position:absolute;left: 150px;' class='" + obj.formGroup + "' method='" + s.method + "' action='" + s.url + "' enctype='" + s.enctype + "'></form>");
            form.appendTo('body');
            ajaxFormSubmit(form, ts, pd, fileArray, obj);

        }


        function serializeAndUploadFiles(s, obj, files) {
            for (var i = 0; i < files.length; i++) {
                if (!isFileTypeAllowed(obj, s, files[i].name)) {
                    if (s.showError) $("<div class='" + s.errorClass + "'><b>" + files[i].name + "</b> " + s.extErrorStr + s.allowedTypes + "</div>").appendTo(obj.errorLog);
                    continue;
                }
                if (!s.allowDuplicates && isFileDuplicate(obj, files[i].name)) {
                    if (s.showError) $("<div class='" + s.errorClass + "'><b>" + files[i].name + "</b> " + s.duplicateErrorStr + "</div>").appendTo(obj.errorLog);
                    continue;
                }
                if (s.maxFileSize != -1 && files[i].size > s.maxFileSize) {
                    if (s.showError) $("<div class='" + s.errorClass + "'><b>" + files[i].name + "</b> " + s.sizeErrorStr + getSizeStr(s.maxFileSize) + "</div>").appendTo(
                        obj.errorLog);
                    continue;
                }
                if (s.maxFileCount != -1 && obj.selectedFiles >= s.maxFileCount) {
                    if (s.showError) $("<div class='" + s.errorClass + "'><b>" + files[i].name + "</b> " + s.maxFileCountErrorStr + s.maxFileCount + "</div>").appendTo(
                        obj.errorLog);
                    continue;
                }
                obj.selectedFiles++;
                obj.existingFileNames.push(files[i].name);
                // Make object immutable
                var ts = $.extend({}, s);
                var fd = new FormData();
                var fileName = s.fileName.replace("[]", "");
                fd.append(fileName, files[i]);
                var extraData = s.formData;
                if (extraData) {
                    var sData = serializeData(extraData);
                    for (var j = 0; j < sData.length; j++) {
                        if (sData[j]) {
                            fd.append(sData[j][0], sData[j][1]);
                        }
                    }
                }
                ts.fileData = fd;

                var pd = new createProgressDiv(obj, s);
                var fileNameStr = "";
                
                if (s.showFileCounter)
                    fileNameStr = '<span class="k-file-name" title="' + files[i].name + '">' + obj.fileCounter + s.fileCounterStyle + files[i].name + '</span>';
                else fileNameStr = '<span class="k-file-name" title="' + files[i].name + '">' + files[i].name + '</span>';


                if (s.showFileSize)
                    fileNameStr += '<span class="k-file-size">' + getSizeStr(files[i].size) + '</span>';

                pd.filename.html(fileNameStr);
                var form = $("<form style='display:block; position:absolute;left: 150px;' class='" + obj.formGroup + "' method='" + s.method + "' action='" +
                    s.url + "' enctype='" + s.enctype + "'></form>");
                form.appendTo('body');
                var fileArray = [];
                fileArray.push(files[i].name);

                ajaxFormSubmit(form, ts, pd, fileArray, obj, files[i]);
                obj.fileCounter++;
            }
        }

        function isFileTypeAllowed(obj, s, fileName) {
            var fileExtensions = s.allowedTypes.toLowerCase().split(/[\s,]+/g);
            var ext = fileName.split('.').pop().toLowerCase();
            if (s.allowedTypes != "*" && jQuery.inArray(ext, fileExtensions) < 0) {
                return false;
            }
            return true;
        }

        function isFileDuplicate(obj, filename) {
            var duplicate = false;
            if (obj.existingFileNames.length) {
                for (var x = 0; x < obj.existingFileNames.length; x++) {
                    if (obj.existingFileNames[x] == filename
                        || s.duplicateStrict && obj.existingFileNames[x].toLowerCase() == filename.toLowerCase()
                    ) {
                        duplicate = true;
                    }
                }
            }
            return duplicate;
        }

        function removeExistingFileName(obj, fileArr) {
            if (obj.existingFileNames.length) {
                for (var x = 0; x < fileArr.length; x++) {
                    var pos = obj.existingFileNames.indexOf(fileArr[x]);
                    if (pos != -1) {
                        obj.existingFileNames.splice(pos, 1);
                    }
                }
            }
        }

        function getSrcToPreview(file, obj) {
            if (file) {
                obj.show();
                var reader = new FileReader();
                reader.onload = function (e) {
                    obj.attr('src', e.target.result);
                };
                reader.readAsDataURL(file);
            }
        }

        function updateFileCounter(s, obj) {
            
            if (s.showFileCounter) {
                var count = $(obj.container).find(".k-file-name").length;
                obj.fileCounter = count + 1;
                 
                $(obj.container).find(".k-file-name").each(function (i, items) {
                    var arr = $(this).html().split(s.fileCounterStyle);
                    var fileNum = parseInt(arr[0]) - 1; //decrement;
                    var name = count + s.fileCounterStyle + arr[1];
                    $(this).html(name);
                    count--;

                });
            } 
        }

        function createCustomInputFile(obj, group, s, uploadLabel) {

            var fileUploadId = "ajax-upload-id-" + (new Date().getTime());

            var form = $("<form method='" + s.method + "' action='" + s.url + "' enctype='" + s.enctype + "'>" +
                       
                "</form> ");

            var mobileStr = this.IsMobile ? ' capture="camera" ' : "";

            var fileInputStr = "<input type='file' id='" + fileUploadId + "' name='" + s.fileName + "' accept='" + s.acceptFiles + "'" + mobileStr + "/>";
            if (s.multiple) {
                if (s.fileName.indexOf("[]") != s.fileName.length - 2) // if it does not endwith
                {
                    s.fileName += "[]";
                }
                fileInputStr = "<input type='file' id='" + fileUploadId + "' name='" + s.fileName + "' accept='" + s.acceptFiles + "' multiple " + mobileStr + "/>";
            }


            var fileInput = $(fileInputStr).appendTo(form);

            fileInput.change(function () {
                
                obj.errorLog.html("");
                var fileExtensions = s.allowedTypes.toLowerCase().split(",");
                var fileArray = [];
                if (this.files) //support reading files
                {
                    for (i = 0; i < this.files.length; i++) {
                        fileArray.push(this.files[i].name);
                    }

                    if (s.onSelect(this.files) == false) return;
                } else {
                    var filenameStr = $(this).val();
                    var flist = [];
                    fileArray.push(filenameStr);
                    if (!isFileTypeAllowed(obj, s, filenameStr)) {
                        if (s.showError) $("<div class='" + s.errorClass + "'><b>" + filenameStr + "</b> " + s.extErrorStr + s.allowedTypes + "</div>").appendTo(
                            obj.errorLog);
                        return;
                    }
                    //fallback for browser without FileAPI
                    flist.push({
                        name: filenameStr,
                        size: 'NA'
                    });
                    if (s.onSelect(flist) == false) return;

                }

                var count = fileArray.length;

                if (count <= 0) {
                    $("input[type='hidden'][name='" + obj.objectName + "']").val("");
                } else {
                    $("input[type='hidden'][name='" + obj.objectName + "']").val(obj.objectName);
                }

                updateFileCounter(s, obj);

                uploadLabel.unbind("click");
                form.hide();
                createCustomInputFile(obj, group, s, uploadLabel);
                form.addClass(group);
                if (s.serialize && feature.fileapi && feature.formdata) //use HTML5 support and split file submission
                {
                    form.removeClass(group); //Stop Submitting when.
                    var files = this.files;
                    form.remove();
                    serializeAndUploadFiles(s, obj, files);
                } else {
                    var fileList = "";
                    for (var i = 0; i < fileArray.length; i++) {
                        if (s.showFileCounter) fileList += obj.fileCounter + s.fileCounterStyle + fileArray[i] + "<br>";
                        else fileList += fileArray[i] + "<br>";;
                        obj.fileCounter++;

                    }
                    if (s.maxFileCount != -1 && (obj.selectedFiles + fileArray.length) > s.maxFileCount) {
                        if (s.showError) $("<div class='" + s.errorClass + "'><b>" + fileList + "</b> " + s.maxFileCountErrorStr + s.maxFileCount + "</div>").appendTo(
                            obj.errorLog);
                        return;
                    }
                    obj.selectedFiles += fileArray.length;
                   
                    var pd = new createProgressDiv(obj, s);
                    pd.filename.html(fileList);
                    ajaxFormSubmit(form, s, pd, fileArray, obj, null);
                    
                }



            });

            if (s.nestedForms) {
                form.css({
                    'margin': 0,
                    'padding': 0
                });
                uploadLabel.css({
                    position: 'relative',
                    overflow: 'hidden',
                    cursor: 'default'
                });
                fileInput.css({
                    position: 'absolute',
                    'cursor': 'pointer',
                    'top': '0px',
                    'width': '100%',
                    'height': '100%',
                    'left': '0px',
                    'z-index': '100',
                    'opacity': '0.0',
                    'filter': 'alpha(opacity=0)',
                    '-ms-filter': "alpha(opacity=0)",
                    '-khtml-opacity': '0.0',
                    '-moz-opacity': '0.0'
                });
                form.appendTo(uploadLabel);

            } else {
                form.appendTo($('body'));
                form.css({
                    margin: 0,
                    padding: 0,
                    display: 'block',
                    position: 'absolute',
                    left: '-250px'
                });
                if (navigator.appVersion.indexOf("MSIE ") != -1) //IE Browser
                {
                    uploadLabel.attr('for', fileUploadId);
                } else {
                    uploadLabel.click(function () {
                        fileInput.click();
                    });
                }
            }
        }

        function defaultOnLoadCallback(obj) {
            if (obj.DocId === undefined || obj.DocId === null) {
                return false;
            }
            if (obj.DocId === "00000000-0000-0000-0000-000000000000") {
                return false;
            }
            $.ajax({
                cache: false,
                url: app.resolveUrl("~/api/files/?docId=" + obj.DocId + "&fileName=" + obj.fileName),
                dataType: "json",
                success: function (data) {
                    for (var i = 0; i < data.files.length; i++) {
                        obj.createProgress(data.files[i]["name"], data.files[i]["path"], data.files[i]["size"], data.files[i]["id"]);
                    }
                }
            });
        }

        function defaultDownloadCallback(o, data, pd, e) {

            if (e !== undefined) {
                e.preventDefault();
            }

            var fileId = data.files[0].Id;
            var fileName = data.files[0].FileName;

            var win = window.open(app.resolveUrl("~/api/files/download?id=" + fileId), '_blank');
            if (win) {
                win.focus();
              
            } else {
                app.warning('Please allow popups for this website to download attachment',"Warning");
            } 

            if (e !== undefined) {
                e.preventDefault();
            }
        }

        function defaultDeleteCallback(data, pd) {
            var fileId = data.files[0].Id;
            var fileName = data.files[0].FileName;
            $("#" + obj.parentForm).find("#" + fileId).remove();
             
            $.ajax({
                type: "DELETE",
                contentType: "application/json",
                url: app.resolveUrl("~/api/files/delete"),
                data: JSON.stringify({
                    Id: fileId,
                    FileName: fileName
                }),
                success: function (response) {
                    var count = obj.selectedFiles;

                    if (count <= 0) {
                        $("input[type='hidden'][name='" + obj.objectName + "']").val("");
                    } else {
                        $("input[type='hidden'][name='" + obj.objectName + "']").val(obj.objectName);
                    }
                }
            });
        }

        function defaultUploadSuccesCallback(files, response, xhr, pd) {

            if (response.files !== undefined && response.files !== null) {

                var form = $("#" + obj.parentForm);
                
                $.each(response.files, function (i, o) {
                    if (o.Id !== null) {
                        obj.Files.push(o.Id);
                        var container = $("<div class='common-file " + $(obj).data("name").replace(".", "_") + "' data-id='" + o.Id + "' data-filename='" + o.FileName + "' id='" + o.Id + "'></div>").appendTo(form);
                        var commonField = $("<input type='hidden' name='Attachments[][CommonFileId]' value ='" + o.Id + "'> " +
                                            " <input type='hidden' name='Attachments[][FieldCategory]' value ='" + $(obj).data("name") + "'>").appendTo(container);

                    } else {
                        showAlert(alertTypes.error, "Attachment gagal di upload, mohon ulangi kembali.", "Upload Failed");
                    }
                });
            } else {
                showAlert(alertTypes.error, "Attachment gagal di upload, mohon ulangi kembali.", "Upload Failed");
            }
          
        }


        function defaultProgressBar(obj, s) {

            $(obj.container).hide();

            this.statusbar = $("<li class='k-file'></li>");
            this.preview = $("<img class='ajax-file-upload-preview' />").width(s.previewWidth).height(s.previewHeight).appendTo(this.statusbar).hide();
            this.progressDiv = $("<div></div>").appendTo(this.statusbar).hide();
            this.progressbar = $('<span class="k-progress" style="width: 0%;"></span>').appendTo(this.progressDiv);
            this.extensionWraper = $('<span class="k-file-extension-wrapper">' +
                '                    <span class="k-file-extension">png</span>' +
                '                    <span class="k-file-state"></span>' +
                '                   </span>').appendTo(this.statusbar);

            this.filename = $('<span class="k-file-name-size-wrapper">' +
                '                    <span class="k-file-name" title="Ajib.png">Ajib.png</span>' +
                '                    <span class="k-file-size">16.67 KB</span>' +
                '               </span>').appendTo(this.statusbar);

            this.buttonContainer = $('<strong class="k-upload-status"></strong>').appendTo(this.statusbar);

            this.abort = $('<button type="button" class="k-button k-upload-action" aria-label="Remove">' +
                '                        <span class="k-icon k-i-close k-i-x" title="Remove"></span>' +
                '                    </button>').appendTo(this.buttonContainer).hide();
            this.cancel = $("<div>" + s.cancelStr + "</div>").appendTo(this.buttonContainer).hide();
            this.done = $("<div>" + s.doneStr + "</div>").appendTo(this.buttonContainer).hide();
            this.download = $("<button class='btn btn-xs'>" + s.downloadStr + "</button>").appendTo(this.buttonContainer).hide();
            this.del = $("<button class='btn btn-xs'>" + s.deleteStr + "</button>").appendTo(this.buttonContainer).hide();

            this.abort.addClass("ajax-file-upload-red");
            this.done.addClass("ajax-file-upload-green");
            this.download.addClass("ajax-file-upload-green");
            this.cancel.addClass("ajax-file-upload-red");
            this.del.addClass("ajax-file-upload-red");

            return this;
        }
        function createProgressDiv(obj, s) {
            var bar = null;
            if (s.customProgressBar)
                bar = new s.customProgressBar(obj, s);
            else
                bar = new defaultProgressBar(obj, s);

            bar.abort.addClass(obj.formGroup);
            bar.abort.addClass(s.abortButtonClass);

            bar.cancel.addClass(obj.formGroup);
            bar.cancel.addClass(s.cancelButtonClass);

            if (s.extraHTML)
                bar.extraHTML = $("<div class='extrahtml'>" + s.extraHTML() + "</div>").insertAfter(bar.filename);

            $(obj.container).show();

            if (s.uploadQueueOrder == 'bottom')
                $($(obj.container).find(".k-upload-files")[0]).append(bar.statusbar);
            else
                $($(obj.container).find(".k-upload-files")[0]).prepend(bar.statusbar);
            return bar;
        }


        function ajaxFormSubmit(form, s, pd, fileArray, obj, file) {
            var currentXHR = null;
            var options = {
                cache: false,
                contentType: false,
                processData: false,
                forceSync: false,
                type: s.method,
                data: s.formData,
                formData: s.fileData,
                dataType: s.returnType,
                headers: s.headers,
                beforeSubmit: function (formData, $form, options) {
                    if (s.onSubmit.call(this, fileArray) != false) {
                        if (s.dynamicFormData) {
                            var sData = serializeData(s.dynamicFormData());
                            if (sData) {
                                for (var j = 0; j < sData.length; j++) {
                                    if (sData[j]) {
                                        if (s.serialize && s.fileData != undefined) options.formData.append(sData[j][0], sData[j][1]);
                                        else options.data[sData[j][0]] = sData[j][1];
                                    }
                                }
                            }
                        }

                        if (s.extraHTML) {
                            $(pd.extraHTML).find("input,select,textarea").each(function (i, items) {
                                if (s.serialize && s.fileData != undefined) options.formData.append($(this).attr('name'), $(this).val());
                                else options.data[$(this).attr('name')] = $(this).val();
                            });
                        }
                        return true;
                    }
                    pd.statusbar.append("<div class='" + s.errorClass + "'>" + s.uploadErrorStr + "</div>");
                    pd.cancel.show()
                    form.remove();
                    pd.cancel.click(function () {
                        mainQ.splice(mainQ.indexOf(form), 1);
                        removeExistingFileName(obj, fileArray);
                        pd.statusbar.remove();
                        s.onCancel.call(obj, fileArray, pd);
                        obj.selectedFiles -= fileArray.length; //reduce selected File count
                        updateFileCounter(s, obj);
                    });
                    return false;
                },
                beforeSend: function (xhr, o) {
                    for (var key in o.headers) {
                        xhr.setRequestHeader(key, o.headers[key]);
                    }

                    pd.progressDiv.show();
                    pd.cancel.hide();
                    pd.done.hide();
                    if (s.showAbort) {
                        pd.abort.show();
                        pd.abort.click(function () {
                            removeExistingFileName(obj, fileArray);
                            xhr.abort();
                            obj.selectedFiles -= fileArray.length; //reduce selected File count
                            s.onAbort.call(obj, fileArray, pd);

                        });
                    }
                    pd.progressbar.css('background-color', 'green');
                    if (!feature.formdata) //For iframe based push
                    {
                        pd.progressbar.width('5%');
                    } else pd.progressbar.width('1%'); //Fix for small files
                },
                uploadProgress: function (event, position, total, percentComplete) {
                    //Fix for smaller file uploads in MAC
                    if (percentComplete > 98) percentComplete = 98;

                    var percentVal = percentComplete + '%';
                    if (percentComplete > 1) pd.progressbar.width(percentVal)
                    if (s.showProgress) {
                        pd.progressbar.html(percentVal);
                        pd.progressbar.css('text-align', 'center');
                        pd.progressbar.css('background-color', 'green');
                    }

                },
                success: function (data, message, xhr) {
                    pd.cancel.remove();
                    progressQ.pop();
                    //For custom errors.
                    if (s.returnType == "json" && $.type(data) == "object" && data.hasOwnProperty(s.customErrorKeyStr)) {
                        pd.abort.hide();
                        var msg = data[s.customErrorKeyStr];
                        s.onError.call(this, fileArray, 200, msg, pd);
                        if (s.showStatusAfterError) {
                            pd.progressDiv.hide();
                            pd.statusbar.append("<span class='" + s.errorClass + "'>ERROR: " + msg + "</span>");
                        } else {
                            pd.statusbar.hide();
                            pd.statusbar.remove();
                        }
                        obj.selectedFiles -= fileArray.length; //reduce selected File count

                        var count = obj.selectedFiles;

                        if (count <= 0) {
                            $("input[type='hidden'][name='" + obj.objectName + "']").val("");
                        } else {
                            $("input[type='hidden'][name='" + obj.objectName + "']").val(obj.objectName);
                        }

                        form.remove();
                        return;
                    }
                    obj.responses.push(data);
                    if (obj.ProgressBar === undefined) obj.ProgressBar = [];

                    obj.ProgressBar.push(pd);

                    pd.progressbar.width('100%')
                    if (s.showProgress) {
                        pd.progressbar.html('100%');
                        pd.progressbar.css('text-align', 'center');
                    }

                    pd.abort.hide();
                    s.onSuccess.call(this, fileArray, data, xhr, pd);
                    if (s.showStatusAfterSuccess) {
                        if (s.showDone) {
                            pd.done.show();
                            pd.done.click(function () {
                                pd.statusbar.hide("slow");
                                pd.statusbar.remove();
                            });
                        } else {
                            pd.done.hide();
                        }
                        if (s.showDelete) {
                            pd.del.show();
                            pd.del.click(function () {
                                removeExistingFileName(obj, fileArray);
                                pd.statusbar.hide().remove();
                                if (s.deleteCallback) s.deleteCallback.call(this, data, pd);
                                obj.selectedFiles -= fileArray.length; //reduce selected File count

                                var count = obj.selectedFiles;

                                if (count <= 0) {
                                    $("input[type='hidden'][name='" + obj.objectName + "']").val("");
                                } else {
                                    $("input[type='hidden'][name='" + obj.objectName + "']").val(obj.objectName);
                                }

                                if (obj.selectedFiles <= 0) {
                                    $(obj.container).hide();
                                }

                                updateFileCounter(s, obj);

                            });
                        } else {
                            pd.del.hide();
                        }
                    } else {
                        pd.statusbar.hide("slow");
                        pd.statusbar.remove();

                    }
                    if (s.showDownload) {
                        pd.download.show();
                        pd.download.click(function (e) {
                            if (s.downloadCallback) s.downloadCallback(this, data, pd, e);
                           
                        });
                    }
                    form.remove();
                },
                error: function (xhr, status, errMsg) {
                    pd.cancel.remove();
                    progressQ.pop();
                    pd.abort.hide();
                    if (xhr.statusText == "abort") //we aborted it
                    {
                        pd.statusbar.hide("slow").remove();
                        updateFileCounter(s, obj);

                    } else {
                        s.onError.call(this, fileArray, status, errMsg, pd);
                        if (s.showStatusAfterError) {
                            pd.progressDiv.hide();
                            pd.statusbar.append("<span class='" + s.errorClass + "'>ERROR: " + errMsg + "</span>");
                        } else {
                            pd.statusbar.hide();
                            pd.statusbar.remove();
                        }
                        obj.selectedFiles -= fileArray.length; //reduce selected File count

                        var count = obj.selectedFiles;

                        if (count <= 0) {
                            $("input[type='hidden'][name='" + obj.objectName + "']").val("");
                        } else {
                            $("input[type='hidden'][name='" + obj.objectName + "']").val(obj.objectName);
                        }
                    }

                    form.remove();
                }
            };

            if (s.showPreview && file != null) {
                if (file.type.toLowerCase().split("/").shift() == "image") getSrcToPreview(file, pd.preview);
            }

            if (s.autoSubmit) {
                form.ajaxForm(options);
                mainQ.push(form);
                submitPendingUploads();

            } else {
                if (s.showCancel) {
                    pd.cancel.show();
                    pd.cancel.click(function () {
                        mainQ.splice(mainQ.indexOf(form), 1);
                        removeExistingFileName(obj, fileArray);
                        form.remove();
                        pd.statusbar.remove();
                        s.onCancel.call(obj, fileArray, pd);
                        obj.selectedFiles -= fileArray.length; //reduce selected File count

                        var count = obj.selectedFiles;

                        if (count <= 0) {
                            $("input[type='hidden'][name='" + obj.objectName + "']").val("");
                        } else {
                            $("input[type='hidden'][name='" + obj.objectName + "']").val(obj.objectName);
                        }
                        updateFileCounter(s, obj);
                    });
                }
                form.ajaxForm(options);
            }

        }

        var t = this;

        $.uploadFile = function () {
            return t;
        }
        

    }
}(jQuery));
