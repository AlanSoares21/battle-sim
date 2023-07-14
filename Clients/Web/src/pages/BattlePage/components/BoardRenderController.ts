import { BackgroundRender, PlayerRender, PointerRender } from "./BoardRenderComponents";
import CanvasWrapper from "../../../CanvasWrapper";
import { IPlayerRenderData, TBoard, TBoardCoordinates } from "../../../interfaces";

export default class BoardRenderController {
    canvas: CanvasWrapper;
    /**
     * lista de elementos para rendenizar
     */
    pointer: PointerRender;
    private background: BackgroundRender;
    private players: IPlayerRenderData[] = [];
    private renders: PlayerRender[] = [];
    private board: TBoard;

    private cellSize: number;

    constructor(
        canvas: CanvasWrapper,
        board: TBoard, 
        cellSize: number
    ) {
        this.board = board;
        this.canvas = canvas;
        this.cellSize = cellSize;
        this.pointer = new PointerRender(canvas, board, cellSize);
        this.background = new BackgroundRender(canvas, board, cellSize);
        this.render();
    }

    setPlayer(data: IPlayerRenderData, position: TBoardCoordinates) {
        const index = this.players.findIndex(p => p.name === data.name);
        if (index === -1) {
            this.players.push(data);
            this.renders.push(new PlayerRender(
                this.canvas,
                this.cellSize,
                this.board,
                data.name,
                position
            ));
        }
        else {
            this.players[index] = data;
            this.renders[index].updatePosition(position);
        }
    }

    render() {
        this.background.render();
        for (let index = 0; index < this.renders.length; index++) {
            this.renders[index].render();
        }
        this.pointer.render();
    }
}