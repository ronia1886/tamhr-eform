(function ($, app) {
    function toggleClass($el) {
        let $span = $el.find("span"),
            $icon = $el.find("i");

        $el.data("add", !$el.data("add"));
        $span.toggleClass("badge-danger").toggleClass("badge-success");
        $icon.toggleClass("ft-minus").toggleClass("ft-plus");
    }

    app.registerHandler("dataInterceptor", function () {
        var $this = $(this),
            $modal = $this.closest(".modal"),
            ids = $modal.find("div[data-favourite='true']").find(".flex-icon-action").toArray().map(x => {
                return x.getAttribute("data-id");
            });

        return ids;
    });

    app.registerHandler("switchHandler", function () {
        let $this = $(this),
            $form = $this.closest("form"),
            $favouriteWrapper = $form.find("div[data-favourite='true']").find(".flex-wrap"),
            id = $this.data("id"),
            isNew = $this.data("add"),
            maxFavouriteItems = app.get("maxFavouriteItems") || 8;

        if (!isNew) {
            toggleClass($this);

            let selector = `a[data-id='${id}']`;

            $favouriteWrapper.find(selector).parent().remove();

            let $refBtn = $form.find(selector);
            if ($refBtn[0] != $this[0]) {
                toggleClass($refBtn);
            }
        } else {
            var len = $favouriteWrapper.find(".flex-icon-item").length;

            if (len + 1 > maxFavouriteItems) {
                app.error(`Cannot add to favourite items, Max is ${maxFavouriteItems} items`);
            } else {
                toggleClass($this);

                $favouriteWrapper.append($this.parent().clone(true));
            }
        }
    });

    $(function () {
        $("body").addClass("home-with-carousel");
    });
})(jQuery, app);