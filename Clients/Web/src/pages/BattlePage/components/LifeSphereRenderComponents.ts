import { ICanvasWrapper } from "../../../CanvasWrapper";
import { sumCoordinate } from "../../../CoordinatesUtils";
import { IAsset, TCanvasCoordinates, TCanvasSize, TCoordinates } from "../../../interfaces";
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

export class EquipRender implements IRender {
    canvas: ICanvasWrapper;
    private coordinates: TCanvasCoordinates[];
    private equipPattern?: CanvasPattern;

    constructor(
        canvas: ICanvasWrapper,
        scale: number,
        healthRadiusInScale: number,
        coordinates: TCoordinates[],
        asset?: IAsset
    ) {
        this.canvas = canvas;
        this.coordinates = coordinates.map(c => 
            ({ 
                x: healthRadiusInScale + c.x * scale, 
                y: healthRadiusInScale - c.y * scale
            })
        );
        if (asset) {
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

export interface ILifeSphereRenderProps {
    canvas: ICanvasWrapper;
    healthRadiusInScale: number;
    asset?: IAsset;
}

export class LifeSphereRender implements IRender {
    canvas: ICanvasWrapper;
    private canvasSize: TCanvasSize;
    private lifeCenter: TCanvasCoordinates;
    private healthRadiusInScale: number;
    private asset?: IAsset;

    constructor({
        asset,
        canvas,
        healthRadiusInScale
    }: ILifeSphereRenderProps) {
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
        if (this.asset)
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

export interface IManaBarRenderProps {
    canvas: ICanvasWrapper;
    background?: IAsset;
    border?: IAsset;
    fill?: IAsset;
}

export class ManaBarRender implements IRender {
    canvas: ICanvasWrapper;
    private barSize: TCanvasSize;
    private wiriteAt: TCoordinates;
    private currentValue: string = '0';
    private assets?: {
        background: IAsset;
        border: IAsset;
        fill: IAsset;
    };
    private fillWidth: number = 0;

    constructor({
        canvas,
        background,
        border,
        fill
    }: IManaBarRenderProps) {
        this.canvas = canvas;
        if (background && border && fill)
            this.assets = {background, border, fill};
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

    updateCurrentValue(value: number, max: number) {
        this.currentValue = value.toString();
        if (max !== 0)
            this.fillWidth = this.barSize.width * (value/max);
        this.render();
    }

    render() {
        if (this.assets) {
            this.canvas.drawAsset(
                this.assets.background,
                {
                    startAt: {x: 0, y: 0},
                    ...this.barSize
                }
            );
            this.canvas.drawAsset(
                this.assets.fill,
                {
                    startAt: {x: 0, y: 0},
                    height: this.barSize.height,
                    width: this.fillWidth
                }
            )
            this.canvas.drawAsset(
                this.assets.border,
                {
                    startAt: {x: 0, y: 0},
                    ...this.barSize
                }
            )
        } else {
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
        }
        this.canvas.writeText(
            this.wiriteAt,
            this.currentValue,
            colors['mana-text']
        );
    }
}

export class LifeCoordRender {
    private canvas: ICanvasWrapper;
    private center: TCanvasCoordinates;
    private currentLife: TCanvasCoordinates;
    private assetPoint: TCanvasCoordinates;
    private scale: number;
    private asset?: IAsset
    private assetHeight: number = 0;

    constructor(
        canvas: ICanvasWrapper,
        scale: number,
        healthRadiusInScale: number,
        asset?: IAsset
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
        if (this.asset)
            this.assetHeight = this.asset.size.height;
        this.assetPoint = sumCoordinate(this.currentLife, - this.assetHeight / 2);
    }

    setLife(life: TCoordinates) {
        this.currentLife = {
            x: this.center.x + (life.x * this.scale),
            y: this.center.y - (life.y * this.scale)
        };
        this.assetPoint = sumCoordinate(this.currentLife, - this.assetHeight / 2);
    }

    render() {
        if (this.asset)
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