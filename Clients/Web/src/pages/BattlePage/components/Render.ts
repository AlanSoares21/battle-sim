import { ICanvasWrapper, SubAreaOnCanvasDecorator } from "../../../CanvasWrapper";
import { subCoordinates } from "../../../CoordinatesUtils";
import { IAsset, IAssetsData, TCanvasCoordinates, TCanvasSize, TSize } from "../../../interfaces";

export interface IRender {
    canvas: ICanvasWrapper;
    render: () => void;
}

const colors = {
    'skill-background': '#9d9d9d',
    'skill-name': '#FFFFFF'
}

const skillNameHeigth = 0.15;

export class SkillRender implements IRender {
    canvas: ICanvasWrapper;
    private size: TCanvasSize;
    private name: string;
    private asset: IAsset;
    private textRectHeight: number;
    private assetPosition: TCanvasCoordinates;
    private assetSize: TSize;

    constructor(canvas: ICanvasWrapper, name: string, asset: IAsset) {
        this.canvas = canvas;
        this.name = name;
        this.asset = asset;
        this.size = canvas.getSize();
        this.textRectHeight = this.size.height * skillNameHeigth;
        
        const assetSide = this.size.height - this.textRectHeight;
        this.assetSize = { height: assetSide, width: assetSide };
        this.assetPosition = { 
            x: (this.size.height - assetSide) / 2, 
            y: this.textRectHeight
        };
    }

    private drawBackground() {
        this.canvas.drawRect(
            colors['skill-background'], 
            { x: 0, y: 0}, 
            this.size
        );
    }

    private writeText() {
        this.canvas.writeText(
            { x: 0, y: this.textRectHeight - 3 }, 
            this.name, 
            colors['skill-name']
        );
    }

    private drawAsset() {
        this.canvas.drawAsset(
            this.asset, 
            {
                ...this.assetSize,
                startAt: this.assetPosition
            }
        );
    }

    render() {
        this.drawBackground()
        this.drawAsset();
        this.writeText();
    }
}

const skillAssetsMap: { [skillName: string]: keyof IAssetsData } = {
    'basicNegativeDamageOnX': 'base-damage-x-negative',
    'basicNegativeDamageOnY': 'base-damage-y-negative',
    'basicPositiveDamageOnX': 'base-damage-x-positive',
    'basicPositiveDamageOnY': 'base-damage-y-positive'
}

const skillMarginLeft = 10;
const skillSpaceFromBottom = 10;
const skillSpaceFromTop = 10;

function getSkillRenders(
    skills: string[], 
    assets: IAssetsData, 
    canvas: ICanvasWrapper
): SkillRender[] {
    const canvasSize = canvas.getSize();
    const side = canvasSize.height - skillSpaceFromBottom - skillSpaceFromTop;
    const skillSize: TCanvasSize = {
        width: side,
        height: side
    }
    console.log('creating renders canvas size', canvasSize, side);

    return skills.map((name, i) => {
        let asset: IAsset;
        if (name in skillAssetsMap)
            asset = assets[skillAssetsMap[name]];
        else
            asset = assets['unknowed-skill'];
        const startAt: TCanvasCoordinates = {
            x: skillSize.width * i,
            y: skillSpaceFromTop
        };
        startAt.x += skillMarginLeft * (i + 1);
        const skillCanvas = new SubAreaOnCanvasDecorator(canvas, startAt, skillSize);
        return new SkillRender(skillCanvas, name, asset);
    });
}

export class SkillBarController implements IRender {
    canvas: ICanvasWrapper;
    private skills: string[];
    private skillsRenders: SkillRender[] = [];
    
    constructor(
        canvas: ICanvasWrapper, 
        skills: string[], 
        assetsData: IAssetsData
    ) {
        this.canvas = canvas;
        this.skills = skills;
        this.skillsRenders = getSkillRenders(skills, assetsData, canvas);
    } 

    clickOnSkill(click: TCanvasCoordinates): string | undefined {
        click = this.canvas.distanceFromOrigin(click);
        for (let index = 0; index < this.skillsRenders.length; index++) {
            const render = this.skillsRenders[index];
            if (render.canvas.isOnCanvas(click))
                return this.skills[index];
        }
        return;
    }
    
    render() {
        this.canvas.drawEmptyRect('#d9d9d9', { x: 0, y: 0}, this.canvas.getSize());
        this.skillsRenders.forEach(r => r.render());
    }
}