import { sumCoordinates } from "./CoordinatesUtils";
import { IAsset, TCanvasCoordinates, TCanvasSize, TCoordinates, TSize } from "./interfaces";

export interface ICanvasWrapper {
    canvasWidth: () => number;

    canvasHeight: () => number;

    getSize: () => TCanvasSize;

    drawRect: (
        color: CanvasFillStrokeStyles['fillStyle'], 
        start: TCanvasCoordinates, 
        size: TSize    
    ) => void;

    drawEmptyRect: (
        color: CanvasFillStrokeStyles['fillStyle'], 
        start: TCanvasCoordinates, 
        size: TSize
    ) => void;
    
    drawLine: (
        start: TCanvasCoordinates, 
        end: TCanvasCoordinates, 
        lineColor: CanvasFillStrokeStyles['strokeStyle']
    ) => void;

    drawLinesAndFill: (
        points: TCanvasCoordinates[],
        fillColor: CanvasFillStrokeStyles['fillStyle']
    ) => void;

    drawEmptyCircle: (
        center: TCanvasCoordinates, 
        radius: number, 
        borderColor: CanvasFillStrokeStyles['strokeStyle']
    ) => void;

    drawCircle: (
        center: TCanvasCoordinates, 
        radius: number, 
        color: CanvasFillStrokeStyles['fillStyle']
    ) => void;

    drawEmptyElipse: (
        center: TCanvasCoordinates, 
        radius: TCoordinates, 
        rotation: number,
        borderColor: CanvasFillStrokeStyles['strokeStyle']
    ) => void;

    writeText: (
        coordinates: TCanvasCoordinates, 
        value: string, 
        color: CanvasFillStrokeStyles['fillStyle']
    ) => void;

    drawImage: (
        image: IAsset, 
        destination: {
            startAt: TCoordinates,
            height: number,
            width: number
        }
    ) => void;

    drawPattern: (
        image: ImageBitmap, 
        destination: {
            startAt: TCoordinates,
            height: number,
            width: number
        }
    ) => void;
}

export default class CanvasWrapper implements ICanvasWrapper {
    private context: CanvasRenderingContext2D;
    
    constructor(context: CanvasRenderingContext2D) {
        this.context = context;
    }
    
    drawLinesAndFill(
        points: TCoordinates[], 
        fillColor: string | CanvasGradient | CanvasPattern
    ) {
        this.context.beginPath();
        this.context.fillStyle = fillColor;
        this.context.moveTo(points[0].x, points[0].y);
        for (let index = 0; index < points.length; index++) {
            const current = points[index];
            this.context.lineTo(current.x, current.y);
        }
        this.context.lineTo(points[0].x, points[0].y);
        this.context.fill();
    };

    drawEmptyElipse(
        center: TCoordinates, 
        radius: TCoordinates, 
        rotation: number, 
        borderColor: string | CanvasGradient | CanvasPattern
    ) {
        this.context.beginPath();
        this.context.strokeStyle = borderColor;
        this.context.ellipse(
            center.x, 
            center.y, 
            radius.x,
            radius.y,
            rotation,
            0,
            Math.PI * 2
        );
        this.context.stroke();
    }

    canvasWidth() {
        return this.context.canvas.width;
    }

    canvasHeight() {
        return this.context.canvas.height;
    }

    getSize(): TCanvasSize {
        return {
            width: this.canvasWidth(),
            height: this.canvasHeight(),
        };
    }

    drawRect(
        color: CanvasFillStrokeStyles['fillStyle'], 
        start: TCanvasCoordinates, 
        size: TSize
    ) {
        this.context.fillStyle = color;
        this.context.fillRect(start.x, start.y, size.width, size.height);
    }

    drawEmptyRect(
        color: CanvasFillStrokeStyles['fillStyle'], 
        start: TCanvasCoordinates, 
        size: TSize
    ) {
        this.context.strokeStyle = color;
        this.context.rect(start.x, start.y, size.width, size.height);
        this.context.stroke();
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

    drawImage(
        image: IAsset, 
        destination: {
            startAt: TCoordinates,
            height: number,
            width: number
        }
    ) {
        this.context.drawImage(
            image.image,
            image.start.x,
            image.start.y,
            image.size.width,
            image.size.height,
            destination.startAt.x,
            destination.startAt.y,
            destination.width,
            destination.height
        );
    }

    drawPattern(
        image: ImageBitmap, 
        destination: {
            startAt: TCoordinates,
            height: number,
            width: number
        }
    ) {
        const pattern = this.context.createPattern(image, "repeat");
        if (pattern !== null)
            this.context.fillStyle = pattern;
        this.context.fillRect(
            destination.startAt.x, 
            destination.startAt.y, 
            destination.width, 
            destination.height
        );
    }

    writeText(coordinates: TCanvasCoordinates, value: string, color: CanvasFillStrokeStyles['fillStyle']) {
        this.context.fillStyle = color;
        this.context.fillText(value, coordinates.x, coordinates.y);
    }
}

class CanvasWarapperDecorator implements ICanvasWrapper {
    private canvasWarapper: CanvasWrapper;

    constructor(canvasWarapper: CanvasWrapper) {
        this.canvasWarapper = canvasWarapper;
    }
    drawImage(
        image: IAsset, 
        destination: { startAt: TCoordinates; height: number; width: number; }
    ) {
        this.canvasWarapper.drawImage(image, destination);
    }

