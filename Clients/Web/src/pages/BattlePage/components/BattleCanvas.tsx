import React, { MouseEventHandler, useCallback, useContext, useEffect, useMemo, useState } from "react";
import { BattleContext } from "../BattleContext";
import { IEntity, TBoard, TBoardCoordinates, TCanvasCoordinates } from "../../../interfaces";
import CanvasWrapper from "../../../CanvasWrapper";
import BattleRenderController from "./BattleRenderController";
import { IServerEvents } from "../../../server";

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

const onAttack = (render: BattleRenderController, userId: string): IServerEvents['Attack'] => 
(
    (_, target, currentHealth) => {
        render.updateEntityCurrentHealth(target === userId, currentHealth);
        render.render();
    }
)

const onSkill = (render: BattleRenderController, userId: string): IServerEvents['Skill'] => 
(
    (_, source, target, currentHealth) => {
        render.updateEntityCurrentHealth(target === userId, currentHealth);
        render.render();
    }
)

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

export const BattleCanvas: React.FC = () => {
    const { battle, server, player, assets } = useContext(BattleContext);
    
    const [canvasOffset, setCanvasOffset] = useState({ top: 0, left: 0 });
    const [renderController, setRenderController] = useState<BattleRenderController>();

    const [skillSelected, setSkillSelected] = useState<string>();

    const handleBoardClick = useCallback(
        (cell: TBoardCoordinates) => {
            const index = battle.board.entitiesPosition.findIndex(p =>
                p.x === cell.x && p.y === cell.y);
            if (index === -1) {
                server.Move(cell.x, cell.y)
                return;
            }
            const target = battle.board.entitiesPosition[index].entityIdentifier;
            if (skillSelected !== undefined) {
                server.Skill(skillSelected, target);    
                setSkillSelected(s => s === skillSelected ? undefined : s);
            }
            else
                server.Attack(target);
        }, 
    [battle.board.entitiesPosition, server, skillSelected]);

    const handleCanvasClick = useCallback<MouseEventHandler<HTMLCanvasElement>>(
        ev => {
            if (!ev || !renderController)
                return;
            
            const canvasClick: TCanvasCoordinates = { 
                x: ev.clientX - canvasOffset.left,
                y: ev.clientY - canvasOffset.top
            };

            const boardClick = renderController.clickOnBoard(canvasClick);
            
            if (boardClick) {
                handleBoardClick(boardClick);
                renderController.pointer.setPosition(boardClick);
                return;
            }
            
            const skill = renderController.clickOnSkill(canvasClick);
            if (skill) {
                if (skillSelected === skill) 
                    setSkillSelected(undefined);
                else {
                    setSkillSelected(skill);
                    renderController.skillBarController.selectSkill(skill);
                }
            }
        }, 
    [renderController, canvasOffset, handleBoardClick, skillSelected, setSkillSelected]);

    const setCanvasRef = useCallback((canvasRef: HTMLCanvasElement | null) => {
        if (!canvasRef) {
            console.error('canvas reference is null');
            return;
        }
        
        setCanvasOffset({ 
            left: canvasRef.offsetLeft + canvasRef.clientLeft, 
            top: canvasRef.offsetTop + canvasRef.clientTop 
        }); 

        canvasRef.height = canvasRef.clientHeight;
        canvasRef.width = canvasRef.clientWidth;
        
        const context = canvasRef.getContext('2d');
        if (!context) {
            console.error("context is null")
            return;
        }
        const canvasWrapper = new CanvasWrapper(context)
        const board: TBoard = battle.board.size;
        const value = new BattleRenderController(
            canvasWrapper,
            board,
            assets,
            player,
            getSkillsBindingsToKeyboard(player.skills)
        );
        
        setRenderController(value);
    }, [setRenderController, setCanvasOffset, assets, player, battle.board.size]);

    useEffect(() => {
        console.log(`Update mana in render. ${new Date()}`)
    }, [])

    /**
     * update entities position on board
     */
    useEffect(() => {
        if (!renderController)
            return;
        for (let index = 0; index < battle.entities.length; index++) {
            const entity = battle.entities[index];
            const position = battle.board.entitiesPosition
                .find(e => e.entityIdentifier === entity.id);
            if (position !== undefined)
                renderController.setPlayer(entity, position, entity.id === player.id);
        }
    }, [battle.board.entitiesPosition, renderController, player.id, battle.entities]);

    /**
     * update entities life when server events happen
     */
    useEffect(() => {
        if (renderController)
            server
            .onAttack(onAttack(renderController, player.id))
            .onSkill(onSkill(renderController, player.id));
    }, [server, player.id, renderController]);

    useEffect(() => {
        if (renderController) {
            console.log("render loop start");
            const renderLoop = setInterval(() => {renderController.render()}, 500);
            return () => {
                clearInterval(renderLoop);
                console.log("render loop finished");
            };
        }
        console.log("render controller unset")
    }, [renderController]);

    
    useEffect(() => {
        if (renderController !== undefined && skillSelected === undefined)
            renderController.skillBarController.unSelectSkill();
    }, [ renderController, skillSelected ]);

    useEffect(() => {
        if (renderController) {
            const mappedKeyBoard: { [key: string]: string } = 
                getKeybordBindingsToSkills(player.skills);
            document.onkeydown = (ev) => {
                const key = ev.key;
                if (key in mappedKeyBoard) {
                    const skill = mappedKeyBoard[key];
                    renderController.skillBarController.selectSkill(skill);
                    setSkillSelected(skill);
                }
            };
        }
    }, [renderController, player]);
    
    return(<canvas
        onClick={handleCanvasClick}
        ref={setCanvasRef}
        style={{background: "#000000", width: '99%'}}>
    </canvas>);
}
