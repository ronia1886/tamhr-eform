/*!
  * App Notification v1.0.0
  * Copyright 2018 by Hensem Brian
  */
(function ($) {
    $(function () {
        $(".notification-wrapper").on("get.notification", function () {
            var $this = $(this),
                url = $this.data("url"),
                readUrl = $this.data("readUrl"),
                viewUrl = $this.data("viewUrl"),
                templateId = $this.data("template"),
                badgeClass = $this.data("badgeClass") || "badge-primary",
                affectedMenu = $this.data("affectedMenu") || null;

            if (typeof readUrl !== 'undefined' && readUrl != '' && readUrl != null) {
                $this.on("click", function () {
                    $.post(readUrl, { }, function () {
                        $this.find(".notification-count").remove();
                    });
                });
            }

            $.get(url, {}, function (d) {
                $this.find(".dropdown-toggle > span").remove();
                if (affectedMenu != null) {
                    $("a.nav-link[href$='" + affectedMenu + "'] span.badge").remove();
                }

                if (d.Total > 0) {
                    if (affectedMenu != null) {
                        $("a.nav-link[href$='" + affectedMenu + "']").append("<span class='badge " + badgeClass + "'>" + d.Total + "</span>");
                    }

                    $this.find(".dropdown-toggle").append("<span class=\"badge " + badgeClass + " notification-count\"> " + d.Total + " </span>");
                }

                var tpl = kendo.template($("#" + templateId).html());
                var output = tpl({ ViewAllUrl: viewUrl, Total: d.Total, Messages: d.Messages });

                $this.find(".dropdown-menu").html(output);
            });
        });

        $(".notification-wrapper").trigger("get.notification");
    });
}(jQuery));