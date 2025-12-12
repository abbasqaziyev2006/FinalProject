/**
    * selectImages
    * menuleft
    * tabs
    * progresslevel
    * collapse_menu
    * fullcheckbox
    * showpass
    * gallery
    * coppy
    * select_colors_theme
    * icon_function
    * box_search
    * preloader
*/

; (function ($) {

    "use strict";

    var selectImages = function () {
        if ($(".image-select").length > 0) {
            const selectIMG = $(".image-select");
            selectIMG.find("option").each((idx, elem) => {
                const selectOption = $(elem);
                const imgURL = selectOption.attr("data-thumbnail");
                if (imgURL) {
                    selectOption.attr(
                        "data-content",
                        "<img src='%i'/> %s"
                            .replace(/%i/, imgURL)
                            .replace(/%s/, selectOption.text())
                    );
                }
            });
            selectIMG.selectpicker();
        }
    };

    var menuleft = function () {
        if ($('div').hasClass('section-menu-left')) {
            var bt = $(".section-menu-left").find(".has-children");
            bt.on("click", function () {
                var args = { duration: 200 };
                if ($(this).hasClass("active")) {
                    $(this).children(".sub-menu").slideUp(args);
                    $(this).removeClass("active");
                } else {
                    $(".sub-menu").slideUp(args);
                    $(this).children(".sub-menu").slideDown(args);
                    $(".menu-item.has-children").removeClass("active");
                    $(this).addClass("active");
                }
            });
            $('.sub-menu-item').on('click', function (event) {
                event.stopPropagation();
            });
        }
    };

    var tabs = function () {
        $('.widget-tabs').each(function () {
            $(this).find('.widget-content-tab').children().hide();
            $(this).find('.widget-content-tab').children(".active").show();
            $(this).find('.widget-menu-tab').find('li').on('click', function () {
                var liActive = $(this).index();
                var contentActive = $(this).siblings().removeClass('active').parents('.widget-tabs').find('.widget-content-tab').children().eq(liActive);
                contentActive.addClass('active').fadeIn("slow");
                contentActive.siblings().removeClass('active');
                $(this).addClass('active').parents('.widget-tabs').find('.widget-content-tab').children().eq(liActive).siblings().hide();
            });
        });
    };

    $('ul.dropdown-menu.has-content').on('click', function (event) {
        event.stopPropagation();
    });

    $('.button-close-dropdown').on('click', function () {
        $(this).closest('.dropdown').find('.dropdown-toggle').removeClass('show');
        $(this).closest('.dropdown').find('.dropdown-menu').removeClass('show');
    });

    var progresslevel = function () {
        if ($('div').hasClass('progress-level-bar')) {
            var bars = document.querySelectorAll('.progress-level-bar > span');
            setInterval(function () {
                bars.forEach(function (bar) {
                    var t1 = parseFloat(bar.dataset.progress);
                    var t2 = parseFloat(bar.dataset.max);
                    var getWidth = (t1 / t2) * 100;
                    bar.style.width = getWidth + '%';
                });
            }, 500);
        }
    };

    var collapse_menu = function () {
        $(".button-show-hide").on("click", function () {
            $('.layout-wrap').toggleClass('full-width');
        });
    };

    var fullcheckbox = function () {
        $('.total-checkbox').on('click', function () {
            if ($(this).is(':checked')) {
                $(this).closest('.wrap-checkbox').find('.checkbox-item').prop('checked', true);
            } else {
                $(this).closest('.wrap-checkbox').find('.checkbox-item').prop('checked', false);
            }
        });
    };

    var showpass = function () {
        $(".show-pass").on("click", function () {
            $(this).toggleClass("active");
            var input = $(this).parents(".password").find(".password-input");

            if (input.attr("type") === "password") {
                input.attr("type", "text");
            } else if (input.attr("type") === "text") {
                input.attr("type", "password");
            }
        });
    };

    var gallery = function () {
        $(".button-list-style").on("click", function () {
            $(".wrap-gallery-item").addClass("list");
        });
        $(".button-grid-style").on("click", function () {
            $(".wrap-gallery-item").removeClass("list");
        });
    };

    var coppy = function () {
        $(".button-coppy").on("click", function () {
            var copyText = document.querySelector(".coppy-content");
            if (copyText) {
                var textToCopy = copyText.textContent || copyText.innerText;
                navigator.clipboard.writeText(textToCopy).then(function () {
                    console.log('Text copied successfully');
                }).catch(function (err) {
                    console.error('Failed to copy text: ', err);
                });
            }
        });
    };

    var select_colors_theme = function () {
        if ($('div').hasClass("select-colors-theme")) {
            $(".select-colors-theme .item").on("click", function (e) {
                $(this).parents(".select-colors-theme").find(".active").removeClass("active");
                $(this).toggleClass("active");
            });
        }
    };

    var icon_function = function () {
        if ($('div').hasClass("list-icon-function")) {
            $(".list-icon-function .trash").on("click", function (e) {
                e.preventDefault();
                $(this).parents(".product-item").remove();
                $(this).parents(".attribute-item").remove();
                $(this).parents(".countries-item").remove();
                $(this).parents(".user-item").remove();
                $(this).parents(".roles-item").remove();
            });
        }
    };

    var box_search = function () {
        // Header dashboard search (dropdown search)
        var $searchBox = $('.box-content-search');
        var $showSearch = $('.show-search');
        var $headerInput = $('.header-dashboard .form-search input');

        // Input yazanda dropdown-u aç
        if ($headerInput.length > 0) {
            $headerInput.on('input', function () {
                if ($(this).val().trim() !== '') {
                    $searchBox.addClass('active');
                } else {
                    $searchBox.removeClass('active');
                }
            });

            // Header input daxilinə klikləyəndə bağlanmasın
            $headerInput.on('click', function (event) {
                event.stopPropagation();
            });
        }

        // Düyməyə kliklə dropdown-u aç/bağla
        if ($showSearch.length > 0) {
            $showSearch.on('click', function (event) {
                event.stopPropagation();
                $searchBox.toggleClass('active');
                $headerInput.focus();
            });
        }

        // Search box daxilinə klikləyəndə bağlanmasın
        if ($searchBox.length > 0) {
            $searchBox.on('click', function (event) {
                event.stopPropagation();
            });

            // Səhifənin digər hissəsinə klikləyəndə dropdown-u bağla
            $(document).on('click', function () {
                $searchBox.removeClass('active');
            });
        }

        // Admin panel search forms - stopPropagation-u tətbiq etmə
        // Sadəcə formu submit et
        $('.wg-filter .form-search').each(function () {
            var $form = $(this);

            // Form submit
            $form.on('submit', function (e) {
                // Normal submit olsun
                return true;
            });

            // Button click
            $form.find('.button-submit button').on('click', function (e) {
                e.preventDefault();
                e.stopPropagation();
                $(this).closest('form').submit();
            });

            // Enter key
            $form.find('input[name="search"]').on('keydown', function (e) {
                if (e.key === 'Enter' || e.keyCode === 13) {
                    e.preventDefault();
                    $(this).closest('form').submit();
                }
            });
        });
    };

    var retinaLogos = function () {
        var retina = window.devicePixelRatio > 1 ? true : false;
        if (retina) {
            if ($(".dark-theme").length > 0) {
                $('#logo_header').attr({ src: 'images/logo/logo.png', width: '154px', height: '52px' });
            } else {
                $('#logo_header').attr({ src: 'images/logo/logo.png', width: '154px', height: '52px' });
            }
        }
    };

    var preloader = function () {
        setTimeout(function () {
            $("#preload").fadeOut("slow", function () {
                $(this).remove();
            });
        }, 1000);
    };

    // Dom Ready
    $(function () {
        selectImages();
        menuleft();
        tabs();
        progresslevel();
        collapse_menu();
        fullcheckbox();
        showpass();
        gallery();
        coppy();
        select_colors_theme();
        icon_function();
        box_search();
        retinaLogos();
        preloader();
    });

})(jQuery);