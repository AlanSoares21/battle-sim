import React, { MouseEventHandler, useCallback, useContext, useEffect, useState } from "react";
import { BattleContext } from "../BattleContext";
import SkillBar from "./SkillBar";
import { TBoard, TBoardCoordinates, TCanvasCoordinates, TCanvasSize, TSize } from "../../../interfaces";
import CanvasWrapper from "../../../CanvasWrapper";
import BattleRender from "./BattleRender";
import { IServerEvents } from "../../../server";

/**
 * tela -> 400x300
 * tabuleiro -> 8x8
 * tabuleiro ocupa 30% da tela
 * tabuleiro no canvas -> 400*0.3 x 300*0.3 = 30% da area de 400x300?
 * 
 * retangulo 4x2
 * area 8
 * 0.5 do retangulo -> 2x1
 * area 2
 * 
 * 4*2 = 8
 * 
 * area = 4 
 * wh = 4
 * 
 * proporção
 * 4/2 = w/h -> w = Wh/H
 * 
 * 4/2 = w/h
 * 4h = 2w
 * 2h = w
 * 
 * equações
 * wh = 4
 * 2h = w
 * 
 * wh = 4
 * 2h * h = 4
 * 2h² = 4
 * h² = 2
 * 
 * (Wh/H) * h = targetArea
 * (Wh²/H) = targetArea
 * h² = targetArea * H / W
 * 
 * w = Wh/H
 * 
 * 2h = w
 * w = 2 * sqtr(2)
 * 
 */

const isDev = true;

const onAttack = (render: BattleRender, userId: string): IServerEvents['Attack'] => 
(
    (_, target, currentHealth) => {
        render.updateEntityCurrentHealth(target === userId, currentHealth);
        render.render();
    }
)

const onSkill = (render: BattleRender, userId: string): IServerEvents['Skill'] => 
(
    (_, source, target, currentHealth) => {
        render.updateEntityCurrentHealth(target === userId, currentHealth);
        render.render();
    }
)

export const BattleController: React.FC = () => {
    const { battle, server, player } = useContext(BattleContext);

    const [canvasOffset, setCanvasOffset] = useState({ top: 0, left: 0 });
    const [renderController, setRenderController] = useState<BattleRender>();

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
            }
        }, 
    [renderController, canvasOffset, handleBoardClick]);

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
        
        const value = new BattleRender(
            canvasWrapper,
            board
        );
        
        setRenderController(value);
    }, [setRenderController, setCanvasOffset]);

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
    }, [battle.board.entitiesPosition, renderController]);

    /**
     * update entities life when server events happen
     */
    useEffect(() => {
        if (renderController)
            server
            .onAttack(onAttack(renderController, player.id))
            .onSkill(onSkill(renderController, player.id));
    }, [server, renderController]);

    useEffect(() => {
        if (renderController) {
            console.log("render loop start");
            const renderLoop = setInterval(() => {renderController.render()}, 1000);
            return () => {
                clearInterval(renderLoop);
                console.log("render loop finished");
            };
        }
        console.log("render controller unset")
    }, [renderController]);

    return(<div>
            <canvas
                onClick={handleCanvasClick}
                ref={setCanvasRef}
                style={{background: "#000000", width: '99%'}}>
            </canvas>
            <SkillBar selected={skillSelected} onSkillSelect={setSkillSelected} />
    </div>);
}
