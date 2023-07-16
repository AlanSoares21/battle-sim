import CanvasWrapper from "../../../CanvasWrapper"
import { IEntity, TCoordinates } from "../../../interfaces";
import { 
    LifeCoordRender,
    LifeSphereRender,
    NameRender
} from "./LifeSphereRenderComponents";

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

interface IPlayer {
    id: string;
    life: TCoordinates;
    healthRadius: number;
}

function entityToPlayer(entity: IEntity): IPlayer {
    return {
        id: entity.id,
        healthRadius: entity.healthRadius,
        life: { x: 0, y: 0 }
    }
}

interface IPlayerRenders {
    lifeSphere: LifeSphereRender;
    name: NameRender;
    lifeCoord: LifeCoordRender;
}

export default class LifeBarRender {
    private players: IPlayer[] = [];
    private renders: IPlayerRenders[] = [];
    private maxSphereSize = 100;
    private scale = {
        life: 2
    };

    private canvas: CanvasWrapper;

    constructor(canvas: CanvasWrapper) {
        this.canvas = canvas;
        
    }

    addPlayer(player: IPlayer)
    {
        const index = this.players.push(player) - 1;
        this.renders.push(this.createPlayerRenders(player, index));
    }

    createPlayerRenders(player: IPlayer, index: number): IPlayerRenders {
        const nameHeigth = 15;
        
        const sphereScale = Math.abs(this.maxSphereSize / (player.healthRadius * 2));
        const healthRadiusInScale = player.healthRadius * sphereScale;
        
        return {
            name: new NameRender(
                this.canvas, 
                { x: this.maxSphereSize * index, y: nameHeigth },
                player.id
            ),
            lifeSphere: new LifeSphereRender(
                this.canvas,
                { x: index * this.maxSphereSize, y: 0 + nameHeigth },
                healthRadiusInScale
            ),
            lifeCoord: new LifeCoordRender(
                this.canvas,
                { x: index * this.maxSphereSize, y: nameHeigth },
                sphereScale,
                healthRadiusInScale
            )
        }
    }

    drawBackground() {
        const size = this.canvas.getSize();
        this.canvas.drawRect(
            config.background.color, 
            { x: 0, y: 0 }, 
            size
        );
    }

    setEntity(entity: IEntity) {
        const index = this.players.findIndex(p => p.id === entity.id);
        if (index === -1)
            this.addPlayer(entityToPlayer(entity));
        else
            this.players[index] = entityToPlayer(entity);
    }
    
    setEntityCurrentHealth(id: string, health: TCoordinates) {
        const index = this.players.findIndex(e => e.id === id);
        if (index !== -1) {
            this.players[index].life = health;
            this.renders[index].lifeCoord.setLife(health);
        }
    }

    calculeCurrentLifeCoord(
        sphereCenter: TCoordinates, 
        currentHealth: TCoordinates): TCoordinates
    {
        let currentLifeCoord: TCoordinates = {
            x: sphereCenter.x,
            y: sphereCenter.y
        };
        
        let y = currentHealth.y * this.scale.life;
        let x = currentHealth.x * this.scale.life;
        
        currentLifeCoord.y -= y;
        currentLifeCoord.x += x;

        return currentLifeCoord;
    }

    render() {
        this.drawBackground();
        for (let index = 0; index < this.players.length; index++) {
            this.renderPlayer(index);
        }
    }

    renderPlayer(index: number) {
        const render = this.renders[index];
        render.lifeSphere.render();
        render.lifeCoord.render();
        render.name.render();
    }
}