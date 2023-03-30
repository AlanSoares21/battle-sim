import CanvasWrapper from "./CanvasWrapper";
import { 
    IBoardColors, 
    IPlayerRenderData, 
    TBoard, 
    TBoardCoordinates, 
    TCanvasCoordinates } from "./interfaces";

function removerResto(value: number, dividento: number): number {
    return value - (value % dividento);
}

export default class BoardCanvas {
    private canvas: CanvasWrapper;
    private colors: IBoardColors = {
        background: '#D9D9D9',
        grid: '#000000',
        pointer: '#FF0000',
        player: {
            circle: '#00FF00',
            name: '#00000F'
        }
    }
    private cellSize: number;
    board: TBoard;

    constructor(board: TBoard, canvas: CanvasWrapper, cellSize: number) {
        this.canvas = canvas;
        this.board = board;
        this.cellSize = cellSize;
    }
    
    fillBackground() {
        const start: TCanvasCoordinates = { x: 0, y: 0 };
        const canvas = this.canvas.getSize();
        const end: TCanvasCoordinates = { x: canvas.width, y: canvas.height };
        this.canvas.drawRect(this.colors.background, start, end);
    }

    drawPointerCircle(cell: TBoardCoordinates) {
        if (cell.x > this.board.width || cell.y > this.board.height) 
            return console.error(`cordinates (${cell.x}, ${cell.y}) are out of board.`);
        const radius = this.cellSize / 2;
        const center = this.boardToCanvasCoordinates(cell);
        this.canvas.drawEmptyCircle(center, radius, this.colors.pointer);
    }

    drawPlayer(cell: TBoardCoordinates, data: IPlayerRenderData) {
        this.drawPlayerCircle(cell);
        this.writePlayerName(cell, data.name);
    }

    private drawPlayerCircle(cell: TBoardCoordinates) {
        if (cell.x > this.board.width || cell.y > this.board.height) 
            return console.error(`cordinates (${cell.x}, ${cell.y}) are out of board.`);
        const radius = this.cellSize / 2;
        const center = this.boardToCanvasCoordinates(cell);
        this.canvas.drawCircle(center, radius, this.colors.player.circle);
    }

    private writePlayerName(cell: TBoardCoordinates, name: string) {
        if (cell.x > this.board.width || cell.y > this.board.height) 
            return console.error(`cordinates (${cell.x}, ${cell.y}) are out of board.`);
        const center = this.boardToCanvasCoordinates(cell);
        center.x -= this.cellSize / 4;
        this.canvas.writeText(center, name, this.colors.player.name);
    }

    drawGridVerticalLines() {
        const linesHeight = this.canvas.canvasHeight();
        for (let index = 0; index < this.board.width; index++) {
            const x = (index + 1) *  this.cellSize;
            this.canvas.drawLine({ x, y: 0}, { x, y: linesHeight}, this.colors.grid);
        }
    }

    drawGridHorizontalLines() {
        const linesWidth = this.canvas.canvasWidth();
        for (let index = 0; index < this.board.height; index++) {
            const y = index *  this.cellSize;
            this.canvas.drawLine({ x: 0, y}, { x: linesWidth, y}, this.colors.grid);
        }
    }

    drawGrid() {
        this.drawGridVerticalLines();
        this.drawGridHorizontalLines();
    }

    canvasToBoardCoordinates(coordinates: TCanvasCoordinates): TBoardCoordinates {
        return {
            x: removerResto(coordinates.x, this.cellSize) / this.cellSize, 
            y: removerResto(coordinates.y, this.cellSize) / this.cellSize
        };
    }

    boardToCanvasCoordinates(coordinates: TBoardCoordinates): TCanvasCoordinates {
        const diffFromCenter = this.cellSize / 2;
        const canvas = this.canvas.getSize();
        return {
            x: (coordinates.x * (canvas.width / this.board.width)) + diffFromCenter, 
            y: (coordinates.y * (canvas.height / this.board.height)) + diffFromCenter
        };
    }
}