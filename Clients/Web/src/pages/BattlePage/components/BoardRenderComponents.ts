import CanvasWrapper from "../../../CanvasWrapper";
import {
    TBoard, 
    TBoardCoordinates, 
    TCanvasCoordinates 
} from "../../../interfaces";

function removerResto(value: number, dividento: number): number {
    return value - (value % dividento);
}

function boardToCanvasCoordinates(
    coordinates: TBoardCoordinates, 
    cellSize: number,
    board: TBoard,
    canvasWidth: number,
    canvasHeight: number
): TCanvasCoordinates {
    const diffFromCenter = cellSize / 2;
    return {
        x: (coordinates.x * (canvasWidth / board.width)) + diffFromCenter, 
        y: (coordinates.y * (canvasHeight / board.height)) + diffFromCenter
    };
}

export function canvasToBoardCoordinates(coordinates: TCanvasCoordinates, cellSize: number): TBoardCoordinates {
    return {
        x: removerResto(coordinates.x, cellSize) / cellSize, 
        y: removerResto(coordinates.y, cellSize) / cellSize
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

export class PlayerRender {
    canvas: CanvasWrapper;
    name: string;
    cellSize: number;
    board: TBoard;
    
    canvasSize: TBoard;
    circleRadius: number;
    circleCenter: TCanvasCoordinates;
    textStart: TCanvasCoordinates;

    constructor(
        canvas: CanvasWrapper,
        cellSize: number,
        board: TBoard,
        name: string,
        current: TBoardCoordinates
    ) {
        this.canvas = canvas;
        this.name = name;
        this.cellSize = cellSize;
        this.board = board;

        this.canvasSize = canvas.getSize();
        this.circleRadius = cellSize / 2;
        this.circleCenter = boardToCanvasCoordinates(
            current,
            this.cellSize,
            this.board,
            this.canvasSize.width,
            this.canvasSize.height
        );
        this.textStart = {
            x: this.circleCenter.x - this.cellSize / 4,
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
        this.textStart = {
            x: this.circleCenter.x - this.cellSize / 4,
            y: this.circleCenter.y
        };
    }

    private drawPlayerCircle() {
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

export class PointerRender {
    canvas: CanvasWrapper;
    board: TBoard;
    cellSize: number;
    
    radius: number;
    canvasSize: TBoard;
    postition: TCanvasCoordinates;

    constructor(
        canvas: CanvasWrapper,
        board: TBoard,
        cellSize: number
    ) {
        this.canvas = canvas;
        this.board = board;
        this.cellSize = cellSize;
        this.canvasSize = canvas.getSize();
        this.radius = cellSize / 2;
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
        this.canvas.drawEmptyCircle(this.postition, this.radius, colors['pointer']);
    }
}

export class BackgroundRender {
    canvas: CanvasWrapper;
    board: TBoard;
    cellSize: number;

    canvasSize: TBoard;
    
    constructor (
        canvas: CanvasWrapper,
        board: TBoard,
        cellSize: number
    ) {
        this.canvas = canvas;
        this.board = board;
        this.cellSize = cellSize;
        this.canvasSize = canvas.getSize();
    }

    private fillBackground() {
        const start: TCanvasCoordinates = { x: 0, y: 0 };
        const canvas = this.canvas.getSize();
        const end: TCanvasCoordinates = { x: canvas.width, y: canvas.height };
        this.canvas.drawRect(colors['background'], start, end);
    }

    private drawGrid() {
        this.drawGridVerticalLines();
        this.drawGridHorizontalLines();
    }

    private drawGridVerticalLines() {
        for (let index = 0; index < this.board.width; index++) {
            const x = (index + 1) *  this.cellSize;
            this.canvas.drawLine({ x, y: 0}, { x, y: this.canvasSize.height}, colors['grid']);
        }
    }

    private  drawGridHorizontalLines() {
        for (let index = 0; index < this.board.height; index++) {
            const y = index *  this.cellSize;
            this.canvas.drawLine({ x: 0, y}, { x: this.canvasSize.width, y}, colors['grid']);
        }
    }

    render() {
        this.fillBackground();
        this.drawGrid();
    }

}