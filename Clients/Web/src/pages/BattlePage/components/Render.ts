import { ICanvasWrapper, SubAreaOnCanvasDecorator } from "../../../CanvasWrapper";
import { IAsset, IAssetsData, TCanvasCoordinates, TCanvasSize } from "../../../interfaces";

export interface IRender {
    render: () => void;
}

const colors = {
    'skill': '#9d9d9d',
    'skill-name': '#FFFFFF'
}

const skillNameHeigth = 0.15;

export class SkillRender implements IRender {
    private canvas: ICanvasWrapper;
    private size: TCanvasSize;
    private name: string;

    constructor(canvas: ICanvasWrapper, name: string, asset: IAsset) {
        this.canvas = canvas;
        this.name = name;
        this.size = canvas.getSize();
    }

    render() {
        this.canvas.drawEmptyRect(colors['skill'], { x: 0, y: 0}, this.size);
        const skillNameRectHeight = this.size.height * skillNameHeigth;
        this.canvas.drawRect(
            colors['skill'], 
            { x: 0, y: 0}, 
            {
                width: this.size.width,
                height: skillNameRectHeight
            }
        );
        this.canvas.writeText(
            { x: 0, y: skillNameRectHeight - 3 }, 
            this.name, 
            colors['skill-name']
        );
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
    private canvas: ICanvasWrapper;
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
    
    render() {
        this.canvas.drawEmptyRect('#d9d9d9', { x: 0, y: 0}, this.canvas.getSize());
        this.skillsRenders.forEach(r => r.render());
    }
}