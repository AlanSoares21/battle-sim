import { TCanvasCoordinates } from "./interfaces";

export default class CanvasWrapper {
    private context: CanvasRenderingContext2D;
    constructor(context: CanvasRenderingContext2D) {
        this.context = context;
    }

    canvasWidth() {
        return this.context.canvas.width;
    }

    canvasHeight() {
        return this.context.canvas.height;
    }

    drawRect(color: CanvasFillStrokeStyles['fillStyle'], start: TCanvasCoordinates, end: TCanvasCoordinates) {
        this.context.fillStyle = color;
        this.context.fillRect(start.x, start.y, end.x, end.y);
    }
    
    drawLine(start: TCanvasCoordinates, end: TCanvasCoordinates, lineColor: CanvasFillStrokeStyles['strokeStyle']) {
        this.context.beginPath();
        this.context.strokeStyle = lineColor;
        this.context.moveTo(start.x, start.y);
        this.context.lineTo(end.x, end.y);
        this.context.stroke();
    }

    drawEmptyCircle(center: TCanvasCoordinates, radius: number, borderColor: CanvasFillStrokeStyles['strokeStyle']) {
        this.context.strokeStyle = borderColor;
        this.context.beginPath();
        this.context.arc(center.x, center.y, radius,  0, Math.PI * 2);
        this.context.stroke();
    }

    drawCircle(center: TCanvasCoordinates, radius: number, color: CanvasFillStrokeStyles['fillStyle']) {
        this.context.fillStyle = color;
        this.context.beginPath();
        this.context.arc(center.x, center.y, radius,  0, Math.PI * 2);
        this.context.fill();
    }

    writeText(coordinates: TCanvasCoordinates, value: string, color: CanvasFillStrokeStyles['fillStyle']) {
        this.context.fillStyle = color;
        this.context.fillText(value, coordinates.x, coordinates.y);
    }
}