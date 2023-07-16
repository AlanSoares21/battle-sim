import React, { useCallback, useContext, useEffect, useState } from "react";
import LifeBar from "./LifeBar";
import { BattleContext } from "../BattleContext";
import SkillBar from "./SkillBar";
import { TBoard, TBoardCoordinates, TCanvasCoordinates, TCanvasSize, TSize } from "../../../interfaces";
import CanvasWrapper from "../../../CanvasWrapper";
import BoardRender from "./BoardRender";

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

export const BattleController: React.FC = () => {
    const { battle, server, player } = useContext(BattleContext);

    const [canvasOffset, setCanvasOffset] = useState({ top: 0, left: 0 });
    const [renderController, setRenderController] = useState<BoardRender>();

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
    [battle, server, skillSelected]);

    const setCanvasRef = useCallback((canvasRef: HTMLCanvasElement | null) => {
        if (!canvasRef) {
            console.error('canvas reference is null');
            return;
        }
        console.log('canvas ref', canvasRef);
        
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
        
        const value = new BoardRender(
            canvasWrapper,
            board
        );
        
        const enemyEntity = battle.entities.find(e => e.id !== player.id);
        const enemyPosition = battle.board.entitiesPosition
            .find(e => e.entityIdentifier !== player.id);
        
        if (enemyEntity !== undefined && enemyPosition !== undefined)
            value.setPlayer(enemyEntity, enemyPosition, false);
        
        setRenderController(value);
    }, [setRenderController, setCanvasOffset]);

    useEffect(() => {
        if (renderController) {
            if (isDev) {
                renderController.render();
                return;
            }
            console.log("render loop start");
            const renderLoop = setInterval(() => {renderController.render()}, 3000);
            return () => {
                clearInterval(renderLoop);
                console.log("render loop finished");
            };
        }
        console.log("render controller unset")
    }, [renderController]);

    return(<div>
            <canvas
                onClick={(e) => {
                    console.log('canvas click', {
                        offset: canvasOffset,
                        e: e,
                        tX: e.clientX - canvasOffset.left,
                        tY: e.clientY - canvasOffset.top
                    })
                }}
                ref={setCanvasRef}
                style={{background: "#000000", width: '99%'}}>
            </canvas>
        {
            /*
            <LifeBar />
            <Board onBoardClick={handleBoardClick} cellSize={25} />
            <SkillBar selected={skillSelected} onSkillSelect={setSkillSelected} />
            */
        }
    </div>);
}
