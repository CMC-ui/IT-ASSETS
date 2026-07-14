window.qrScanner = {
    html5Qrcode: null,

    start: function (elementId, dotnetHelper) {
        if (this.html5Qrcode) {
            this.stop();
        }

        this.html5Qrcode = new Html5Qrcode(elementId);
        
        // Using a rectangular qrbox makes it much easier to scan 1D barcodes (which are wide).
        const advancedConfig = { 
            fps: 20, 
            qrbox: { width: 300, height: 150 },
            experimentalFeatures: {
                useBarCodeDetectorIfSupported: true
            }
        };

        const basicConfig = { 
            fps: 10, 
            qrbox: { width: 300, height: 150 } 
        };

        const handleSuccess = (decodedText) => {
            dotnetHelper.invokeMethodAsync('OnQrCodeScanned', decodedText);
            this.stop();
        };

        const handleError = (errorMessage) => {
            // parse error, ignore it.
        };

        const injectFallbackUI = (err) => {
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
        };

        // Try advanced high-res hardware-accelerated camera first
        this.html5Qrcode.start(
            { facingMode: "environment", width: { ideal: 1920 }, height: { ideal: 1080 } },
            advancedConfig,
            handleSuccess,
            handleError
        ).catch(err => {
            console.warn("Advanced camera configuration failed, trying basic fallback...", err);
            
            // If it failed due to permissions, don't retry, just show the error
            if (err.name === 'NotAllowedError') {
                injectFallbackUI(err);
                return;
            }

            // Retry with basic configuration
            this.html5Qrcode.start(
                { facingMode: "environment" },
                basicConfig,
                handleSuccess,
                handleError
            ).catch(basicErr => {
                console.error("Basic camera fallback completely failed: ", basicErr);
                injectFallbackUI(basicErr);
            });
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
    },

    scanText: async function (elementId, dotnetHelper) {
        try {
            // Find the active video element used by html5-qrcode
            const video = document.querySelector(`#${elementId} video`);
            if (!video) {
                alert("Camera feed not found. Please ensure the scanner is running.");
                return;
            }

            // Create a hidden canvas to draw the current video frame
            const canvas = document.createElement('canvas');
            canvas.width = video.videoWidth;
            canvas.height = video.videoHeight;
            const ctx = canvas.getContext('2d');
            ctx.drawImage(video, 0, 0, canvas.width, canvas.height);

            // Notify Blazor that OCR has started
            await dotnetHelper.invokeMethodAsync('OnOcrProcessingStarted');

            // Run Tesseract.js on the captured canvas
            const result = await Tesseract.recognize(canvas, 'eng', {
                logger: m => console.log(m)
            });

            const text = result.data.text.trim();
            
            // Clean up text (remove excessive newlines, limit to alphanumerics if needed, etc)
            // For now, we'll just return the raw trimmed text.
            if (text) {
                dotnetHelper.invokeMethodAsync('OnQrCodeScanned', text);
                this.stop();
            } else {
                alert("Could not recognize any text. Please try again.");
                await dotnetHelper.invokeMethodAsync('OnOcrProcessingFinished');
            }
        } catch (error) {
            console.error("OCR Error:", error);
            alert("Error running text scanner. See console for details.");
            await dotnetHelper.invokeMethodAsync('OnOcrProcessingFinished');
        }
    }
};

window.qrShare = {
    shareImage: async function (base64Data, filename, toEmail, subject, body) {
        
        const fallbackToAlert = () => {
            alert("Your browser is blocking the Share menu because you are not on a secure HTTPS connection (Netbird HTTP).\n\nPlease click 'Download Image' first, and then attach it manually to your Email/WhatsApp.");
        };

        if (!navigator.canShare) {
            fallbackToAlert();
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
                fallbackToAlert();
                return false;
            }
        } catch (error) {
            console.error('Error sharing:', error);
            if (error.name !== 'AbortError') {
                fallbackToAlert();
            }
            return false;
        }
    }
};
