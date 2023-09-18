import { ICanvasWrapper } from "../../../CanvasWrapper";
import { TCanvasCoordinates, TCanvasSize, TCoordinates } from "../../../interfaces";
import { IBoardItemRender } from "./BoardController";


const borderColor = "#FF0000";

export class PointerRender implements IBoardItemRender {
    canvas: ICanvasWrapper;
    private radius: TCanvasCoordinates;
    private center: TCanvasCoordinates;
    constructor(
        canvas: ICanvasWrapper,
        cellSize: TCanvasSize
    ) {
        this.canvas = canvas;
        this.radius = {x: cellSize.width / 2, y: cellSize.height / 2};
        this.center = {x: 0, y: 0};
    }
    setPosition(canvasCoordinates: TCoordinates) {
        this.center.x = canvasCoordinates.x + this.radius.x;
        this.center.y = canvasCoordinates.y + this.radius.y;
    }
    turnTo(point: TCoordinates) {}
    render() {
        this.canvas.drawEmptyElipse(this.center, this.radius, 0, borderColor);
    }

}

export interface IPointerRenderFactoryProps {
    canvas: ICanvasWrapper;
    cellSize: TCanvasSize;
}

export function createPointerRender(props: IPointerRenderFactoryProps) {
    return new PointerRender(props.canvas, props.cellSize);
}