import { TSize } from "./interfaces";

export function scaledSize(value: TSize, scale: TSize): TSize {
    return {
        height: Math.round(value.height * scale.height),
        width: Math.round(value.width * scale.width)
    }
}