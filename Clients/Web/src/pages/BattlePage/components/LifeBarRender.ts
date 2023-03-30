import CanvasWrapper from "../../../CanvasWrapper"
import { IEntity, TCoordinates } from "../../../interfaces";

const config = {
    background: {
        color: '#555AAA'
    },
    life: {
        circle: {
            color: '#FF7777',
            initialCoord: { x: 0, y: 75 }
        },
        point: {
            color: '#FFFFFF'
        }
    },
    name: {
        color: '#FFFFFF',
        gap: 15,
        initialCoord: { x: 15, y: 15 }
    },
    baseCoord: { x: 0, y: 0 }
}
export default class LifeBarRender {
    private entities: IEntity[] = [];
    private scale = {
        life: 2
    };

    private canvas: CanvasWrapper;

    constructor(canvas: CanvasWrapper) {
        this.canvas = canvas;
    }

    
    drawBackground() {
        const size = this.canvas.getSize();
        this.canvas.drawRect(
            config.background.color, 
            { x: 0, y: 0 }, 
            { x: size.width, y: size.height }
        );
    }

    drawName(name: string, start: TCoordinates) {
        this.canvas.writeText(start, name, config.name.color);
    }

    drawLifeSphere(radius: number, center: TCoordinates) {
        this.canvas.drawCircle(center, radius, config.life.circle.color);
    }

    drawCurrentLifePoint(radius: number, center: TCoordinates) {
        this.canvas.drawCircle(center, radius, config.life.point.color);
    }

    setEntity(entity: IEntity) {
        this.entities.push(entity);
    }

    calculeCurrentLifeCoord(
        sphereCenter: TCoordinates, 
        currentHealth: TCoordinates,
        healthRadius: number): TCoordinates
    {
        const currentLifeCoord: TCoordinates = sphereCenter;
        
        const y = currentHealth.y * this.scale.life;
        const x = currentHealth.x * this.scale.life;

        if (healthRadius < currentHealth.x)
            currentLifeCoord.x = sphereCenter.x + x;
        else if (healthRadius > currentHealth.x)
            currentLifeCoord.x = sphereCenter.x - x;
        
        if (healthRadius < currentHealth.y)
            currentLifeCoord.y = sphereCenter.y + y;
        else if (healthRadius > currentHealth.y)
            currentLifeCoord.y = sphereCenter.y - y;
        
        return currentLifeCoord;
    }

    render() {
        this.drawBackground();
        const coordinates = {
            base: config.baseCoord,
            name: config.name.initialCoord,
            sphereCenter: config.life.circle.initialCoord
        };
        for (const entity of this.entities) {
            this.drawName(entity.id, coordinates.name);
            
            const circleRadius = entity.state.healthRadius * this.scale.life;
            
            coordinates.sphereCenter.x += circleRadius;
            this.drawLifeSphere(circleRadius, coordinates.sphereCenter);
            this.drawCurrentLifePoint(
                this.scale.life, 
                this.calculeCurrentLifeCoord(
                    coordinates.sphereCenter,
                    entity.state.currentHealth,
                    entity.state.healthRadius
                )
            );
            coordinates.sphereCenter.x += circleRadius;

            coordinates.base.x += coordinates.sphereCenter.x;
            coordinates.name.x = coordinates.base.x + config.name.gap;
        }
    }
}