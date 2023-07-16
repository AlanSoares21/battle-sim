import { ICanvasWrapper } from "../../../CanvasWrapper";
import { TCanvasCoordinates, TCoordinates } from "../../../interfaces";
import { IRender } from "./Render";

const colors = {
    'sphere-background': '#FF0000',
    'name-text': "#FFFFFF",
    'life-coord': "#FFFFFF"
}

class LifeSphereRender implements IRender {
    canvas: ICanvasWrapper;
    lifeCenter: TCanvasCoordinates;
    healthRadiusInScale: number;

    constructor(
        canvas: ICanvasWrapper,
        healthRadiusInScale: number
    ) {
        this.canvas = canvas;
        this.healthRadiusInScale = healthRadiusInScale;
        this.lifeCenter = {
            x: healthRadiusInScale,
            y: healthRadiusInScale
        };
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
    canvas: ICanvasWrapper;
    startDraw: TCanvasCoordinates;
    name: string;

    constructor(
        canvas: ICanvasWrapper, 
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
    canvas: ICanvasWrapper;
    center: TCanvasCoordinates;
    private currentLife: TCanvasCoordinates;
    scale: number;

    constructor(
        canvas: ICanvasWrapper,
        scale: number,
        healthRadiusInScale: number,
    ) {
        this.canvas = canvas;
        this.scale = scale;
        this.center = {
            x: healthRadiusInScale,
            y: healthRadiusInScale
        };
        this.currentLife = {
            x: healthRadiusInScale,
            y: healthRadiusInScale
        };
    }

    setLife(life: TCoordinates) {
        this.currentLife = {
            x: this.center.x + (life.x * this.scale),
            y: this.center.y - (life.y * this.scale)
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