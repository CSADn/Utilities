<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Index.aspx.cs" Inherits="iTextSharpPDFToolGUI.Index" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">

    <head runat="server">
        <title>iTextSharpPDF - ToolGUI</title>

        <link href="css/index.css" rel="stylesheet" />
    </head>

    <body>

        <div class="pdf-viewer">
            <div class="pdf-viewer-page">
                <canvas></canvas>
                <div class="guide-margins"></div>
                <div class="guide-coord">
                    <span>x:</span><span class="x">0.0</span> <span>y:</span><span class="y">0.0</span>
                </div>
            </div>
            <div class="guide-x t"></div>
            <div class="guide-x b"></div>
            <div class="guide-y l"></div>
            <div class="guide-y r"></div>
        </div>

        <div class="controls-panel">
            <div><input type="button" class="btLoad" value="Load"/></div>
            <div><input type="button" class="btZoomIn" value="Zoom +"/></div>
            <div><input type="button" class="btZoomOut" value="Zoom -"/></div>
            <div><input type="button" class="btFullPage" value="Full page"/></div>
            <div><input type="button" class="btWidth" value="Width"/></div>
        </div>

    </body>

    <script>
        var RootPath = '<%= (Request.ApplicationPath == "/" ? string.Empty : Request.ApplicationPath) %>';
        var handler = RootPath + '/ajax/handler.ashx?method='
    </script>

    <script src="libs/jquery-3.2.1.min.js"></script>

    <!-- https://github.com/mozilla/pdf.js -->
    <!-- https://www.jsdelivr.com/package/npm/pdfjs-dist?path=build -->
    <script src="libs/pdf.min.js"></script>

    <script src="js/pdf-manager.js"></script>
    <script src="js/index.js"></script>

</html>
