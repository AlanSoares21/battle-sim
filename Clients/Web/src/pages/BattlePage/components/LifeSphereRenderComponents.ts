import { ICanvasWrapper } from "../../../CanvasWrapper";
import { sumCoordinate } from "../../../CoordinatesUtils";
import { IAsset, TCanvasCoordinates, TCanvasSize, TCoordinates, TSize } from "../../../interfaces";
import { scaledSize } from "../../../utils";
import { IRender } from "./Render";

const colors = {
    'sphere-background': '#FF0000',
    'name-text': "#FFFFFF",
    'life-coord': "#FFFFFF",
    'life-equip': "#004F55",
    'mana-border': '#000000',
    'mana-background': '#589099',
    'mana-text': '#FFFFFF'
}

class EquipRender implements IRender {
    canvas: ICanvasWrapper;
    private coordinates: TCanvasCoordinates[];
    private asset: IAsset;
    private equipPattern?: CanvasPattern;

    constructor(
        canvas: ICanvasWrapper,
        scale: number,
        healthRadiusInScale: number,
        coordinates: TCoordinates[],
        asset: IAsset
    ) {
        this.asset = asset;
        this.canvas = canvas;
        this.coordinates = coordinates.map(c => 
            ({ 
                x: healthRadiusInScale + c.x * scale, 
                y: healthRadiusInScale - c.y * scale
            })
        );
        if (asset.image) {
            const pattern = canvas.createPattern(asset.image);
            if (pattern)
                this.equipPattern = pattern;
        }
    }

    render() {
        this.canvas.drawLinesAndFill(
            this.coordinates,
            colors['life-equip']
        );
        if (this.equipPattern)
            this.canvas.drawLinesAndFill(
                this.coordinates,
                this.equipPattern
            );
    }
}

class LifeSphereRender implements IRender {
    canvas: ICanvasWrapper;
    private canvasSize: TCanvasSize;
    private lifeCenter: TCanvasCoordinates;
    private healthRadiusInScale: number;
    private asset: IAsset;

    constructor(
        canvas: ICanvasWrapper,
        healthRadiusInScale: number,
        asset: IAsset
    ) {
        this.asset = asset;
        this.canvas = canvas;
        this.canvasSize = canvas.getSize();
        this.healthRadiusInScale = healthRadiusInScale;
        this.lifeCenter = {
            x: healthRadiusInScale,
            y: healthRadiusInScale
        };
    }

    render() {
        if (this.asset.image)
            this.canvas.drawAsset(
                this.asset, 
                {
                    startAt: { x: 0, y: 0},
                    ...this.canvasSize
                }
            );
        else
            this.canvas.drawCircle(
                this.lifeCenter, 
                this.healthRadiusInScale,
                colors['sphere-background']
            );
    }
}

class ManaBarRender implements IRender {
    canvas: ICanvasWrapper;
    private barSize: TCanvasSize;
    private wiriteAt: TCoordinates;
    private currentValue: string = '0';

    constructor(
        canvas: ICanvasWrapper
    ) {
        this.canvas = canvas;
        const canvasSize = canvas.getSize();
        this.barSize = {
            height: Math.round(canvasSize.width / 4),
            width: canvasSize.width
        };
        this.wiriteAt = {
            x: Math.round(this.barSize.width / 2 - 2),
            y: Math.round(this.barSize.height * 3 / 5)
        }
    }

    updateCurrentValue(value: number) {
        this.currentValue = value.toString();
        this.render();
    }

    render() {
        this.canvas.drawRect(
            colors['mana-background'], 
            {x: 0, y: 0}, 
            this.barSize
        );
        this.canvas.drawEmptyRect(
            colors['mana-border'], 
            {x: 0, y: 0}, 
            this.barSize
        );
        this.canvas.writeText(
            this.wiriteAt,
            this.currentValue,
            colors['mana-text']
        );
    }
}

class LifeCoordRender {
    private canvas: ICanvasWrapper;
    private center: TCanvasCoordinates;
    private currentLife: TCanvasCoordinates;
    private assetPoint: TCanvasCoordinates;
    private scale: number;
    private asset: IAsset

    constructor(
        canvas: ICanvasWrapper,
        scale: number,
        healthRadiusInScale: number,
        asset: IAsset
    ) {
        this.asset = asset;
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
        this.assetPoint = sumCoordinate(this.currentLife, - this.asset.size.height / 2);
    }

    setLife(life: TCoordinates) {
        this.currentLife = {
            x: this.center.x + (life.x * this.scale),
            y: this.center.y - (life.y * this.scale)
        };
        this.assetPoint = sumCoordinate(this.currentLife, - this.asset.size.height / 2);
    }

    render() {
        if (this.asset.image)
            this.canvas.drawAsset(
                this.asset, 
                {
                    startAt: this.assetPoint,
                    ...this.asset.size
                }
            );
        else
            this.canvas.drawCircle(
                this.currentLife, 
                2, 
                colors['name-text']
            );
    }
}

export {
    LifeSphereRender,
    LifeCoordRender,
    EquipRender,
    ManaBarRender
};