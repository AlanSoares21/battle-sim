import { ICanvasWrapper } from "../../../CanvasWrapper";

export interface IRender {
    canvas: ICanvasWrapper;
    render: () => void;
}
