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
        const index = this.entities.findIndex(e => e.id === entity.id);
        if (index === -1)
            this.entities.push(entity);
        else
            this.entities[index] = entity;
    }
    
    setEntityCurrentHealth(id: string, health: TCoordinates) {
        const index = this.entities.findIndex(e => e.id === id);
        if (index !== -1) {
            this.entities[index].state.currentHealth = health;
        }
    }

    calculeCurrentLifeCoord(
        sphereCenter: TCoordinates, 
        currentHealth: TCoordinates,
        healthRadius: number): TCoordinates
    {
        let currentLifeCoord: TCoordinates = {
            x: sphereCenter.x,
            y: sphereCenter.y
        };
        
        let y = Math.abs(currentHealth.y - healthRadius) * this.scale.life * -1;
        let x = Math.abs(currentHealth.x - healthRadius) * this.scale.life;

        if (healthRadius < currentHealth.x) {
            currentLifeCoord.x = sphereCenter.x + x;
        }
        else if (healthRadius > currentHealth.x) {
            currentLifeCoord.x = sphereCenter.x - x;
        }
        
        if (healthRadius < currentHealth.y) {
            currentLifeCoord.y = sphereCenter.y + y;
        }
        else if (healthRadius > currentHealth.y) {
            currentLifeCoord.y = sphereCenter.y - y;
        }
        
        return currentLifeCoord;
    }

    render() {
        this.drawBackground();
        let coordinates: {
            base: TCoordinates;
            name: TCoordinates;
            sphereCenter: TCoordinates;
        } = {
            base: { x: config.baseCoord.x, y: config.baseCoord.y },
            name: { x: config.name.initialCoord.x, y: config.name.initialCoord.y },
            sphereCenter: { 
                x: config.life.circle.initialCoord.x, 
                y: config.life.circle.initialCoord.y 
            }
        };
        for (const entity of this.entities) {
            this.drawName(entity.id, coordinates.name);
            
            let circleRadius = entity.state.healthRadius * this.scale.life;
            
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