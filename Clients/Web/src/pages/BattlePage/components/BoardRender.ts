import CanvasWrapper, { ICanvasWrapper, SubAreaOnCanvasDecorator } from "../../../CanvasWrapper";
import { IEntity, IPlayerRenderData, TBoard, TBoardCoordinates, TCanvasCoordinates, TCanvasSize, TSize } from "../../../interfaces";
import { BackgroundRender, PlayerRender, PointerRender } from "./BoardRenderComponents";
import { LifeCoordRender, LifeSphereRender } from "./LifeSphereRenderComponents";
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
    
    /**
     * Board components
     */
    private boardCanvas: ICanvasWrapper;
    pointer: PointerRender;
    private background: BackgroundRender;
    private playersData: IPlayerRenderData[] = [];
    private renders: PlayerRender[] = [];

    /**
     * Life
     */
    private enemyLifeSphereCanvas: ICanvasWrapper;
    private enemyRenders?: {
        lifeSphere: LifeSphereRender,
        lifePointer: LifeCoordRender
    };
    private userLifeSphereCanvas: ICanvasWrapper;
    private userRenders?: {
        lifeSphere: LifeSphereRender,
        lifePointer: LifeCoordRender
    };

    private board: TBoard;
    private cellSize: TSize;

    constructor(
        canvas: CanvasWrapper,
        board: TBoard
    ) {
        this.board = board;
        
        const canvasSize = canvas.getSize();

        const boardAreaToDraw: TCanvasSize = { 
            height: canvasSize.height * boardWidth, 
            width: canvasSize.width * boardHeigth 
        };
        const boardStartDrawAt: TCanvasCoordinates = { 
            x: canvasSize.width * boardMarginLeft, 
            y: canvasSize.height * boardMarginTop 
        };
        
        this.cellSize = {
            width: boardAreaToDraw.width / board.width,
            height:boardAreaToDraw.height / board.height
        };

        this.boardCanvas = new SubAreaOnCanvasDecorator(
            canvas, 
            boardStartDrawAt, 
            boardAreaToDraw
        );
        
        this.pointer = new PointerRender(this.boardCanvas, board, this.cellSize);
        this.background = new BackgroundRender(this.boardCanvas, board, this.cellSize);
        
        const enemyLifeSphereStartAt: TCanvasCoordinates = {
            x: 0,
            y: 0
        };
        
        const enemyLifeSphereArea: TCanvasSize = {
            height: boardStartDrawAt.x,
            width: boardStartDrawAt.x
        };

        this.enemyLifeSphereCanvas = new SubAreaOnCanvasDecorator(
            canvas, 
            enemyLifeSphereStartAt, 
            enemyLifeSphereArea
        );

        const userLifeSphereStartAt: TCanvasCoordinates = {
            x: boardStartDrawAt.x + boardAreaToDraw.width,
            y: enemyLifeSphereStartAt.y
        };
        
        const userLifeSphereArea: TCanvasSize = {
            height: boardStartDrawAt.x,
            width: boardStartDrawAt.x
        };
        
        this.userLifeSphereCanvas = new SubAreaOnCanvasDecorator(
            canvas, 
            userLifeSphereStartAt, 
            userLifeSphereArea
        );
    }

    setPlayer(data: IEntity, position: TBoardCoordinates, isTheUser: boolean) {
        const index = this.playersData.findIndex(p => p.name === data.id);
        if (index === -1) {
            this.playersData.push({ name: data.id });
            this.renders.push(new PlayerRender(
                this.boardCanvas,
                this.cellSize,
                this.board,
                data.id,
                position
            ));
            
            const maxSphereSize = this.enemyLifeSphereCanvas.getSize().width;
            const lifeSphereScale = Math.abs(maxSphereSize / (data.healthRadius * 2));
            const healthRadiusInScale = data.healthRadius * lifeSphereScale;

            if (isTheUser) {
                this.userRenders = {
                    lifePointer: new LifeCoordRender(
                        this.userLifeSphereCanvas,
                        lifeSphereScale,
                        healthRadiusInScale
                    ),
                    lifeSphere: new LifeSphereRender(
                        this.userLifeSphereCanvas,
                        healthRadiusInScale
                    )
                };
            } else {
                this.enemyRenders = {
                    lifePointer: new LifeCoordRender(
                        this.enemyLifeSphereCanvas,
                        lifeSphereScale,
                        healthRadiusInScale
                    ),
                    lifeSphere: new LifeSphereRender(
                        this.enemyLifeSphereCanvas,
                        healthRadiusInScale
                    )
                };
            }
        }
        else {
            this.playersData[index] = { name: data.id };
            this.renders[index].updatePosition(position);
        }
    }

    render() {
        // Enemy life sphere
        if (this.enemyRenders !== undefined) {
            this.enemyRenders.lifeSphere.render();
            this.enemyRenders.lifePointer.render();
        }
        // User life sphere
        if (this.userRenders !== undefined) {
            this.userRenders.lifeSphere.render();
            this.userRenders.lifePointer.render();
        }
        // Board
        this.background.render();
        for (let index = 0; index < this.renders.length; index++) {
            this.renders[index].render();
        }
        this.pointer.render();
    }
}