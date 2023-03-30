import React, { useEffect, useMemo, useState } from "react";
import { IEntity, TCoordinates } from "../../../interfaces";
import CanvasWrapper from "../../../CanvasWrapper";

export interface ILifeBarProps {
    entities: IEntity[];
}

const LifeBar: React.FC<ILifeBarProps> = ({
    entities
}) => {
    const [canvasRef, setCanvasRef] = useState<HTMLCanvasElement | null>(null);
    
    const canvasWrapper = useMemo<CanvasWrapper | undefined>(() => {
        if (!canvasRef) {
            console.error("canvas ref ta nulo");
            return;
        }
        const context = canvasRef.getContext('2d');
        if (!context) {
            console.error("contexto ta nulo");
            return;
        }
        return new CanvasWrapper(context)
    }, [ canvasRef ]);

    useEffect(() => {
        if (canvasWrapper !== undefined) {
            let radiusScale = 2;
            let lifePointRadius = 2 * radiusScale;
            canvasWrapper.drawRect('#555AAA', { x: 0, y: 0 }, { x: 500, y: 125 })
            const lifeCircleCoord: TCoordinates = { x: 0, y: 75 };
            const nameGap = 15;
            const nameStart: TCoordinates = { x: nameGap, y: 15 };

            
            for (const entity of entities) {
                const radius = entity.state.healthRadius * radiusScale;
                lifeCircleCoord.x += radius;
                canvasWrapper.drawCircle(lifeCircleCoord, radius, '#FF7777');
                
                const y = entity.state.currentHealth.y * radiusScale;
                const x = entity.state.currentHealth.x * radiusScale;

                const currentLife: TCoordinates = {
                    x :  lifeCircleCoord.x,
                    y : lifeCircleCoord.y
                };

                if (entity.state.healthRadius < entity.state.currentHealth.x)
                    currentLife.x = lifeCircleCoord.x + x;
                else if (entity.state.healthRadius > entity.state.currentHealth.x)
                    currentLife.x = lifeCircleCoord.x - x;
                
                if (entity.state.healthRadius < entity.state.currentHealth.y)
                    currentLife.y = lifeCircleCoord.y + y;
                else if (entity.state.healthRadius > entity.state.currentHealth.y)
                    currentLife.y = lifeCircleCoord.y - y;

                canvasWrapper.drawCircle(currentLife, lifePointRadius, '#FFFFFF');

                lifeCircleCoord.x += radius;
                
                canvasWrapper.writeText(nameStart, entity.id, '#FFFFFF');
                nameStart.x += nameGap + lifeCircleCoord.x;
            }
        }
    }, [entities, canvasWrapper]);

    return (<div>
        <canvas 
            style={{border: "1px solid #f1f1f1"}}
            width={500}
            height={125}
            ref={setCanvasRef}>
        </canvas>
        {
            entities.reduce((v, e) => 
                `${v}${e.id}: ${e.state.healthRadius} - (${e.state.currentHealth.x},${e.state.currentHealth.y}) | `
                , '| ')
        }
    </div>);
}

export default LifeBar;