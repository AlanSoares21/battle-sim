import CanvasWrapper, { ICanvasWrapper, SubAreaOnCanvasDecorator } from "../../../CanvasWrapper";
import { IPlayerRenderData, TBoard, TBoardCoordinates, TCanvasCoordinates, TCanvasSize, TSize } from "../../../interfaces";
import { BackgroundRender, PlayerRender, PointerRender } from "./BoardRenderComponents";
import { IRender } from "./Render";

function areaInnerCanvas(canvasSize: TCanvasSize, canvasAreaProportion: number): TCanvasSize {
    const target: TCanvasSize = { height: 0, width:  0 };
    const canvasArea = canvasSize.width * canvasSize.height;
    const targetArea = canvasArea * canvasAreaProportion;
    target.height = Math.sqrt(targetArea * canvasSize.height / canvasSize.width);
    target.width = target.height * canvasSize.width / canvasSize.height;
    return target;
}

const boardMarginLeft = 0.2;
const boardMarginTop = 0.1;
const boardWidth = 0.5;
const boardHeigth = 0.5;

export default class BoardRender implements IRender {
    canvas: ICanvasWrapper;

    areaToDraw: TCanvasSize;

    /**
     * lista de elementos para rendenizar
     */
    pointer: PointerRender;
    private background: BackgroundRender;
    private playersData: IPlayerRenderData[] = [];
    private renders: PlayerRender[] = [];
    private board: TBoard;
    private cellSize: TSize;

    constructor(
        canvas: CanvasWrapper,
        board: TBoard
    ) {
        this.board = board;
        
        const canvasSize = canvas.getSize();
        // this.areaToDraw = areaInnerCanvas(canvasSize, boardArea);
        this.areaToDraw = { height: canvasSize.height * boardWidth, width: canvasSize.width * boardHeigth };
        const startDrawAt = { x: canvasSize.width * boardMarginLeft, y: canvasSize.height * boardMarginTop };
        
        this.cellSize = {
            width: this.areaToDraw.width / board.width,
            height: this.areaToDraw.height / board.height
        };

        console.log("new board render",{
            areaToDraw: this.areaToDraw,
            startDrawAt,
            canvasSize,
            cellSize: this.cellSize
        });

        this.canvas = new SubAreaOnCanvasDecorator(canvas, startDrawAt, this.areaToDraw);
        
        this.pointer = new PointerRender(this.canvas, board, this.cellSize);
        this.background = new BackgroundRender(this.canvas, board, this.cellSize);
    }

    setPlayer(data: IPlayerRenderData, position: TBoardCoordinates) {
        const index = this.playersData.findIndex(p => p.name === data.name);
        if (index === -1) {
            this.playersData.push(data);
            this.renders.push(new PlayerRender(
                this.canvas,
                this.cellSize,
                this.board,
                data.name,
                position
            ));
        }
        else {
            this.playersData[index] = data;
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