    drawPattern (
        image: ImageBitmap, 
        destination: { startAt: TCoordinates; height: number; width: number; }
    ) {
        this.canvasWarapper.drawPattern(image, destination);
    }
    
    drawLinesAndFill(
        points: TCoordinates[], 
        fillColor: string | CanvasGradient | CanvasPattern
    ) {
        this.canvasWarapper.drawLinesAndFill(points, fillColor);
    }

    drawEmptyElipse(center: TCoordinates, radius: TCoordinates, rotation: number, borderColor: string | CanvasGradient | CanvasPattern) {
        this.canvasWarapper.drawEmptyElipse(center, radius, rotation, borderColor);
    }
    
    canvasWidth() {
        return this.canvasWarapper.canvasWidth();
    }
    
    canvasHeight() {
        return this.canvasWarapper.canvasHeight();
    }

    getSize() {
        return this.canvasWarapper.getSize();
    }
    
    drawRect(
        color: string | CanvasGradient | CanvasPattern, 
        start: TCoordinates, 
        size: TSize
    ) {
        this.canvasWarapper.drawRect( color, start, size);
    }

    drawEmptyRect(
        color: string | CanvasGradient | CanvasPattern, 
        start: TCoordinates, 
        size: TSize
    ) {
        this.canvasWarapper.drawEmptyRect(color, start, size);
    }
    
    drawLine(
        start: TCoordinates, 
        end: TCoordinates, 
        lineColor: string | CanvasGradient | CanvasPattern
    ) {
        this.canvasWarapper.drawLine(
            start,
            end,
            lineColor
        );
    }

    drawEmptyCircle(
        center: TCoordinates, 
        radius: number, 
        borderColor: string | CanvasGradient | CanvasPattern
    ) {
        this.canvasWarapper.drawEmptyCircle(
            center,
            radius,
            borderColor
        );
    }

    drawCircle(
        center: TCoordinates, 
        radius: number, 
        color: string | CanvasGradient | CanvasPattern
    ) {
        this.canvasWarapper.drawCircle(
            center,
            radius,
            color
        );
    }

    writeText(
        coordinates: TCoordinates, 
        value: string, 
        color: string | CanvasGradient | CanvasPattern
    ) {
        this.canvasWarapper.writeText(
            coordinates,
            value,
            color
        );
    }

}

export class SubAreaOnCanvasDecorator extends CanvasWarapperDecorator {
    private newOrigin: TCanvasCoordinates;
    private newArea: TCanvasSize;
    
    constructor(
        canvasWarapper: CanvasWrapper, 
        newOrigin: TCanvasCoordinates,
        newArea: TCanvasSize
    ) {
        super(canvasWarapper);
        this.newOrigin = newOrigin;
        this.newArea = newArea;
    }

    drawImage(
        image: IAsset, 
        destination: { startAt: TCoordinates; height: number; width: number; }
    ): void {
        destination.startAt = sumCoordinates(destination.startAt, this.newOrigin);
        super.drawImage(image, destination);
    }

    drawPattern (
        image: ImageBitmap, 
        destination: { startAt: TCoordinates; height: number; width: number; }
    ) {
        destination.startAt = sumCoordinates(destination.startAt, this.newOrigin);
        super.drawPattern(image, destination);
    }

    drawEmptyElipse(
        center: TCoordinates, 
        radius: TCoordinates, 
        rotation: number, 
        borderColor: string | CanvasGradient | CanvasPattern
    ): void {
        super.drawEmptyElipse(
            sumCoordinates(center, this.newOrigin),
            radius,
            rotation,
            borderColor
        );
    }

    drawLinesAndFill(
        points: TCoordinates[], 
        fillColor: string | CanvasGradient | CanvasPattern
    ): void {
        super.drawLinesAndFill(
            points.map(p => sumCoordinates(p, this.newOrigin)),
            fillColor
        );    
    }

    canvasWidth(): number {
        return this.newArea.width
    }

    canvasHeight(): number {
        return this.newArea.height
    }

    getSize(): TSize {
        return this.newArea;
    }

    drawRect(
        color: string | CanvasGradient | CanvasPattern, 
        start: TCoordinates, 
        size: TSize
    ) {
        super.drawRect(
            color,
            sumCoordinates(start, this.newOrigin),
            size
        );
    }

    drawEmptyRect(
        color: string | CanvasGradient | CanvasPattern, 
        start: TCoordinates, 
        size: TSize
    ): void {
        super.drawEmptyRect(
            color,
            sumCoordinates(start, this.newOrigin),
            size
        ); 
    }

    drawLine(
        start: TCoordinates, 
        end: TCoordinates, 
        lineColor: string | CanvasGradient | CanvasPattern
    ) {
        super.drawLine(
            sumCoordinates(start, this.newOrigin),
            sumCoordinates(end, this.newOrigin),
            lineColor
        );
    }

    drawCircle(
        center: TCoordinates, 
        radius: number, 
        color: string | CanvasGradient | CanvasPattern
    ) {
        super.drawCircle(
            sumCoordinates(center, this.newOrigin),
            radius,
            color
        );
    }

    writeText(
        coordinates: TCoordinates, 
        value: string, 
        color: string | CanvasGradient | CanvasPattern
    ) {
        super.writeText(
            sumCoordinates(coordinates, this.newOrigin),
            value,
            color
        );
    }
}