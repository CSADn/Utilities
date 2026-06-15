/// <reference path="../libs/jquery-3.2.1.min.js" />
/// <reference path="../libs/pdf.min.js" />

var PDFManager = new (function () {

    var self = this;
    var pdf = null;
    var currentPage = 1;
    var currentScale = 1;
    var defaultSettings = {};

    this.Load = function (settings) {

        if ($.isPlainObject(settings))
            $.extend(defaultSettings, settings);

        var file = 'cotizacion.pdf'
        var project = '20171016224001'
        var url = RootPath + '/projects/' + project + '/pdf/' + file;

        var deferred = $.Deferred();

        pdfjsLib.GlobalWorkerOptions.workerSrc = '../libs/pdf.worker.min.js';

        pdfjsLib.getDocument(url)
            .then(
                function (document) {

                    pdf = document;

                    RenderPage(currentPage, -2)
                        .done(function () {
                            deferred.resolve();
                        });
                },

                function (reason) {
                    pdf = null;
                    deferred.reject();
                    console.error(reason);
                }
            )

        return deferred;
    }

    this.ZoomIn = function () {

        var deferred = $.Deferred();

        if (pdf === null) {
            deferred.reject();
            return deferred;
        }

        currentScale += 0.25;

        if (currentScale > 3.0)
            currentScale = 3.0;

        RenderPage(currentPage, currentScale, false)
            .done(function () {
                deferred.resolve();
            });

        return deferred;
    }

    this.ZoomOut = function () {

        var deferred = $.Deferred();

        if (pdf === null) {
            deferred.reject();
            return deferred;
        }

        currentScale -= 0.25;

        if (currentScale < 0.8)
            currentScale = 0.8;

        RenderPage(currentPage, currentScale, false)
            .done(function () {
                deferred.resolve();
            });

        return deferred;
    }

    this.ZoomFullPage = function () {

        var deferred = $.Deferred();

        if (pdf === null) {
            deferred.reject();
            return deferred;
        }

        RenderPage(currentPage, -1)
            .done(function () {
                deferred.resolve();
            });

        return deferred;
    }

    this.ZoomWidth = function () {

        var deferred = $.Deferred();

        if (pdf === null) {
            deferred.reject();
            return deferred;
        }

        RenderPage(currentPage, -2)
            .done(function () {
                deferred.resolve();
            });

        return deferred;
    }


    function RenderPage(number, zoom, fade) {

        if (fade === undefined || fade === null)
            fade = true;

        var deferred = $.Deferred();

        pdf.getPage(number)
            .then(function (page) {

                var scale = currentScale;

                if (zoom < 0) {
                    var viewport = page.getViewport(1);

                    if (zoom === -1) // Ver página completa
                        scale = defaultSettings.viewerHeight / viewport.height;
                    else if (zoom === -2) // Ajuste al ancho de página
                        scale = defaultSettings.viewerWidth / viewport.width;
                }
                else if (zoom > 0) {
                    if (zoom > 3)
                        scale = 3;
                    else
                        scale = zoom;
                }

                currentScale = scale;
                viewport = page.getViewport(scale);

                // Prepare canvas using PDF page dimensions
                var canvas = $('.pdf-viewer canvas').get(0);
                var context = canvas.getContext('2d');

                canvas.height = viewport.height;
                canvas.width = viewport.width;

                // Render PDF page into canvas context
                var renderContext = {
                    canvasContext: context,
                    viewport: viewport
                };

                if (fade)
                    $(canvas).hide();

                page.render(renderContext)
                    .then(function () {
                        if (fade)
                            $(canvas).fadeIn();
                        deferred.resolve();
                    });
            })

        return deferred;
    }

});