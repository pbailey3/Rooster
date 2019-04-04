$("#menu-toggle").click(function(e) {
    e.preventDefault();
     $("#wrapper").toggleClass("toggled");
     // doing close all method on metisMenu
     $('#menu').metisMenu('closeAll');
});

$.getScript('../Scripts/Chart.js', function () {
  var data = {
    labels: ["", "", "", "", "", "", "",""],
    datasets: [
        {
            label: "This weeks shifts",
            fillColor: "rgba(67,82,95,1)",
            strokeColor: "rgba(255,255,255,1)",
            pointColor: "rgba(35,214,142,1)",
            pointStrokeColor: "#23D68E",
            pointHighlightFill: "#23D68E",
            pointHighlightStroke: "rgba(35,214,142,1)",
            data: ((typeof chartData != 'undefined') ? chartData : null)
        },        
    ]
};
    var options = {
        animation: true,
        responsive: true,
        scaleShowGridLines: false,
        bezierCurveTension : 0.3,
        maintainAspectRatio: true,
        tooltipTemplate: "<%if (label){%><%=label%>: <%}%><%= value %> hour shift",
    };  
    var ctl = $('#thisWeek');
    if (ctl.length) {
        var ctx = ctl.get(0).getContext('2d');
        thisWeek = new Chart(ctx).Line(data, options);
    }
});


;(function ($, window, document, undefined) {
    var pluginName = "metisMenu",
       defaults = {
           toggle: true,
           onOpen: function () {},
           onClose: function () {}
       };
    function Plugin(element, options) {
        this.element = element;
        this.settings = $.extend({}, defaults, options);
        this._defaults = defaults;
        this._name = pluginName;
        this.init();
    }
    Plugin.prototype = {
        init: function () {
            var metisMenu = this,
                $this = $(this.element),
                $toggle = this.settings.toggle;

            $this.find('li.active').has('ul').children('ul').addClass('collapse in');
            $this.find('li').not('.active').has('ul').children('ul').addClass('collapse');

            //Ensure 1st level nav elements when clicked retain the selected 'active' highlights
            $this.find('li').not(":has(ul)").children('a').on('click', function (e) {
                $(this).parent('li').toggleClass('active');

                //If this is a second level nav with no children then parent is a UL
                $(this).parent('ul').toggleClass('active');

                if ($(this).parent('li').hasClass('active')) {
                    //Remove any active tags front othe menu items at same level
                    $(this).parent('li').siblings().removeClass('active');
                }
            });
            //If this is a 1st level nav which has 2nd level nav child elements
            $this.find('li').has('ul').children('a').on('click', function (e) {
                //Remove all other active classes
                $(this).parent('li').find('li').removeClass('active');
                e.preventDefault();
                $(this).parent('li').toggleClass('active').children('ul').collapse('toggle');

                ////If the second level nav is hidden and there is a third level, then expand the third level
                //if (!$(this).parent('li').children('ul').children('li').children('a').length)
                //{
                //    $(this).parent('li').children('ul').children('li').children('ul').collapse('toggle');
                //}

                if ($(this).parent('li').hasClass('active')) {
                    metisMenu.settings.onOpen.call();
                    $(this).parent('li').siblings().removeClass('active').children('ul.in').collapse('hide');
                } else {
                    metisMenu.settings.onClose.call();
                }

                if ($toggle) {
                    $(this).parent('li').siblings().removeClass('active').children('ul.in').collapse('hide');
                }
                
            });
        },
        closeAll: function() {
            var item = $(this.element).find('li').has('ul').children('a').parent('li');
            item.each(function() {
                if ($(this).hasClass('active')) {
                    $(this).removeClass('active');
                    
                    var childrenOpen = $(this).children('ul.collapse.in');
                    childrenOpen.each(function() {
                        $(this).collapse('hide');
                    });
                }
            });
        }
    };
    $.fn[ pluginName ] = function (options) {
        return this.each(function () {
            if (!$.data(this, "plugin_" + pluginName)) {
                $.data(this, "plugin_" + pluginName, new Plugin(this, options));
            } else {
                var pluginInstance = $.data(this, "plugin_" + pluginName);
                if ($.isFunction(pluginInstance[options])) {
                    pluginInstance[options].apply(pluginInstance, arguments);
                }
            }
        });
    };
})(jQuery, window, document);
$(function () {
    function checkToggled() {
        var wrapper = $('#wrapper'),
            windowWidth = $(window).width();

        if (windowWidth > 768) {
            if (wrapper.hasClass('toggled')) {
                wrapper.removeClass('toggled');
            }
        } else {
            if (!wrapper.hasClass('toggled')) {
                wrapper.addClass('toggled');
            }
        }
    }
    $('#menu').metisMenu({
        toggle: false,
        onOpen: function () {
            checkToggled();
        }
    });
});