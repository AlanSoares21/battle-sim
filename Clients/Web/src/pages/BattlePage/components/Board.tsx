import React, { useCallback, useContext, useEffect, useRef } from "react";
import { BattleContext } from "../BattleContext";
import CanvasWrapper from "../../../CanvasWrapper";
import { TBoard } from "../../../interfaces";
import BoardCanvas from "../../../BoardCanvas";
import BoardRenderController from "../../../BoardRenderController";

export interface IBoardProps { cellSize: number }

const Board: React.FC<IBoardProps> = ({ cellSize }) => {
    const { battle } = useContext(BattleContext);

    const canvasRef = useRef<HTMLCanvasElement>(null);

    const handleCanvasClick = useCallback<React.MouseEventHandler<HTMLCanvasElement>>(ev => {
        
    }, []);

    useEffect(() => {
        const canvas = canvasRef.current;
        if (!canvas) {
            console.log("canvas ta nulo");
            return;
        }
        const context = canvas.getContext('2d');
        if (!context) {
            console.log("contexto ta nulo")
            return;
        }
        const canvasWrapper = new CanvasWrapper(context)
        const board: TBoard = { height: 6, width: 6 };
        const boardCanvas = new BoardCanvas(board, canvasWrapper, cellSize);
        const value = new BoardRenderController(board, boardCanvas)
        value.render();
    }, []);

    return (<div>
        <canvas 
            ref={canvasRef} 
            width={battle.board.width * cellSize} 
            height={battle.board.height * cellSize} 
            style={{border: "1px solid #f1f1f1"}}
            onClick={handleCanvasClick}>
        </canvas>
    </div>);
}

export default Board;