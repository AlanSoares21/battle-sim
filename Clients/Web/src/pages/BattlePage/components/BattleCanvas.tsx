import React, { useCallback, useContext, useEffect, useState } from "react";
import { BattleContext, IBattleContext } from "../BattleContext";
import { IBattleData, IEntity, IEntityPosition, ISkillData, TBoardCoordinates, TCanvasCoordinates } from "../../../interfaces";
import CanvasWrapper from "../../../CanvasWrapper";
import BattleRenderController, { IBattleRenderControllerProps, ICreateRenders } from "./BattleRenderController";
import { IServerEvents, ServerConnection } from "../../../server";
import { LifeSphereRender, ManaBarRender } from "./LifeSphereRenderComponents";
import { PlayerRender } from "./BoardRenderComponents";


const keysToMap = [ "q", "w", "e", "r", "a", "s", "d", "f" ];

const unknowedSkill: ISkillData = {
    cost: 5,
    range: 1
}

const commomDamageSkill: ISkillData = {
    cost: 5,
    range: 5
}

const skillInfo: {[skilllname: string]: ISkillData} =  {
    'basicNegativeDamageOnX': commomDamageSkill,
    'basicNegativeDamageOnY': commomDamageSkill,
    'basicPositiveDamageOnX': commomDamageSkill,
    'basicPositiveDamageOnY': commomDamageSkill
}

function getSkillInfo(skill: string): ISkillData {
    if (skill in skillInfo)
        return skillInfo[skill];
    return unknowedSkill;
}

function getSkillsBindingsToKeyboard(skills: string[]): { [skill: string]: string } {
    return skills.reduce<{ [skill: string]: string }>((keyBindings, skill, i) => {
        if (i < keysToMap.length)
            keyBindings[skill] = keysToMap[i];
        return keyBindings;
    }, {});
}

function getKeybordBindingsToSkills(skills: string[]): { [key: string]: string } {
    return keysToMap.reduce<{ [key: string]: string }>((keyBindings, key, i) => {
        if (i < skills.length)
            keyBindings[key] = skills[i];
        return keyBindings;
    }, {});
}

type TCanvasOffset = {left: number, top: number};

function getCanvasOffset(canvasRef: HTMLCanvasElement): TCanvasOffset {
    return { 
        left: canvasRef.offsetLeft + canvasRef.clientLeft, 
        top: canvasRef.offsetTop + canvasRef.clientTop 
    }
}

const createRenders: ICreateRenders = {
    lifeSphere: p => new LifeSphereRender(p),
    manaBar: p => new ManaBarRender(p),
    playerRender: p => new PlayerRender(p)
}

interface IPlayerState extends IEntity {
    mana: number;
}

type TEntitiesDataDictionary = {[id: IEntity['id']]: IEntity};

interface IBattleState {
    currentPlayer: IPlayerState;
    positions: IBattleData['board']['entitiesPosition'];
    entities: TEntitiesDataDictionary;
}

function transformInEntitiesDictionary(
    dictionary: TEntitiesDataDictionary, 
    value: IEntity) {
    dictionary[value.id] = value;
    return dictionary;
}

export class CanvasController {
    private canvasOffset: TCanvasOffset;
    private battleRender: BattleRenderController;
    private mappedKeyBoard;
    private server: ServerConnection;
    state: IBattleState;
    skillSelected?: string;

    constructor(
        canvasRef: HTMLCanvasElement, 
        state: IBattleContext,
        createRender: (props:IBattleRenderControllerProps) => BattleRenderController
    ) {
        this.state = {
            positions: state.battle.board.entitiesPosition,
            entities: state.battle.entities.reduce(transformInEntitiesDictionary, {}),
            currentPlayer: {...state.player, mana: 0}
        };
        this.server = state.server; 
        this.canvasOffset = getCanvasOffset(canvasRef);

        canvasRef.height = canvasRef.clientHeight;
        canvasRef.width = canvasRef.clientWidth;

        const canvasContext = canvasRef.getContext('2d');
        if (canvasContext === null)
            throw new Error(`Canvas context is null`);
    
        this.battleRender = createRender({
            board: state.battle.board.size,
            assetsData: state.assets,
            canvas: new CanvasWrapper(canvasContext),
            createRenders,
            player: state.player,
            skillKeyBindings: getSkillsBindingsToKeyboard(state.player.skills)
        })
        
        this.mappedKeyBoard = getKeybordBindingsToSkills(state.player.skills);
        canvasRef.onclick = this.handleOnClick();
        this.setServerEventsListenners();
        this.setBattleInitialState();
    }

    private setServerEventsListenners() {
        this.server
            .onEntitiesMove(this.handleEntitiesMove())
            .onSkill(this.handleSkill())
            .onManaRecovered(this.handleManaRecovered());
    }

    private handleEntitiesMove(): IServerEvents['EntitiesMove'] {
        return moves => {
            for (const key in moves) {
                const positionIndex = this.state.positions
                    .findIndex(p => p.entityIdentifier === key);
                if (positionIndex === -1)
                    continue;
                this.state.positions[positionIndex].x = moves[key].x;
                this.state.positions[positionIndex].y = moves[key].y;

                if (this.state.entities[key] === undefined) 
                    continue;
                this.battleRender.setPlayer(
                    this.state.entities[key], 
                    moves[key], 
                    this.state.currentPlayer.id === key
                );
            }
        }
    }

