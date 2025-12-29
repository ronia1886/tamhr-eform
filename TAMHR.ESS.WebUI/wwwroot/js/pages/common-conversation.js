(function ($, app) {
    var currentNoreg = app.get("noreg"),
        ownerChannelId = app.get("channelId"),
        users = app.get("users"),
        connection = app.getHub();

    $.lastUpdateBy = '';
    $.connected = $.connected || false;

    function scrollToBottom() {
        $(".chat-app-window").animate({ scrollTop: $(".chat-app-window .chats").height() }, 0);
    }

    function onReceiveMessage(channelId, noreg, message) {
        if (ownerChannelId === channelId) {
            let content = '',
                $chatWindow = $(".chat-app-window"),
                timeStr = kendo.toString(new Date(), "HH:mm");

            if ($chatWindow.hasClass("hidden")) {
                $chatWindow.closest(".card").find(".no-record").remove();
                $chatWindow.removeClass("hidden");
            }

            if ($.lastUpdateBy !== noreg) {
                content = `<div class="chat ` + (noreg === currentNoreg ? 'chat-left' : '') + `">
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

            $.lastUpdateBy = noreg;
            scrollToBottom();
        }
    }

    app.registerHandler("conversationInterceptor", function (data) {
        data.ChannelId = app.get("channelId");
        data.NoReg = app.get("noreg");
        data.Name = app.get("name");
        data.Members = $.map(app.get("users"), function (item) { return item.NoReg; }).join(",");
        data.Url = app.get("url");

        return data;
    });

    app.registerHandler("conversationHandler", function (d) {
        let $input = $(this).closest("form").find("input"),
            value = $input.val() || "";

        $input.removeClass("is-invalid");

        if (value.trim() !== "") {
            $input.val("");
            connection.invoke("ReceiveMessage", ownerChannelId, d.NoReg, d.Message).catch(function (err) {
                onReceiveMessage(ownerChannelId, d.NoReg, d.Message);

                return console.error(err.toString());
            });
        } else {
            $input.addClass("is-invalid");
        }
    });

    $(function () {
        if (!$.connected) {
            connection.on("ReceiveMessage", onReceiveMessage);
            connection.start()
                .catch(err => console.error(err.toString()));

            $.connected = true;
        }

        scrollToBottom();

        $('input[data-role="chat-input"]').suggest('@', {
            data: users,
            map: function (user) {
                return {
                    value: user.Username,
                    text: '<strong>' + user.Username + '</strong> <small>(' + user.Name + ')</small>'
                };
            }
        });
    });
})(jQuery, app);