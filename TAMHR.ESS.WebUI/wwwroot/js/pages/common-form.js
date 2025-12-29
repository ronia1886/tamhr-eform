(function ($, app) {
    var lastUpdateBy = '',
        connection = app.getHub();

    function scrollToBottom() {
        $(".chat-app-window").animate({ scrollTop: $(".chat-app-window .chats").height() }, 0);
    }

    function onReceiveMessage(documentApprovalId, noreg, message) {
        if (documentApprovalId === app.get("documentApprovalId")) {
            let content = '',
                $chatWindow = $(".chat-app-window"),
                timeStr = kendo.toString(new Date(), "HH:mm");

            if ($chatWindow.hasClass("hidden")) {
                $chatWindow.closest(".card").find(".no-record").remove();
                $chatWindow.removeClass("hidden");
            }

            if (lastUpdateBy !== noreg) {
                content = `<div class="chat ` + (noreg === app.get("noreg") ? 'chat-left' : '') + `">
                            <div class="chat-avatar">
                                <a class="avatar" data-toggle="tooltip" href="#" data-placement="right" title="" data-original-title="">
                                    <img class="rounded" src="` + app.resolveImageUrl(noreg) + `" alt="avatar" style="width: 2.25rem; height: 2.5rem;">
                                </a>
                            </div>
                            <div class="chat-body">
                                <div class="chat-content">
                                    <p>` + message + `</p>
                                    <span class="d-block mt-2 font-small-1">at ` + timeStr + `</span>
                                </div>
                            </div>
                        </div>`;

                $(".chats").append(content);
            } else {
                content = `<div class="chat-content"><p>${message}</p><span class="d-block mt-2 font-small-1">at ` + timeStr + `</span></div>`;
                $(".chats .chat:last-child .chat-body:last-child").append(content);
            }

            lastUpdateBy = noreg;
            scrollToBottom();
        }
    }

    app.registerHandler("conversationInterceptor", function (data) {
        data.DocumentApprovalId = app.get("documentApprovalId");

        return data;
    });

    app.registerHandler("conversationHandler", function (d) {
        let $input = $(this).closest("form").find("input"),
            value = $input.val() || "";

        $input.removeClass("is-invalid");

        if (value.trim() !== "") {
            $input.val("");
            connection.invoke("ReceiveMessage", d.DocumentApprovalId, d.NoReg, d.Message).catch(function (err) {
                onReceiveMessage(d.DocumentApprovalId, d.NoReg, d.Message);

                return console.error(err.toString());
            });
        } else {
            $input.addClass("is-invalid");
        }
    });

    $(function () {
        var users = app.get("users"),
            canCreateConversation = app.get("canCreateConversation");

        connection.on("ReceiveMessage", onReceiveMessage);
        connection.start().catch(err => console.error(err.toString()));

        scrollToBottom();

        if (canCreateConversation) {
            $('input[data-role="chat-input"]').suggest('@', {
                data: users,
                map: function (user) {
                    return {
                        value: user.Username,
                        text: '<strong>' + user.Username + '</strong> <small>(' + user.Name + ')</small>'
                    };
                }
            });
        }

        $('body').on('change', 'input[type=file]', function (s) {
            var data = $(this).data();

            if (data.autoUpload === undefined || data.autoUpload === true) {
                $(this).upload();
            }
        });

        if ($(".file-upload").length > 0) {
            $(".file-upload").each(function () {
                $(this).uploadFile({
                    url: app.resolveUrl("~/api/files/upload")
                });
            });
        }
    });
})(jQuery, app);