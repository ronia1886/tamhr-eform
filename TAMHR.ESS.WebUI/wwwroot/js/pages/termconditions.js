(function ($, app, kendo) {
    app.registerHandler("contentCallback", function () {
        var $this = $(this),
            $htmlContent = $this.find(".custom-content"),
            $form = $("#mainForm"),
            data = $form.serializeObject(),
            template = kendo.template($("[data-template='term-and-conditions']").html());

        $htmlContent.append("<h3 class='font-medium-1 bold'>" + app.translate("Form Data") + "</h3>");
        $htmlContent.append(template(data));
        $htmlContent.append("<h3 class='font-medium-1 bold'>" + app.translate("List of Benefits") + "</h3>");
        $htmlContent.append($("#divHeader .list-style-icons").clone());
    });

    app.registerHandler("precallbackHandler", function () {
        $("#termAndConditionsBtn").trigger("click");

        return false;
    });

    $(function () {
        $("a[ data-parameters-action='initiate']").data("confirmation", false);
    });
})(jQuery, app, kendo);