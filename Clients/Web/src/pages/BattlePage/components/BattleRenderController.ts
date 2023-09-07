import CanvasWrapper, { ICanvasWrapper, SubAreaOnCanvasDecorator } from "../../../CanvasWrapper";
import { subCoordinates } from "../../../CoordinatesUtils";
import { IAssetsData, IEntity, IPlayerRenderData, TBoard, TBoardCoordinates, TCanvasCoordinates, TCanvasSize, TCoordinates, TSize } from "../../../interfaces";
import { BackgroundRender, PlayerRender, PointerRender, canvasToBoardCoordinates } from "./BoardRenderComponents";
import { EquipRender, LifeCoordRender, LifeSphereRender, ManaBarRender } from "./LifeSphereRenderComponents";
import { SkillBarController } from "./SkillBarRenderComponents";

const boardMarginLeft = 0.2;
const boardMarginTop = 0.1;
const boardWidth = 0.5;
const boardHeigth = 0.5;

const skillBarMarginTop = 10;

function getSkillBar(
    canvas: ICanvasWrapper, 
    playerSkills: string[], 
    assetsData: IAssetsData,
    skillKeyBindings: { [skillName: string]: string }
): SkillBarController {
    const canvasSize = canvas.getSize();
    const startAt: TCanvasCoordinates = {
        x: 0,
        y: canvasSize.height * boardMarginTop 
            + canvasSize.height * boardHeigth 
            + skillBarMarginTop
    };
    const canvasArea: TCanvasSize = {
        width: canvasSize.width,
        height: canvasSize.height - startAt.y
    };
    const skillBarCanvas = new SubAreaOnCanvasDecorator(canvas, startAt, canvasArea);
    return new SkillBarController(
        skillBarCanvas, 
        playerSkills, 
        assetsData, 
        skillKeyBindings
    );
}

export default class BattleRenderController {
    
    /**
     * Board components
     */
    private boardCanvas: ICanvasWrapper;
    private boardStartAt: TCanvasCoordinates;
    
    pointer: PointerRender;
    private background: BackgroundRender;
    private playersData: IPlayerRenderData[] = [];
    private playersRenders: PlayerRender[] = [];

    /**
     * Skill Bar components
     */
    skillBarController: SkillBarController;

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
    private userManaBarCanvas: ICanvasWrapper;
    private userRenders?: {
        lifeSphere: LifeSphereRender,
        lifePointer: LifeCoordRender,
        mana: ManaBarRender,
        equips: EquipRender[]
    };

    private board: TBoard;
    private cellSize: TSize;

    private assets: IAssetsData;

    constructor(
        canvas: CanvasWrapper,
        board: TBoard,
        assetsData: IAssetsData,
        player: IEntity,
        skillKeyBindings: { [skillName: string]: string }
    ) {
        this.assets = assetsData;
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
            assetsData['board-background']
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

        const manaBarStartAt: TCanvasCoordinates = {
            x: userLifeSphereStartAt.x,
            y: userLifeSphereStartAt.y + userLifeSphereArea.height
        }
        const manaBarArea: TCanvasSize = {
            height: 25,
            width: userLifeSphereArea.width
        }

        this.userManaBarCanvas = new SubAreaOnCanvasDecorator(
            canvas, 
            manaBarStartAt, 
            manaBarArea
        );

        this.skillBarController = getSkillBar(
            canvas, 
            player.skills, 
            assetsData,
            skillKeyBindings
        );
        this.skillBarController.render();
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
                position,
                isTheUser ? this.assets['player'] : this.assets['enemy']
            ));
            
            const maxSphereSize = this.enemyLifeSphereCanvas.getSize().width;
            const lifeSphereScale = Math.abs(maxSphereSize / (data.healthRadius * 2));
            const healthRadiusInScale = data.healthRadius * lifeSphereScale;
            
            if (isTheUser) {
                this.userRenders = {
                    lifePointer: new LifeCoordRender(
                        this.userLifeSphereCanvas,
                        lifeSphereScale,
                        healthRadiusInScale,
                        this.assets['life-pointer']
                    ),
                    lifeSphere: new LifeSphereRender(
                        this.userLifeSphereCanvas,
                        healthRadiusInScale,
                        this.assets['life-sphere']
                    ),
                    equips: data.equips.map(e => new EquipRender(
                        this.userLifeSphereCanvas,
                        lifeSphereScale,
                        healthRadiusInScale,
                        e.coordinates,
                        this.assets['barrier-equip-pattern']
                    )),
                    mana: new ManaBarRender(
                        this.userManaBarCanvas
                    )
                };
            } else {
                this.enemyRenders = {
                    lifePointer: new LifeCoordRender(
                        this.enemyLifeSphereCanvas,
                        lifeSphereScale,
                        healthRadiusInScale,
                        this.assets['life-pointer']
                    ),
                    lifeSphere: new LifeSphereRender(
                        this.enemyLifeSphereCanvas,
                        healthRadiusInScale,
                        this.assets['life-sphere']
                    ),
                    equips: data.equips.map(e => new EquipRender(
                        this.enemyLifeSphereCanvas,
                        lifeSphereScale,
                        healthRadiusInScale,
                        e.coordinates,
                        this.assets['barrier-equip-pattern']
                    ))
                };
            }
        }
        else {
            this.playersRenders[index].updatePosition(position);
        }
    }

    clickOnBoard(canvasClick: TCanvasCoordinates): TCoordinates | undefined {
        if (this.background.canvas.isOnCanvas(canvasClick)) {
            const b = canvasToBoardCoordinates(
                subCoordinates(canvasClick, this.boardStartAt), 
                this.cellSize
            );
            return b;
        }
    }

    clickOnSkill(canvasClick: TCanvasCoordinates): string | undefined {
        if (this.skillBarController.canvas.isOnCanvas(canvasClick)) 
            return this.skillBarController.clickOnSkill(canvasClick);
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
            this.enemyRenders.equips.forEach(e => e.render());
            this.enemyRenders.lifePointer.render();
        }
        // User life sphere
        if (this.userRenders !== undefined) {
            this.userRenders.lifeSphere.render();
            this.userRenders.equips.forEach(e => e.render());
            this.userRenders.lifePointer.render();
            this.userRenders.mana.render();
        }
        // Board
        this.background.render();
        for (let index = 0; index < this.playersRenders.length; index++) {
            this.playersRenders[index].render();
        }
        this.pointer.render();
    }
}