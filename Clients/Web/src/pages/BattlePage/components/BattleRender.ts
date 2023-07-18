import CanvasWrapper, { ICanvasWrapper, SubAreaOnCanvasDecorator } from "../../../CanvasWrapper";
import { subCoordinates } from "../../../CoordinatesUtils";
import { IAssetsFile, IEntity, IPlayerRenderData, TBoard, TBoardCoordinates, TCanvasCoordinates, TCanvasSize, TCoordinates, TSize } from "../../../interfaces";
import { BackgroundRender, PlayerRender, PointerRender, canvasToBoardCoordinates } from "./BoardRenderComponents";
import { EquipRender, LifeCoordRender, LifeSphereRender } from "./LifeSphereRenderComponents";
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

export default class BattleRender implements IRender {
    
    /**
     * Board components
     */
    private boardCanvas: ICanvasWrapper;
    private boardStartAt: TCanvasCoordinates;
    private boardCanvasSize: TCanvasSize;
    pointer: PointerRender;
    private background: BackgroundRender;
    private playersData: IPlayerRenderData[] = [];
    private playersRenders: PlayerRender[] = [];

    /**
     * Life
     */
    private enemyLifeSphereCanvas: ICanvasWrapper;
    private enemyRenders?: {
        lifeSphere: LifeSphereRender,
        lifePointer: LifeCoordRender,
        equips: EquipRender[]
    };
    private userLifeSphereCanvas: ICanvasWrapper;
    private userRenders?: {
        lifeSphere: LifeSphereRender,
        lifePointer: LifeCoordRender,
        equips: EquipRender[]
    };

    private board: TBoard;
    private cellSize: TSize;

    constructor(
        canvas: CanvasWrapper,
        board: TBoard,
        assetsData: {
            map: IAssetsFile,
            file: ImageBitmap
        }
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
        
        this.boardCanvasSize = boardAreaToDraw;
        this.boardStartAt = boardStartDrawAt;
        this.boardCanvas = new SubAreaOnCanvasDecorator(
            canvas, 
            boardStartDrawAt, 
            boardAreaToDraw
        );
        
        this.pointer = new PointerRender(this.boardCanvas, board, this.cellSize);
        this.background = new BackgroundRender(
            this.boardCanvas, 
            board, 
            this.cellSize,
            {
                image: assetsData.file,
                ...assetsData.map['board-background']
            }
        );
        
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
            this.playersRenders.push(new PlayerRender(
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
                    ),
                    equips: data.equips.map(e => new EquipRender(
                        this.userLifeSphereCanvas,
                        lifeSphereScale,
                        healthRadiusInScale,
                        e.coordinates
                    ))
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
                    ),
                    equips: data.equips.map(e => new EquipRender(
                        this.enemyLifeSphereCanvas,
                        lifeSphereScale,
                        healthRadiusInScale,
                        e.coordinates
                    ))
                };
            }
        }
        else {
            this.playersRenders[index].updatePosition(position);
        }
    }

    clickOnBoard(canvasClick: TCanvasCoordinates): TCoordinates | undefined {
        canvasClick = subCoordinates(canvasClick, this.boardStartAt);
        console.log('canvas click', {
            cellSize: this.cellSize,
            boardCanvasSize: this.boardCanvasSize,
            board: this.board,
            baordH: this.board.height * this.cellSize.height
        });
        if (
            this.boardCanvasSize.width >= canvasClick.x &&
            this.boardCanvasSize.height >= canvasClick.y &&
            0 <= canvasClick.x &&
            0 <= canvasClick.y
        ) {
            const b = canvasToBoardCoordinates(canvasClick, this.cellSize);
            console.log('board click', b);
            return b;
        }
    }

    updateEntityCurrentHealth(isTheUser: boolean, health: TCoordinates) {
        if (isTheUser && this.userRenders)
            this.userRenders.lifePointer.setLife(health);
        else if (this.enemyRenders)
            this.enemyRenders.lifePointer.setLife(health);
    }

    render() {
        // Enemy life sphere
        if (this.enemyRenders !== undefined) {
            this.enemyRenders.lifeSphere.render();
            this.enemyRenders.lifePointer.render();
            for (let index = 0; index < this.enemyRenders.equips.length; index++) {
                this.enemyRenders.equips[index].render();
            }
            // this.enemyRenders.equips.forEach(e => e.render());
        }
        // User life sphere
        if (this.userRenders !== undefined) {
            this.userRenders.lifeSphere.render();
            this.userRenders.lifePointer.render();
            this.userRenders.equips.forEach(e => e.render());
        }
        // Board
        this.background.render();
        for (let index = 0; index < this.playersRenders.length; index++) {
            this.playersRenders[index].render();
        }
        this.pointer.render();
    }
}