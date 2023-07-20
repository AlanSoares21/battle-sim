import { ICanvasWrapper, SubAreaOnCanvasDecorator } from "../../../CanvasWrapper";
import { IAsset, IAssetsData, TCanvasCoordinates, TCanvasSize, TSize } from "../../../interfaces";
import { IRender } from "./Render";

const colors = {
    'skill-background': '#9d9d9d',
    'skill-text': '#FFFFFF',
    'skillbar-border': '#d9d9d9'
}

const skillNameHeigth = 0.15;

export class SkillRender implements IRender {
    canvas: ICanvasWrapper;
    private size: TCanvasSize;
    private text: string;
    private asset: IAsset;
    private textRectHeight: number;
    private assetPosition: TCanvasCoordinates;
    private assetSize: TSize;
    private isSelected: boolean = false;

    constructor(canvas: ICanvasWrapper, text: string, asset: IAsset) {
        this.canvas = canvas;
        this.text = text;
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

    setSelected(value: boolean) {
        this.isSelected = value;
        this.render();
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
            this.text, 
            colors['skill-text']
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

    private drawBlur() {
        this.canvas.drawRect(
            colors['skill-background'], 
            { x: 0, y: 0}, 
            this.size,
            0.5
        );
    }

    render() {
        this.drawBackground()
        this.drawAsset();
        this.writeText();
        if (this.isSelected)
            this.drawBlur();
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
    canvas: ICanvasWrapper,
    skillKeyBinginds: { [skillName: string]: string }
): SkillRender[] {
    const canvasSize = canvas.getSize();
    const side = canvasSize.height - skillSpaceFromBottom - skillSpaceFromTop;
    const skillSize: TCanvasSize = {
        width: side,
        height: side
    }

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
        let text = name;
        if (text in skillKeyBinginds)
            text = skillKeyBinginds[text];
        return new SkillRender(skillCanvas, text, asset);
    });
}

export class SkillBarController implements IRender {
    canvas: ICanvasWrapper;
    private skills: string[];
    private skillsRenders: SkillRender[] = [];
    private selected?: number;
    
    constructor(
        canvas: ICanvasWrapper, 
        skills: string[], 
        assetsData: IAssetsData,
        skillKeyBinginds: { [skillName: string]: string }
    ) {
        this.canvas = canvas;
        this.skills = skills;
        this.skillsRenders = getSkillRenders(
            skills, 
            assetsData, 
            canvas, 
            skillKeyBinginds
        );
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

    selectSkill(name: string) {
        for (let index = 0; index < this.skills.length; index++) {
            if (name === this.skills[index]) {
                this.skillsRenders[index].setSelected(true);
                this.selected = index;
                break;
            }
        }
    }

    unSelectSkill() {
        if (this.selected !== undefined) {
            this.skillsRenders[this.selected].setSelected(false);
            this.selected = undefined;
        }
    }
    
    render() {
        this.canvas.drawEmptyRect(
            colors['skillbar-border'], 
            { x: 0, y: 0}, 
            this.canvas.getSize()
        );
        this.skillsRenders.forEach(r => r.render());
    }
}