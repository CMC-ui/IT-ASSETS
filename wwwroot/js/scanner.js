window.qrScanner = {
    html5Qrcode: null,

    start: function (elementId, dotnetHelper) {
        if (this.html5Qrcode) {
            this.stop();
        }

        this.html5Qrcode = new Html5Qrcode(elementId);
        
        const config = { fps: 10, qrbox: { width: 250, height: 250 } };

        this.html5Qrcode.start(
            { facingMode: "environment" },
            config,
            (decodedText, decodedResult) => {
                // Successfully scanned
                dotnetHelper.invokeMethodAsync('OnQrCodeScanned', decodedText);
                this.stop();
            },
            (errorMessage) => {
                // parse error, ignore it.
            }
        ).catch(err => {
            console.error("Camera start failed: ", err);
            
            // Inject a friendly error message and a file input fallback into the UI
            const element = document.getElementById(elementId);
            if (element) {
                element.innerHTML = `
                    <div class="alert alert-danger mb-3 p-2" style="font-size: 0.9rem;">
                        <strong><i class="bi bi-exclamation-triangle"></i> Camera Error</strong><br>
                        ${err.name === 'NotAllowedError' ? 'Please grant camera permissions.' : 'Could not start camera (requires HTTPS or a valid camera). Please use the fallback below.'}
                    </div>
                    <div class="mb-3">
                        <label class="btn btn-primary w-100">
                            <i class="bi bi-image"></i> Scan from Image File
                            <input type="file" id="qr-file-input" accept="image/*" hidden />
                        </label>
                    </div>
                `;
                
                document.getElementById('qr-file-input').addEventListener('change', (e) => {
                    if (e.target.files.length === 0) return;
                    const file = e.target.files[0];
                    this.html5Qrcode.scanFile(file, true)
                        .then(decodedText => {
                            dotnetHelper.invokeMethodAsync('OnQrCodeScanned', decodedText);
                            this.stop();
                        })
                        .catch(err => {
                            alert("Could not read QR code from image. Try another picture.");
                        });
                });
            }
        });
    },

    stop: function () {
        if (this.html5Qrcode) {
            try {
                // If it was never fully started (e.g. error caught), stop() will fail, so we catch it
                this.html5Qrcode.stop().then(() => {
                    this.html5Qrcode.clear();
                    this.html5Qrcode = null;
                }).catch(err => {
                    this.html5Qrcode.clear();
                    this.html5Qrcode = null;
                });
            } catch(e) {
                this.html5Qrcode.clear();
                this.html5Qrcode = null;
            }
        }
    }
};

window.qrShare = {
    shareImage: async function (base64Data, filename, toEmail, subject, body) {
        
        const fallbackToEml = () => {
            const boundary = "----=_Part_0_1234567890";
            let emlContent = `X-Unsent: 1\r\nTo: ${toEmail}\r\nSubject: ${subject}\r\nMIME-Version: 1.0\r\nContent-Type: multipart/mixed; boundary="${boundary}"\r\n\r\n--${boundary}\r\nContent-Type: text/plain; charset="utf-8"\r\n\r\n${body}\r\n\r\n--${boundary}\r\nContent-Type: image/png; name="${filename}"\r\nContent-Transfer-Encoding: base64\r\nContent-Disposition: attachment; filename="${filename}"\r\n\r\n${base64Data}\r\n--${boundary}--`;

            const blob = new Blob([emlContent], { type: 'message/rfc822' });
            const url = URL.createObjectURL(blob);
            const a = document.createElement('a');
            a.href = url;
            a.download = `PrintRequest_${filename.replace('.png', '')}.eml`;
            document.body.appendChild(a);
            a.click();
            document.body.removeChild(a);
            URL.revokeObjectURL(url);
        };

        if (!navigator.canShare) {
            fallbackToEml();
            return false;
        }

        try {
            const byteCharacters = atob(base64Data);
            const byteNumbers = new Array(byteCharacters.length);
            for (let i = 0; i < byteCharacters.length; i++) {
                byteNumbers[i] = byteCharacters.charCodeAt(i);
            }
            const byteArray = new Uint8Array(byteNumbers);
            const blob = new Blob([byteArray], {type: 'image/png'});
            const file = new File([blob], filename, { type: 'image/png' });

            if (navigator.canShare({ files: [file] })) {
                await navigator.share({
                    files: [file],
                    title: subject,
                    text: body
                });
                return true;
            } else {
                fallbackToEml();
                return false;
            }
        } catch (error) {
            console.error('Error sharing:', error);
            // If the user cancelled the share, error.name is usually 'AbortError'.
            if (error.name !== 'AbortError') {
                fallbackToEml();
            }
            return false;
        }
    }
};
