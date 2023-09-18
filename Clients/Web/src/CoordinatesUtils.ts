import { TCanvasCoordinates, TCoordinates } from "./interfaces";

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

export type TDirection = "LeftUp" 
    | "Up"
    | "RightUp"
    | "Right"
    | "RightDown"
    | "Down"
    | "LeftDown"
    | "Left"
    | "Same";

export function determineDirection(from: TCoordinates, to: TCoordinates): TDirection {
    if (from.x > to.x) {
        if (from.y > to.y)
            return "LeftUp";
        if (from.y < to.y)
            return "LeftDown";
        return "Left";
    }
    if (from.x < to.x) {
        if (from.y > to.y)
            return "RightUp";
        if (from.y < to.y)
            return "RightDown";
        return "Right";
    }
    // from.x === to.x
    if (from.y === to.y)
        return "Same";
    if (from.y > to.y)
        return "Up";
    return "Down";
}
