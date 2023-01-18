import { TBoardCoordinates, TCanvasCoordinates } from "./interfaces";

function removerResto(value: number, dividento: number): number {
    return value - (value % dividento);
}

export function convertBoardToCanvasCoordinates(
    coordinates: TBoardCoordinates, 
    canvas: { width: number, height: number },
    board: { width: number, height: number, cellSize: number }
): TCanvasCoordinates {
    const diffFromCenter = board.cellSize / 2;
    return {
        x: (coordinates.x * (canvas.width / board.width)) + diffFromCenter, 
        y: (coordinates.y * (canvas.height / board.height)) + diffFromCenter
    };
}

export function convertCanvasToBoardCoordinates(
    coordinates: TCanvasCoordinates,
    boardCellSize: number
): TBoardCoordinates {
    return {
        x: removerResto(coordinates.x, boardCellSize) / boardCellSize, 
        y: removerResto(coordinates.y, boardCellSize) / boardCellSize
    };
}