    private handleSkill(): IServerEvents['Skill'] {
        return (skillName, source, target, currentHealth) => {
            if (source === this.state.currentPlayer.id)
                this.increseManaIn(- getSkillInfo(skillName).cost);
            this.battleRender.updateEntityCurrentHealth(
                target === this.state.currentPlayer.id,
                currentHealth
            );
        }
    }

    private handleManaRecovered(): IServerEvents['ManaRecovered'] {
        return () => {
            this.increseManaIn(5);
        }
    }

    private increseManaIn(value: number) {
        if (this.state.currentPlayer.mana >= this.state.currentPlayer.maxMana && value > 0)
            return;
        this.state.currentPlayer.mana += value;
        this.battleRender.updateMana(this.state.currentPlayer.mana);
    }

    private setBattleInitialState() {
        for (const position of this.state.positions) {
            if (this.state.entities[position.entityIdentifier] === undefined) 
                continue;
            this.battleRender.setPlayer(
                this.state.entities[position.entityIdentifier],
                {x: position.x, y: position.y},
                position.entityIdentifier === this.state.currentPlayer.id
            );
        }
    }

    private handleOnClick() {
        return (ev: MouseEvent) => {
            const canvasClick: TCanvasCoordinates = { 
                x: ev.clientX - this.canvasOffset.left,
                y: ev.clientY - this.canvasOffset.top
            };
            const boardClick = this.battleRender.clickOnBoard(canvasClick);
            if (boardClick)
                this.handleBoardClick(boardClick)
            else {
                const skill = this.battleRender.clickOnSkill(canvasClick);
                if (skill !== undefined) {
                    if (skill === this.skillSelected)
                        this.unselectSkill();
                    else
                        this.selectSkill(skill);
                }
            }
        }
    }

    private handleBoardClick(click: TBoardCoordinates) {
        this.battleRender.pointer.setPosition(click);
        if (this.skillSelected !== undefined) {
            const index = this.state.positions
                .findIndex(e => e.x === click.x && e.y === click.y)
            if (index > -1) {
                this.useSkillIn(
                    this.state.positions[index].entityIdentifier, 
                    this.skillSelected
                );
                return;
            }
        }
        this.server.Move(click.x, click.y);
    }

    private useSkillIn(entityId: string, skill: string) {
        const info = getSkillInfo(skill);
        if (info.cost <= this.state.currentPlayer.mana 
            && info.range >= this.distancePlayerTo(entityId))
            this.server.Skill(
                skill, 
                entityId
            );
        this.unselectSkill();
    }

    private distancePlayerTo(target: string): number {
        let targetPosition: IEntityPosition | undefined;
        let playerPosition: IEntityPosition | undefined;
        for (let index = 0; index < this.state.positions.length; index++) {
            if (this.state.positions[index].entityIdentifier !== target)
                targetPosition = this.state.positions[index];
            if (this.state.positions[index].entityIdentifier !== 
                this.state.currentPlayer.id)
                playerPosition = this.state.positions[index];
            if (playerPosition !== undefined && targetPosition !== undefined)
                break;
        }
        
        if (targetPosition === undefined)
            throw new Error(`${target} not found in state.positions array`);
        if (playerPosition === undefined)
            throw new Error(`Current player not found in state.positions array`);

        return Math.sqrt(
            Math.pow(playerPosition.x - targetPosition.x, 2)
            +
            Math.pow(playerPosition.y - targetPosition.y, 2)
        );
    }

    private selectSkill(skillName: string) {
        this.skillSelected = skillName;
        this.battleRender.skillBarController.selectSkill(skillName);
    }

    private unselectSkill() {
        this.skillSelected = undefined;
        this.battleRender.skillBarController.unSelectSkill();
    }

    handleKey(key: string) {
        if (key in this.mappedKeyBoard) {
            const skill = this.mappedKeyBoard[key];
            this.selectSkill(skill);
        }
    }

    startRenderLoop() {
        return setInterval(() => {this.battleRender.render()}, 500);
    }
}

export const BattleCanvas: React.FC = () => {
    const context = useContext(BattleContext);

    const [canvasController, setCanvasController] = useState<CanvasController>();

    const setCanvasRef = useCallback((canvasRef: HTMLCanvasElement | null) => {
        if (!canvasRef) {
            console.error('canvas reference is null');
            return;
        }

       setCanvasController(new CanvasController(
            canvasRef, 
            context, 
            p => new BattleRenderController(p)
        ));
    }, [setCanvasController, context]);

    /**
     * sets onkeydown event handler
     * Starts and clear the render loop
     */
    useEffect(() => {
        if (canvasController) {
            document.onkeydown = ev => canvasController.handleKey(ev.key);
            console.log("render loop start");
            const renderLoopInterval = canvasController.startRenderLoop();
            return () => {
                clearInterval(renderLoopInterval);
                console.log("render loop finished");
            };
        }
    }, [canvasController]);
    
    return(<canvas
        ref={setCanvasRef}
        style={{background: "#000000", width: '99%'}}>
    </canvas>);
}
