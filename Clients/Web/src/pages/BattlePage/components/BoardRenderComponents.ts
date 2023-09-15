import { ICanvasWrapper } from "../../../CanvasWrapper";
import { sumCoordinate } from "../../../CoordinatesUtils";
import {
    IAsset,
    TBoard, 
    TBoardCoordinates, 
    TCanvasCoordinates, 
    TCanvasSize, 
    TCoordinates, 
    TSize
} from "../../../interfaces";
import { IRender } from "./Render";

function removerResto(value: number, dividento: number): number {
    return value - (value % dividento);
}

function boardToCanvasCoordinates(
    coordinates: TBoardCoordinates, 
    cellSize: TSize,
    board: TBoard,
    canvasWidth: number,
    canvasHeight: number
): TCanvasCoordinates {
    const diffFromCenterInX = cellSize.width / 2;
    const diffFromCenterInY = cellSize.height / 2;
    return {
        x: (coordinates.x * (canvasWidth / board.width)) + diffFromCenterInX, 
        y: (coordinates.y * (canvasHeight / board.height)) + diffFromCenterInY
    };
}

export function canvasToBoardCoordinates(
    coordinates: TCanvasCoordinates, 
    cellSize: TSize
): TBoardCoordinates {
    return {
        x: removerResto(coordinates.x, cellSize.width) / cellSize.width, 
        y: removerResto(coordinates.y, cellSize.height) / cellSize.height
    };
}
    

function getBoardMiddleCoordinates(board: TBoard): TBoardCoordinates {
    return {
        x: board.width / 2,
        y: board.height / 2
    }
}

const colors = {
    'player-circle': '#00FF00',
    'player-name': '#00000F',
    'pointer': '#FF0000',
    'background': '#D9D9D9',
    'grid': '#000000'
}

export interface IPlayerRenderProps {
    canvas: ICanvasWrapper;
    cellSize: TSize;
    board: TBoard;
    name: string;
    current: TBoardCoordinates;
    asset?: IAsset;
}

export class PlayerRender implements IRender {
    canvas: ICanvasWrapper;
    private name: string;
    private cellSize: TSize;
    private board: TBoard;
    
    private canvasSize: TBoard;
    private circleRadius: number;
    private circleCenter: TCanvasCoordinates;
    private assetPostion: TCanvasCoordinates;
    private textStart: TCanvasCoordinates;
    private asset?: IAsset;

    constructor({
        asset, 
        canvas, 
        name, 
        cellSize, 
        board, 
        current
    }: IPlayerRenderProps) {
        this.asset = asset;
        this.canvas = canvas;
        this.name = name;
        this.cellSize = cellSize;
        this.board = board;

        this.canvasSize = canvas.getSize();
        this.circleRadius = cellSize.width / 2;
        this.circleCenter = boardToCanvasCoordinates(
            current,
            this.cellSize,
            this.board,
            this.canvasSize.width,
            this.canvasSize.height
        );
        this.assetPostion = sumCoordinate(this.circleCenter, -this.circleRadius);
        this.textStart = {
            x: this.circleCenter.x - this.cellSize.width / 4,
            y: this.circleCenter.y
        };
    }

    updatePosition(current: TBoardCoordinates) {
        this.circleCenter = boardToCanvasCoordinates(
            current,
            this.cellSize,
            this.board,
            this.canvasSize.width,
            this.canvasSize.height
        );
        this.assetPostion = sumCoordinate(this.circleCenter, -this.circleRadius);
        this.textStart = {
            x: this.circleCenter.x - this.cellSize.width / 4,
            y: this.circleCenter.y
        };
    }

    private drawPlayerCircle() {
        if (this.asset)
            this.canvas.drawAsset(this.asset, {
                height: this.asset.size.height,
                width: this.asset.size.width,
                startAt: this.assetPostion
            });
        else
            this.canvas.drawCircle(this.circleCenter, this.circleRadius, colors['player-circle']);
    }

    private writePlayerName() {
        this.canvas.writeText(this.textStart, this.name, colors['player-name']);
    }

    render() {
        this.drawPlayerCircle();
        this.writePlayerName();
    }
}

export class PointerRender implements IRender {
    canvas: ICanvasWrapper;
    private board: TBoard;
    private cellSize: TSize;
    
    private radius: TCoordinates;
    private canvasSize: TBoard;
    
    private postition: TCanvasCoordinates;

    constructor(
        canvas: ICanvasWrapper,
        board: TBoard,
        cellSize: TSize
    ) {
        this.canvas = canvas;
        this.board = board;
        this.cellSize = cellSize;
        this.canvasSize = canvas.getSize();
        this.radius = {
            x: cellSize.width / 2,
            y: cellSize.height / 2
        };
        this.postition = boardToCanvasCoordinates(
            getBoardMiddleCoordinates(this.board), 
            this.cellSize, 
            this.board, 
            this.canvasSize.width,
            this.canvasSize.height
        );
    }

    setPosition(cell: TBoardCoordinates) {
        this.postition = boardToCanvasCoordinates(
            cell, 
            this.cellSize, 
            this.board, 
            this.canvasSize.width,
            this.canvasSize.height
        );
    }

    render() {
        this.canvas.drawEmptyElipse(
            this.postition, 
            this.radius,
            0, 
            colors['pointer']
        );
    }
}

export class BackgroundRender implements IRender {
    canvas: ICanvasWrapper;
    private board: TBoard;
    private cellSize: TSize;

    private canvasSize: TCanvasSize;

    private backgroundPattern?: CanvasPattern;
    
    constructor (
        canvas: ICanvasWrapper,
        board: TBoard,
        cellSize: TSize,
        asset?: IAsset
    ) {
        this.canvas = canvas;
        this.board = board;
        this.cellSize = cellSize;
        this.canvasSize = canvas.getSize();
        if (asset) {
            const pattern = canvas.createPattern(asset.image);
            if (pattern)
                this.backgroundPattern = pattern;
        }
    }

    private fillBackground() {
        const startAt: TCanvasCoordinates = { x: 0, y: 0 };
        if (this.backgroundPattern)
            this.canvas.drawRect(this.backgroundPattern, startAt, this.canvasSize);
        else
            this.canvas.drawRect(colors['background'], startAt, this.canvasSize);
    }

    private drawGrid() {
        this.drawGridVerticalLines();
        this.drawGridHorizontalLines();
    }

    private drawGridVerticalLines() {
        for (let index = 0; index < this.board.width; index++) {
            const x = (index + 1) *  this.cellSize.width;
            this.canvas.drawLine({ x, y: 0}, { x, y: this.canvasSize.height}, colors['grid']);
        }
    }

    private  drawGridHorizontalLines() {
        for (let index = 0; index < this.board.height; index++) {
            const y = index *  this.cellSize.height;
            this.canvas.drawLine({ x: 0, y}, { x: this.canvasSize.width, y}, colors['grid']);
        }
    }

    render() {
        this.fillBackground();
        this.drawGrid();
    }

}