import React, { useCallback, useContext, useEffect, useMemo, useState } from "react";
import { BattleContext } from "../BattleContext";
import CanvasWrapper from "../../../CanvasWrapper";
import { TBoard } from "../../../interfaces";
import BoardCanvas from "../../../BoardCanvas";
import BoardRenderController from "../../../BoardRenderController";

export interface IBoardProps { cellSize: number }

const Board: React.FC<IBoardProps> = ({ cellSize }) => {
    const { battle } = useContext(BattleContext);

    const [canvasRef, setCanvasRef] = useState<HTMLCanvasElement | null>(null);

    const render = useMemo<BoardRenderController | undefined>(() => {
        if (!canvasRef) {
            console.log("canvas ref ta nulo");
            return;
        }
        const context = canvasRef.getContext('2d');
        if (!context) {
            console.log("contexto ta nulo")
            return;
        }
        const canvasWrapper = new CanvasWrapper(context)
        const board: TBoard = battle.board;
        const boardCanvas = new BoardCanvas(board, canvasWrapper, cellSize);
        const value = new BoardRenderController(board, boardCanvas)
        for (const player of battle.board.entitiesPosition) {
            value.placePlayer({ x: player.x, y: player.y }, { name: player.entityIdentifier });
        }
        return value;
    }, [ canvasRef, battle ])

    const handleCanvasClick = useCallback<React.MouseEventHandler<HTMLCanvasElement>>(ev => {
        
    }, []);

    useEffect(() => {
        if (render !== undefined)
            render.render();
    }, [render]);

    return (<div>
        <canvas 
            ref={setCanvasRef} 
            width={battle.board.width * cellSize} 
            height={battle.board.height * cellSize} 
            style={{border: "1px solid #f1f1f1"}}
            onClick={handleCanvasClick}>
        </canvas>
    </div>);
}

export default Board;