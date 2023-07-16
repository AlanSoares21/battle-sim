import { TCanvasCoordinates } from "./interfaces";

export function sumCoordinate(
    coordinate: TCanvasCoordinates, 
    value: number
): TCanvasCoordinates {
    return { 
        x: coordinate.x + value,
        y: coordinate.y + value
    }
}

export function sumCoordinates(
    first: TCanvasCoordinates, 
    second: TCanvasCoordinates
): TCanvasCoordinates {
    return { 
        x: first.x + second.x,
        y: first.y + second.y
    }
}

export function subCoordinates(
    first: TCanvasCoordinates, 
    second: TCanvasCoordinates
): TCanvasCoordinates {
    return { 
        x: first.x - second.x,
        y: first.y - second.y
    }
}
