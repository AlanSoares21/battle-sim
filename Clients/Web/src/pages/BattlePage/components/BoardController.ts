import { ICanvasWrapper } from "../../../CanvasWrapper";
import { IAsset, IEntity, TAssetsNames, TBoard, TBoardCoordinates, TCanvasCoordinates, TCanvasSize, TCoordinates, TGameAssets } from "../../../interfaces";
import { IEntityFactoryProps, IEntityRender } from "./EntityRenders";
import { IPointerRenderFactoryProps, PointerRender } from "./PointerRender";
import { IRender } from "./Render";

const colors = {
    'player-circle': '#00FF00',
    'player-name': '#00000F',
    'pointer': '#FF0000',
    'background': '#D9D9D9',
    'grid': '#000000'
}

function getBoardPattern(
    canvas: ICanvasWrapper,
    asset?: IAsset
): CanvasFillStrokeStyles['fillStyle'] {
    if (asset) {
        const pattern = canvas.createPattern(asset.image);
        if (pattern !== null)
            return pattern;
    }
    return colors['background'];
}

export interface IBoardItemRender extends IRender {
    setPosition: (canvasCoordinates: TCanvasCoordinates) => void;
    turnTo: (point: TCanvasCoordinates) => void;
}

export interface IBoardControllerProps {
    board: TBoard;
    canvas: {
        wrapper: ICanvasWrapper;
        boarCanvasSize: TCanvasSize;
        startAt: TCanvasCoordinates;
    };
    assets: TGameAssets;
    playerId: IEntity['id'];
    renderFactory: {
        entity: (props: IEntityFactoryProps) => IEntityRender;
        pointer: (props: IPointerRenderFactoryProps) => PointerRender;
    }
}

export class BoardController {
    private board: TBoard;
    private canvas: ICanvasWrapper;
    private assets: TGameAssets;
    private startAt: TCanvasCoordinates;
    private canvasSize: TCanvasSize;
    private playerId: IEntity['id'];
    private renderFactory: IBoardControllerProps['renderFactory'];
    private cellSize: TCanvasSize;

    private entityRenders: {
        [key: IEntity['id']]: IBoardItemRender
    } = { };
    private pointer: PointerRender;

    private backgroundFill: CanvasFillStrokeStyles['fillStyle'];

    constructor(props: IBoardControllerProps) {
        this.assets = props.assets;
        this.board = props.board;
        this.canvas = props.canvas.wrapper;
        this.canvasSize = props.canvas.boarCanvasSize;
        this.startAt = props.canvas.startAt;
        this.playerId = props.playerId;
        this.renderFactory = props.renderFactory;
        
        this.backgroundFill = 
            getBoardPattern(this.canvas, this.assets['board-background']);
        this.cellSize = {
            width: this.canvasSize.width / this.board.width,
            height: this.canvasSize.height / this.board.height
        }
        console.log({cellSize: this.cellSize, startAt: this.startAt, area: this.canvasSize})
        this.pointer = this.renderFactory
            .pointer({canvas: this.canvas, cellSize: this.cellSize});
        this.setPointer({
            x: Math.floor(this.board.width / 2), 
            y: Math.floor(this.board.height / 2)
        });
    }

    clickOnBoard(canvasClick: TCanvasCoordinates): TBoardCoordinates | undefined {
        const click: TCoordinates = {
            x: canvasClick.x - this.startAt.x, 
            y: canvasClick.y - this.startAt.y
        };
        if (
            (click.x < 0 || click.y < 0) ||
            (click.x >= this.canvasSize.height && click.y >= this.canvasSize.width)
        )
            return undefined;
        
        const boardClick: TBoardCoordinates = {
            x: Math.floor(click.x / this.cellSize.width),
            y: Math.floor(click.y / this.cellSize.height)
        };
        this.setPointer(boardClick);
        if (this.entityRenders[this.playerId] !== undefined)
            this.entityRenders[this.playerId].turnTo(canvasClick);
        return boardClick;
    }

    addEntity(entity: IEntity, position: TBoardCoordinates) {
        const cord = this.boardToCanvasCoordinate(position);
        console.log(`add entity ${entity.id} in board`, {position, cord});
        this.entityRenders[entity.id] = this.renderFactory.entity({
            isThePlayer: this.playerId === entity.id,
            cellSize: this.cellSize,
            assets: this.assets,
            canvas: this.canvas,
            start: cord
        });
    }

    updateEntityPosition(id: IEntity['id'], position: TBoardCoordinates) {
        const canvasPosition = this.boardToCanvasCoordinate(position);
        this.entityRenders[id].turnTo(canvasPosition);
        this.entityRenders[id].setPosition(canvasPosition);
    }

    private setPointer(coord: TBoardCoordinates) {
        const canvasCoord = this.boardToCanvasCoordinate(coord);
        this.pointer.setPosition(canvasCoord);
    }

    private boardToCanvasCoordinate(coord: TBoardCoordinates): TCanvasCoordinates {
        return {
            x: (coord.x * this.cellSize.width) + this.startAt.x,
            y: (coord.y * this.cellSize.height) + this.startAt.y
        }
    }

    render() {
        this.fillBackground();
        for (const id in this.entityRenders) {
            this.entityRenders[id].render();
        }
        this.pointer.render();
    }

    private fillBackground() {
        this.canvas.drawRect(this.backgroundFill, this.startAt, this.canvasSize);
    }
}

export function createBoardController(props: IBoardControllerProps): BoardController {
    return new BoardController(props);
}