import { ICanvasWrapper, SubAreaOnCanvasDecorator } from "../../../CanvasWrapper";
import { subCoordinates } from "../../../CoordinatesUtils";
import { TGameAssets, IEntity, IPlayerRenderData, TBoard, TBoardCoordinates, TCanvasCoordinates, TCanvasSize, TCoordinates, TSize } from "../../../interfaces";
import { BoardController, IBoardControllerProps, IBoardItemRender } from "./BoardController";
import { IEntityFactoryProps, createEntityFactory } from "./EntityRenders";
import { EquipRender, ILifeSphereRenderProps, IManaBarRenderProps, LifeCoordRender, LifeSphereRender, ManaBarRender } from "./LifeSphereRenderComponents";
import { IPointerRenderFactoryProps, PointerRender, createPointerRender } from "./PointerRender";
import { SkillBarController } from "./SkillBarRenderComponents";

const boardMarginLeft = 0.2;
const boardMarginTop = 0.1;
const boardWidth = 0.5;
const boardHeigth = 0.5;

const skillBarMarginTop = 10;

function getSkillBar(
    canvas: ICanvasWrapper, 
    playerSkills: string[], 
    assetsData: TGameAssets,
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

export interface ICreateRenders {
    lifeSphere(props: ILifeSphereRenderProps): LifeSphereRender;
    manaBar(props: IManaBarRenderProps): ManaBarRender;
}

export interface IControllerFactory {
    boardController(props: IBoardControllerProps): BoardController;
}

export interface IBattleRenderControllerProps {
    canvas: ICanvasWrapper;
    board: TBoard;
    assetsData: TGameAssets;
    player: IEntity;
    skillKeyBindings: { [skillName: string]: string };
    createRenders: ICreateRenders;
    createController: IControllerFactory;
}

export default class BattleRenderController {
    private playersData: IPlayerRenderData[] = [];

    skillBarController: SkillBarController;
    boardController: BoardController;

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

    private assets: TGameAssets;
    private createRender: ICreateRenders;

    constructor({
        canvas,
        board,
        assetsData,
        player,
        skillKeyBindings,
        createRenders,
        createController
    }: IBattleRenderControllerProps) {
        this.createRender = createRenders;
        this.assets = assetsData;
        
        const canvasSize = canvas.getSize();

        const boardAreaToDraw: TCanvasSize = { 
            height: canvasSize.height * boardWidth, 
            width: canvasSize.width * boardHeigth 
        };
        const boardStartDrawAt: TCanvasCoordinates = { 
            x: canvasSize.width * boardMarginLeft, 
            y: canvasSize.height * boardMarginTop 
        };
        
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

        this.boardController = createController.boardController({
            assets: assetsData,
            board,
            canvas: {
                wrapper: canvas,
                boarCanvasSize: boardAreaToDraw,
                startAt: boardStartDrawAt
            },
            playerId: player.id,
            renderFactory: {
                entity: createEntityFactory,
                pointer: createPointerRender
            },
        })
    }

    setPlayer(data: IEntity, position: TBoardCoordinates, isTheUser: boolean) {
        const index = this.playersData.findIndex(p => p.name === data.id);
        if (index === -1) {
            this.playersData.push({ name: data.id });
            this.boardController.addEntity(data, position);
            
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
                    lifeSphere: this.createRender.lifeSphere({
                        canvas: this.userLifeSphereCanvas,
                        healthRadiusInScale: healthRadiusInScale,
                        asset: this.assets['life-sphere']
                    }),
                    equips: data.equips.map(e => new EquipRender(
                        this.userLifeSphereCanvas,
                        lifeSphereScale,
                        healthRadiusInScale,
                        e.coordinates,
                        this.assets['barrier-equip-pattern']
                    )),
                    mana: this.createRender.manaBar({
                        canvas: this.userManaBarCanvas,
                        background: this.assets['mana-bar-background'],
                        border: this.assets['mana-bar-border'],
                        fill: this.assets['mana-bar-fill']
                    })
                };
            } else {
                this.enemyRenders = {
                    lifePointer: new LifeCoordRender(
                        this.enemyLifeSphereCanvas,
                        lifeSphereScale,
                        healthRadiusInScale,
                        this.assets['life-pointer']
                    ),
                    lifeSphere: this.createRender.lifeSphere({
                        canvas: this.enemyLifeSphereCanvas,
                        healthRadiusInScale: healthRadiusInScale,
                        asset: this.assets['life-sphere']
                    }),
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
            // this.playersRenders[index].updatePosition(position);
        }
    }

    updateMana(value: number, maxMana: number) {
        if (this.userRenders !== undefined)
            this.userRenders.mana.updateCurrentValue(value, maxMana);
    }

    clickOnBoard(canvasClick: TCanvasCoordinates): TCoordinates | undefined {
        /*
        if (this.background.canvas.isOnCanvas(canvasClick)) {
            const b = canvasToBoardCoordinates(
                subCoordinates(canvasClick, this.boardStartAt), 
                this.cellSize
            );
            return b;
        }
        */
       return undefined;
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
        this.boardController.render();
    }
}