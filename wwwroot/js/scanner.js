window.qrScanner = {
    html5Qrcode: null,

    start: function (elementId, dotnetHelper) {
        if (window.isMauiApp || localStorage.getItem('isMauiApp') === 'true' || navigator.userAgent.includes('MAUI-App')) {
            window.dotnetHelper = dotnetHelper;
            window.location.href = "mauiscan://start";
            return;
        }

        if (this.html5Qrcode) {
            this.stop();
        }

        // Delay slightly to ensure DOM is fully rendered in Blazor
        setTimeout(() => {
            const element = document.getElementById(elementId);
            if (!element) {
                console.error("Scanner element not found in DOM");
                return;
            }

            this.html5Qrcode = new Html5Qrcode(elementId);
            
            const config = { 
                fps: 10, 
                qrbox: { width: 300, height: 150 },
                experimentalFeatures: {
                    useBarCodeDetectorIfSupported: true
                }
            };

            const handleSuccess = (decodedText) => {
                dotnetHelper.invokeMethodAsync('OnQrCodeScanned', decodedText);
                this.stop();
            };

            const injectFallbackUI = (errMessage) => {
                const el = document.getElementById(elementId);
                if (el) {
                    el.innerHTML = `
                        <div class="alert alert-danger mb-3 p-2" style="font-size: 0.9rem;">
                            <strong><i class="bi bi-exclamation-triangle"></i> Camera Error</strong><br>
                            Could not start camera. Please ensure permissions are granted and you are on HTTPS.
                            <br><small class="text-muted">${errMessage || 'Unknown error'}</small>
                        </div>
                        <div class="mb-3">
                            <label class="btn btn-primary w-100">
                                <i class="bi bi-image"></i> Scan from Image File
                                <input type="file" id="qr-file-input" accept="image/*" hidden />
                            </label>
                        </div>
                    `;
                    
                    const fileInput = document.getElementById('qr-file-input');
                    if (fileInput) {
                        fileInput.addEventListener('change', (e) => {
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
                }
            };

            // Request camera with basic environment facing mode
            // Avoid overly strict resolution requests that break mobile browsers
            this.html5Qrcode.start(
                { facingMode: "environment" },
                config,
                handleSuccess,
                (errorMessage) => { /* ignore parse errors */ }
            ).catch(err => {
                console.error("Camera failed to start: ", err);
                const errMsg = typeof err === 'string' ? err : (err && err.message ? err.message : 'Permission denied or no camera found');
                injectFallbackUI(errMsg);
            });
        }, 150);
    },

    stop: function () {
        if (this.html5Qrcode) {
            try {
                if (this.html5Qrcode.isScanning) {
                    this.html5Qrcode.stop().then(() => {
                        this.html5Qrcode.clear();
                        this.html5Qrcode = null;
                    }).catch(err => {
                        this.html5Qrcode.clear();
                        this.html5Qrcode = null;
                    });
                } else {
                    this.html5Qrcode.clear();
                    this.html5Qrcode = null;
                }
            } catch(e) {
                try { this.html5Qrcode.clear(); } catch(ex) {}
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
            
            // Crop to the center area where the user is focusing the text
            // This massively reduces background noise that confuses Tesseract
            const cropWidth = video.videoWidth * 0.8;
            const cropHeight = video.videoHeight * 0.3;
            const startX = (video.videoWidth - cropWidth) / 2;
            const startY = (video.videoHeight - cropHeight) / 2;

            canvas.width = cropWidth;
            canvas.height = cropHeight;
            const ctx = canvas.getContext('2d');
            
            // Apply CSS filters to the canvas to binarize and clean the image
            ctx.filter = 'grayscale(100%) contrast(300%) brightness(120%)';
            
            // Draw only the cropped portion from the video onto the canvas
            ctx.drawImage(video, startX, startY, cropWidth, cropHeight, 0, 0, cropWidth, cropHeight);

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
