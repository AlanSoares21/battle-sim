import React, { useCallback, useContext, useEffect, useMemo, useState } from "react";
import { BattleContext } from "../BattleContext";
import CanvasWrapper from "../../../CanvasWrapper";
import { TBoard, TBoardCoordinates, TCanvasCoordinates } from "../../../interfaces";
import BoardCanvas from "../../../BoardCanvas";
import BoardRenderController from "../../../BoardRenderController";

export interface IBoardProps { 
    cellSize: number;
    onBoardClick: (cell: TBoardCoordinates) => any;
}

function getMiddleCoordinates(board: TBoard): TBoardCoordinates {
    return {
        x: board.width / 2,
        y: board.height / 2
    }
}

const Board: React.FC<IBoardProps> = ({ cellSize, onBoardClick }) => {
    const { battle, server } = useContext(BattleContext);

    const [canvasRef, setCanvasRef] = useState<HTMLCanvasElement | null>(null);

    const render = useMemo<BoardRenderController | undefined>(() => {
        if (!canvasRef) {
            console.error("canvas ref ta nulo");
            return;
        }
        const context = canvasRef.getContext('2d');
        if (!context) {
            console.error("contexto ta nulo")
            return;
        }
        const canvasWrapper = new CanvasWrapper(context)
        const board: TBoard = battle.board.size;
        const boardCanvas = new BoardCanvas(board, canvasWrapper, cellSize);
        const value = new BoardRenderController(boardCanvas);
        value.placePointer(getMiddleCoordinates(battle.board.size));
        return value;
    }, [ canvasRef, battle.board.size, cellSize ])

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
        
        render.placePointer(boardCoordinates);
        
        onBoardClick(boardCoordinates);
    }, [render, canvasRef, onBoardClick]);

    useEffect(() => {
        if (render !== undefined) {
            for (const entity of battle.board.entitiesPosition) {
                render.placePlayer({
                    x: entity.x,
                    y: entity.y
                }, 
                {
                    name: entity.entityIdentifier
                });
            }
            render.render();
        }
    }, [render, battle.board.entitiesPosition]);

    return (<div>
        <canvas 
            ref={setCanvasRef} 
            width={battle.board.size.width * cellSize} 
            height={battle.board.size.height * cellSize} 
            style={{border: "1px solid #f1f1f1"}}
            onClick={handleCanvasClick}>
        </canvas>
    </div>);
}

export default Board;