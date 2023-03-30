import React, { useCallback, useContext, useEffect, useMemo, useState } from "react";
import { BattleContext } from "../BattleContext";
import CanvasWrapper from "../../../CanvasWrapper";
import { TBoard, TCanvasCoordinates } from "../../../interfaces";
import BoardCanvas from "../../../BoardCanvas";
import BoardRenderController from "../../../BoardRenderController";

export interface IBoardProps { cellSize: number }

const Board: React.FC<IBoardProps> = ({ cellSize }) => {
    const { battle, server } = useContext(BattleContext);

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
        if (render === undefined) {
            console.error("click handled but render object is null");
            return;
        }
        const canvasCoordinates: TCanvasCoordinates = { 
            x: ev.clientX - (canvasRef !== null ? canvasRef.offsetLeft : 0), 
            y: ev.clientY - (canvasRef !== null ? canvasRef.offsetTop : 0)
        };

        const boardCoordinates = render.boardCanvas
            .canvasToBoardCoordinates(canvasCoordinates);
        
        render.placePointerAndRender(boardCoordinates);
        
        server.Move(boardCoordinates.x, boardCoordinates.y);
    }, [render]);

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