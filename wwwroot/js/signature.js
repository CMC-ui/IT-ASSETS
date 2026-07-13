window.signaturePad = {
    canvas: null,
    ctx: null,
    drawing: false,

    init: function (canvasId) {
        this.canvas = document.getElementById(canvasId);
        if (!this.canvas) return;
        this.ctx = this.canvas.getContext("2d");
        this.ctx.lineWidth = 2;
        this.ctx.lineCap = "round";
        this.ctx.strokeStyle = "#000";

        this.canvas.addEventListener("mousedown", this.startPosition.bind(this));
        this.canvas.addEventListener("mouseup", this.endPosition.bind(this));
        this.canvas.addEventListener("mousemove", this.draw.bind(this));

        this.canvas.addEventListener("touchstart", (e) => { e.preventDefault(); this.startPosition(e.touches[0]); }, { passive: false });
        this.canvas.addEventListener("touchend", (e) => { e.preventDefault(); this.endPosition(); }, { passive: false });
        this.canvas.addEventListener("touchmove", (e) => { e.preventDefault(); this.draw(e.touches[0]); }, { passive: false });
    },

    startPosition: function (e) {
        this.drawing = true;
        this.draw(e);
    },

    endPosition: function () {
        this.drawing = false;
        this.ctx.beginPath();
    },

    draw: function (e) {
        if (!this.drawing) return;
        
        const rect = this.canvas.getBoundingClientRect();
        const x = e.clientX - rect.left;
        const y = e.clientY - rect.top;

        this.ctx.lineTo(x, y);
        this.ctx.stroke();
        this.ctx.beginPath();
        this.ctx.moveTo(x, y);
    },

    clear: function () {
        if (this.canvas && this.ctx) {
            this.ctx.clearRect(0, 0, this.canvas.width, this.canvas.height);
        }
    },

    getImageData: function () {
        if (!this.canvas) return "";
        return this.canvas.toDataURL("image/png");
    }
};
