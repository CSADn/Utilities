/// <reference path="../../Libraries/jquery/jquery-3.4.1.min.js" />
/// <reference path="../../Libraries/fitty/fitty.min.js" />
/// <reference path="../../Libraries/rangeslider.js-2.3.0/rangeslider.min.js" />

var HomeView = new (function () {

    var volume;
    var bypassVolume;

    this.init = function () {

        bypassVolume = false;

        buildControls();
        bindEvents();

    }


    function buildControls() {

        fitty('.play, .pause, .stop');

        $('.volume input[type=range]').rangeslider({
            polyfill: false,

            onSlide: function (p, v) {

                var newVolume = parseFloat($('input[type=range]').val());

                if (volume == newVolume)
                    return;

                volume = newVolume;

                $('.volume .info span').text(v + '%');

                if (bypassVolume)
                    return;

                $.post('/Home/SetVolume', { value: volume });
            }
        });

    }

    function bindEvents() {

        $('.play').on('click', function (e) {
            $.post('/Home/Play');
        });

        $('.pause').on('click', function (e) {
            $.post('/Home/Pause');
        });

        $('.stop').on('click', function (e) {
            $.post('/Home/Stop');
        });

        $('.b-day').on('click', function (e) {
            $.post('/Home/BDay');

            bypassVolume = true;
            var n = 0;
            var v;

            var timer = setInterval(function () {

                $.post('/Home/GetVolume')
                    .done(function (result) {

                        var percent = Math.round(result);

                        $('.volume .info span').text(percent + '%');
                        $('input[type=range]').val(percent).change();

                        if (percent <= 0)
                            n = 50;

                        console.log('percent: ' + percent);
                    });

                n++;

                if (n >= 50) {
                    clearInterval(timer);
                    bypassVolume = false;
                }

                console.log('n: ' + n);
            }, 500);

        });
    }

});

$(function () {

    HomeView.init();

});