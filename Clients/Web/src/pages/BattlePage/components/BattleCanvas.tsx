import React, { MouseEventHandler, useCallback, useContext, useEffect, useMemo, useState } from "react";
import { BattleContext, IBattleContext } from "../BattleContext";
import { IAssetsData, IBattleData, IEntity, TBoard, TBoardCoordinates, TCanvasCoordinates, TCoordinates } from "../../../interfaces";
import CanvasWrapper from "../../../CanvasWrapper";
import BattleRenderController, { IBattleRenderControllerProps, ICreateRenders } from "./BattleRenderController";
import { IServerEvents, ServerConnection } from "../../../server";
import { LifeSphereRender, ManaBarRender } from "./LifeSphereRenderComponents";
import { PlayerRender } from "./BoardRenderComponents";

type TEntityManaList = {[key: IEntity['id']]: {
    current: number;
    max: number;
}};

function onManaRecovered(
    manaList: TEntityManaList, 
    updateMana: (list: TEntityManaList) => void
): IServerEvents['ManaRecovered'] {
    const ids: Array<keyof TEntityManaList> = Object.keys(manaList);
    let updated = 0;
    return () => {
        console.log("mana update in battle page f")
        updated = 0;
        for (const id of ids) {
            if (manaList[id].current === manaList[id].max)
                continue;
            manaList[id].current += 5;
            if (manaList[id].current > manaList[id].max)
                manaList[id].current = manaList[id].max;
            updated++;
        }
        console.log(`update ${updated} entities mana`, manaList)
        if (updated > 0)
            updateMana(manaList);
    }
}

const keysToMap = [ "q", "w", "e", "r", "a", "s", "d", "f" ];

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

export class CanvasController {
    private canvasOffset: TCanvasOffset;
    private battleRender: BattleRenderController;
    private mappedKeyBoard;
    private server: ServerConnection;
    private data: IBattleData;
    private currentPlayer: IEntity;
    skillSelected?: string;

    constructor(
        canvasRef: HTMLCanvasElement, 
        state: IBattleContext,
        createRender: (props:IBattleRenderControllerProps) => BattleRenderController
    ) {
        this.data = state.battle;
        this.server = state.server; 
        this.canvasOffset = getCanvasOffset(canvasRef);
        this.currentPlayer = state.player;

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

    private setBattleInitialState() {
        for (const position of this.data.board.entitiesPosition) {
            const index = this.data.entities
                .findIndex(e => e.id === position.entityIdentifier);
            if (index === -1) 
                continue;
            this.battleRender.setPlayer(
                this.data.entities[index],
                position,
                position.entityIdentifier === this.currentPlayer.id
            );
        }
    }

    private setServerEventsListenners() {
        this.server
            .onEntitiesMove(this.handleEntitiesMove())
            .onSkill(this.handleSkill());
    }

    private handleEntitiesMove(): IServerEvents['EntitiesMove'] {
        return (moves: {[entity: string]: TCoordinates}) => {
            for (const key in moves) {
                const entityIndex = this.data.entities.findIndex(e => e.id === key);
                this.battleRender.setPlayer(
                    this.data.entities[entityIndex], 
                    moves[key], 
                    this.currentPlayer.id === key
                );
            }
        }
    }

    private handleSkill(): IServerEvents['Skill'] {
        return (_, __, target, currentHealth) => {
            this.battleRender.updateEntityCurrentHealth(
                target === this.currentPlayer.id,
                currentHealth
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
        if (this.skillSelected !== undefined) {
            const index = this.data.board.entitiesPosition
                .findIndex(e => e.x === click.x && e.y === click.y)
            if (index > -1) {
                this.server.Skill(
                    this.skillSelected, 
                    this.data.board.entitiesPosition[index].entityIdentifier
                );
                this.unselectSkill();
                return;
            }
        }
        this.server.Move(click.x, click.y);
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
