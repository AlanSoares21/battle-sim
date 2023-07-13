import CanvasWrapper from "../../../CanvasWrapper";
import { TCanvasCoordinates, TCoordinates } from "../../../interfaces";

function sumCoordinate(coordinate: TCanvasCoordinates, value: number): TCanvasCoordinates {
    return { 
        x: coordinate.x + value,
        y: coordinate.y + value
    }
}

const colors = {
    'sphere-background': '#FF0000',
    'name-text': "#FFFFFF",
    'life-coord': "#FFFFFF"
}

class LifeSphereRender {
    canvas: CanvasWrapper;
    lifeCenter: TCanvasCoordinates;
    healthRadiusInScale: number;

    constructor(
        canvas: CanvasWrapper, 
        startDraw: TCanvasCoordinates,
        healthRadiusInScale: number
    ) {
        this.canvas = canvas;
        this.healthRadiusInScale = healthRadiusInScale;
        this.lifeCenter = sumCoordinate(startDraw, healthRadiusInScale);
    }

    render() {
        this.canvas.drawCircle(
            this.lifeCenter, 
            this.healthRadiusInScale,
            colors['sphere-background']
        );
    }
}

class NameRender {
    canvas: CanvasWrapper;
    startDraw: TCanvasCoordinates;
    name: string;

    constructor(
        canvas: CanvasWrapper, 
        startDraw: TCanvasCoordinates,
        name: string
    ) {
        this.canvas = canvas;
        this.startDraw = startDraw;
        this.name = name;
    }

    render() {
        this.canvas.writeText(
            this.startDraw, 
            this.name, 
            colors['name-text']
        );
    }
}

class LifeCoordRender {
    canvas: CanvasWrapper;
    center: TCanvasCoordinates;
    currentLife: TCanvasCoordinates;
    scale: number;

    constructor(
        canvas: CanvasWrapper, 
        startDraw: TCanvasCoordinates,
        scale: number,
        healthRadiusInScale: number,
    ) {
        this.canvas = canvas;
        this.scale = scale;
        this.center = sumCoordinate(startDraw, healthRadiusInScale);
        this.currentLife = sumCoordinate(startDraw, healthRadiusInScale);
    }

    setLife(life: TCoordinates) {
        this.currentLife = {
            x: this.center.x + (life.x * this.scale),
            y: this.center.y + (life.y * this.scale)
        }
    }

    render() {
        this.canvas.drawCircle(
            this.currentLife, 
            5, 
            colors['name-text']
        );
    }
}

export {
    LifeSphereRender,
    NameRender,
    LifeCoordRender
};