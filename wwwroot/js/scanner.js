window.qrScanner = {
    html5QrcodeScanner: null,

    start: function (elementId, dotnetHelper) {
        if (this.html5QrcodeScanner) {
            this.html5QrcodeScanner.clear();
        }

        this.html5QrcodeScanner = new Html5QrcodeScanner(
            elementId,
            { fps: 10, qrbox: { width: 250, height: 250 } },
            /* verbose= */ false
        );

        this.html5QrcodeScanner.render((decodedText, decodedResult) => {
            // Successfully scanned
            dotnetHelper.invokeMethodAsync('OnQrCodeScanned', decodedText);
            
            // Stop scanning after first success
            this.stop();
        }, (errorMessage) => {
            // parse error, ignore it.
        });
    },

    stop: function () {
        if (this.html5QrcodeScanner) {
            this.html5QrcodeScanner.clear().catch(error => {
                console.error("Failed to clear html5QrcodeScanner. ", error);
            });
            this.html5QrcodeScanner = null;
        }
    }
};
