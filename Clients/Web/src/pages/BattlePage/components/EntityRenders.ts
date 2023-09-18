import { ICanvasWrapper, IScale, TCanvasTransformations } from "../../../CanvasWrapper";
import { TDirection, determineDirection } from "../../../CoordinatesUtils";
import { IAsset, TAssetsNames, TCanvasCoordinates, TCanvasSize, TCoordinates, TGameAssets } from "../../../interfaces";
import { IBoardItemRender } from "./BoardController";

const colors = {
    'player-circle': '#00FF00',
    'player-name': '#00000F',
    'pointer': '#FF0000',
    'background': '#D9D9D9',
    'grid': '#000000'
}

export interface IEntityRender extends IBoardItemRender {

}

export class DefaultEntityRender implements IEntityRender {
    canvas: ICanvasWrapper;
    private center: TCanvasCoordinates;
    private radius: number;

    constructor(
        canvas: ICanvasWrapper, 
        start: TCanvasCoordinates,
        private entitySize: TCanvasSize) {
        this.canvas = canvas;
        this.radius = entitySize.width / 2;
        this.center = {
            x: start.x + entitySize.width / 2,
            y: start.y + entitySize.height / 2
        }
    }
    turnTo(point: TCoordinates) {

    }
    setPosition(coordinate: TCoordinates) {
        this.center = {
            x: coordinate.x + this.entitySize.width / 2,
            y: coordinate.y + this.entitySize.height / 2
        }
    }
    render() {
        this.canvas.drawCircle(this.center, this.radius, colors['player-circle']);
    }
}

export interface IEntityAssets {
    normal: IAsset;
    back: IAsset;
    side: IAsset;
    sideUpper: IAsset;
}

export class EntityRender implements IEntityRender {
    canvas: ICanvasWrapper;
    private currentAsset: keyof IEntityAssets = 'normal';
    private assetTransformation?: IScale;
    private startAt: TCanvasCoordinates;

    constructor(
        canvas: ICanvasWrapper, 
        private start: TCanvasCoordinates,
        private cellSize: TCanvasSize,
        private assets: IEntityAssets
        ) {
        this.canvas = canvas;
        this.startAt = {...start};
    }
    setPosition(coordinate: TCoordinates) {
        this.start = coordinate;
        this.startAt = {...coordinate};
        if (this.assetTransformation !== undefined) {
            this.mirrorAsset(
                this.assetTransformation.x < 0,
                this.assetTransformation.y < 0
            )
        }
    }

    turnTo(point: TCoordinates) {
        const direction = determineDirection(this.start, point);
        this.startAt.x = this.start.x;
        this.startAt.y = this.start.y;
        this.assetTransformation = undefined;
        this.chagePostions[direction]();
    }

    private chagePostions: {[key in TDirection]: () => any} = {
        'LeftUp': () => {
            this.mirrorAssetHorizontaly();
            this.currentAsset = 'sideUpper'
        },
        'Up': () => {
            this.currentAsset = 'back'
        },
        'RightUp': () => {
            this.currentAsset = 'sideUpper'
        },
        'Right': () => {
            this.currentAsset = 'side'
        },
        'RightDown': () => {
            this.currentAsset = 'sideUpper'
            this.mirrorAssetVerticaly();
        },
        'Down': () => {
            this.currentAsset = 'normal'
        },
        'Same': () => {
            this.currentAsset = 'normal'
        },
        'LeftDown': () => {
            this.currentAsset = 'sideUpper'
            this.mirrorAssetHorizontalyAndVerticaly();
        },
        'Left': () => {
            this.currentAsset = 'side'
            this.mirrorAssetHorizontaly();
        }
    };

    private mirrorAssetHorizontalyAndVerticaly() {
        this.mirrorAsset(true, true);
    }
    private mirrorAssetVerticaly() {
        this.mirrorAsset(false, true);
    }
    private mirrorAssetHorizontaly() {
        this.mirrorAsset(true, false);
    }

    private mirrorAsset(horizontal: boolean, vertical: boolean) {
        let x = 1, y = 1;
        if (horizontal) {
            x = -1;
            this.startAt.x = -1 * this.start.x;
        }
        if (vertical) {
            y = -1;
            this.startAt.y = -1 * this.start.y;
        }
        this.assetTransformation = {type: 'Scale', x, y};
    }

    render() {
        this.canvas.drawAsset(
            this.assets[this.currentAsset],
            {
                startAt: this.startAt,
                ...this.cellSize
            },
            this.assetTransformation && [this.assetTransformation]
        );
    }
}

export interface IEntityFactoryProps {
    canvas: ICanvasWrapper;
    start: TCanvasCoordinates;
    cellSize: TCanvasSize;
    assets: TGameAssets;
    isThePlayer: boolean;
}

export function createEntityFactory({
    canvas,
    start, 
    cellSize,
    assets,
    isThePlayer
}: IEntityFactoryProps): IEntityRender {
    let normal: IAsset | undefined; 
    let back: IAsset | undefined;
    let side: IAsset | undefined; 
    let sideUpper: IAsset | undefined;

    if (isThePlayer) {
        normal = assets['player'];
        back = assets['player-back'];
        side = assets['player-side'];
        sideUpper = assets['player-side-upper'];
    } else {
        normal = assets['enemy'];
        back = assets['enemy-back'];
        side = assets['enemy-side'];
        sideUpper = assets['enemy-side-upper'];
    }
    if (
        normal !== undefined && 
        back !== undefined && 
        side !== undefined && 
        sideUpper !== undefined)
        return new EntityRender(
            canvas, 
            start, 
            cellSize, 
            {normal, back, side, sideUpper}
        );
    return new DefaultEntityRender(canvas, start, cellSize);
}