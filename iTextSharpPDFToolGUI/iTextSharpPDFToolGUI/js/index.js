/// <reference path="pdf-manager.js" />

var IndexView = new (function () {

    var self = this;
    var controlsPanel;
    var viewer;
    var viewerPage;
    var viewerPageWidth;
    var viewerPageHeight;
    var guides;
    var canvas;
    var mouse;
    var paperSize;


    this.Init = function () {

        initControls();
        initControlPanelEvents();
        initViewerEvents();
    }


    function initControls() {

        controlsPanel = $('.controls-panel');

        viewer = $('.pdf-viewer');
        viewerPage = $('.pdf-viewer-page', viewer);

        viewerPageWidth = viewerPage.width() - 80;
        viewerPageHeight = viewerPage.height() - 64; // 30 de márgenes top bottom

        guides = {
            x_t: $('.guide-x.t', viewer),
            x_b: $('.guide-x.b', viewer),
            y_l: $('.guide-y.l', viewer),
            y_r: $('.guide-y.r', viewer),

            coord: {
                self: $('.guide-coord', viewerPage),
                x: $('.guide-coord .x', viewerPage),
                y: $('.guide-coord .y', viewerPage),

                on: function () {
                    if (!this.self.is(':visible'))
                        this.self.fadeIn(500);
                },
                off: function () {
                    this.self.hide();
                },
                move: function (x, y) {

                    var self = {
                        width: this.self.prop('clientWidth'),
                        height: this.self.prop('clientHeight')
                    };

                    var page = {
                        width: canvas.width(),
                        height: canvas.height(),
                        left: canvas.prop('offsetLeft'),
                        top: canvas.prop('offsetTop')
                    };

                    var scroll = {
                        left: viewerPage.prop('scrollLeft'),
                        top: viewerPage.prop('scrollTop')
                    };

                    var top = y;
                    var left = x;

                    if (x > (page.left + self.width + scroll.left))
                        left = x - self.width;

                    if (y > (page.top + self.height + scroll.top))
                        top = y - self.height;

                    this.self.css('top', top);
                    this.self.css('left', left);

                    var realX = ((paperSize.width / page.width) * (x - page.left)).toFixed(1);
                    var realY = ((paperSize.height / page.height) * (y - page.top - 1)).toFixed(1);

                    this.x.text(realX);
                    this.y.text(realY);

                    mouse = {
                        x: realX,
                        y: realY
                    };
                }
            },

            on: function () {
                if (!this.x_t.is(':visible'))
                    this.x_t.fadeIn(500);

                if (!this.x_b.is(':visible'))
                    this.x_b.fadeIn(500);

                if (!this.y_l.is(':visible'))
                    this.y_l.fadeIn(500);

                if (!this.y_r.is(':visible'))
                    this.y_r.fadeIn(500);

                this.coord.on();
            },

            off: function () {
                this.x_t.hide();
                this.x_b.hide();
                this.y_l.hide();
                this.y_r.hide();

                this.coord.off();
            },

            move: function (x, y) {

                var rect = {
                    top: parseFloat(viewerPage.css('top')) + viewerPage.prop('offsetTop'),
                    left: parseFloat(viewerPage.css('left')) + viewerPage.prop('offsetLeft'),
                    right: parseFloat(viewerPage.css('left')) + viewerPage.prop('offsetLeft') + viewerPage.prop('clientWidth'),
                    bottom: parseFloat(viewerPage.css('top')) + viewerPage.prop('offsetTop') + viewerPage.prop('clientHeight')
                };

                this.x_t.css('left', x);
                this.x_t.css('top', rect.top);
                this.x_t.css('height', y - rect.top - 21);

                this.x_b.css('left', x);
                this.x_b.css('top', y + 21);
                this.x_b.css('height', rect.bottom - rect.top - y - 10);

                this.y_l.css('left', rect.left);
                this.y_l.css('top', y);
                this.y_l.css('width', x - rect.left - 21);

                this.y_r.css('left', x + 21);
                this.y_r.css('top', y);
                this.y_r.css('width', rect.right - rect.left - x - 10);

                this.coord.move(
                    x - 10 + viewerPage.prop('scrollLeft'),
                    y - 10 + viewerPage.prop('scrollTop')
                );
            }
        };

        canvas = $('canvas', viewerPage);

        // A4: 595x842
        paperSize = {
            width: 595,
            height: 842
        };
    }

    function initControlPanelEvents() {

        $('input.btLoad', controlsPanel).on('click', function (e) {

            var settings = {
                viewerWidth: viewerPageWidth,
                viewerHeight: viewerPageHeight
            };

            PDFManager.Load(settings)
                .done(function () {
                    $('.pdf-viewer canvas').fadeIn();
                });

        });

        $('input.btZoomIn', controlsPanel).on('click', function (e) {
            PDFManager.ZoomIn();
        });

        $('input.btZoomOut', controlsPanel).on('click', function (e) {
            PDFManager.ZoomOut();
        });

        $('input.btFullPage', controlsPanel).on('click', function (e) {
            PDFManager.ZoomFullPage();
        });

        $('input.btWidth', controlsPanel).on('click', function (e) {
            PDFManager.ZoomWidth();
        });

    }

    function initViewerEvents() {

        var scrolled = false;

        viewerPage
            .on('scroll', function (e) {

                if (!scrolled) {
                    scrolled = true;

                    guides.off();

                    setTimeout(function () {
                        scrolled = false;
                    }, 250);
                }
            });

        canvas
            .on('mousemove', function (e) {

                guides.on();
                guides.move(e.pageX, e.pageY);

            })
            .on('mouseleave', function (e) {

                if (e.relatedTarget === null)
                    return;

                var target = e.relatedTarget.className;

                if (target !== 'guide-x' && target !== 'guide-y')
                    guides.off();

                mouse = null;

            });
    }

});

$(function () {
    IndexView.Init();
})
