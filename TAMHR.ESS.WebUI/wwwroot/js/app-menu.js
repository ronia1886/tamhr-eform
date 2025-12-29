const Layout = (function($) {
    class Layout {
        constructor() {
        }
        updateHeight() {
            let $mainMenu = $('.main-menu-content');
    
            if ($mainMenu.length > 0) {
                $mainMenu.css('height', $(window).height() - $('header.navbar').height());
                $mainMenu.perfectScrollbar('update');
            }
        }
        init() {
            let cls = this,
                $mainMenuContent = $(".main-menu-content"),
                $menuToggler = $(".menu-toggler"),
                $fixedMenuToggler = $(".menu-toggle"),
                $link = $(".navigation .nav-item.has-sub > a"),
                $mainMenu = $(".main-menu,.navbar-brand");
            
            if ($fixedMenuToggler.length > 0) {
                $fixedMenuToggler.on("click", function() {
                    let $this = $(this),
                        $li = $this.closest("li"),
                        $fixedMenu = $($this.data("fixedMenu"));

                    $li.toggleClass("selected");
                    $fixedMenu.toggleClass("hidden");

                    if ($fixedMenu.is(":visible")) {
                        $fixedMenu.scrollTop(0);
                        $("body").addClass("overflow-hidden");
                    } else {
                        $("body").removeClass("overflow-hidden");
                    }
                });
            }

            if ($mainMenuContent.length > 0) {
                $mainMenuContent.perfectScrollbar({
                    suppressScrollX: true,
                    theme: 'light'
                });
            }
    
            if ($menuToggler.length > 0) {
                $menuToggler.on("click", function() {
                    let $this = $(this),
                        $body = $("body");
                    
                    $body.toggleClass("menu-expanded menu-collapsed");
                    $body.removeClass("menu-hover");

                    cls.updateHeight();
                });

                $mainMenu.on("mouseenter", function(e) {
                    let $this = $(this),
                        $body = $("body");

                    if ($body.hasClass("menu-collapsed") && !$body.hasClass("menu-hover")) {
                        $body.addClass("menu-hover");
                        cls.updateHeight();
                    }
                }).on("mouseleave", function(e) {
                    let $this = $(this),
                        $body = $("body");

                    if ($body.hasClass("menu-collapsed") && $body.hasClass("menu-hover") && $(".navbar-brand:hover").length === 0) {
                        $body.removeClass("menu-hover");
                    }
                });
            }
    
            if ($link.length > 0) {
                $link.on("click", function(e) {
                    let $this = $(this),
                        $parent = $this.closest(".nav-item");
                    
                    if (!$parent.hasClass("open")) {
                        $parent.siblings().removeClass("open");
                    }
        
                    $parent.toggleClass("open");
                    
                    cls.updateHeight();
        
                    e.preventDefault();
                });
            }
    
            $(window).resize(function() {
                cls.updateHeight();
            });
        }
    }

    return Layout;
})(jQuery);