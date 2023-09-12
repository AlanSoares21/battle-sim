import { DamageDirection, IEntity, IEntityPosition } from "../../../interfaces";
import { IBattleContext } from "../BattleContext";
import { CanvasController } from "./BattleCanvas"
import BattleRenderController, { IBattleRenderControllerProps } from "./BattleRenderController";
import { SkillBarController } from "./SkillBarRenderComponents";


function stubCanvasContext() {
    const context: Partial<CanvasRenderingContext2D> = {};
    return context as CanvasRenderingContext2D;
}

function stubCanvasRef(p?: Partial<HTMLCanvasElement>) {
    const canvasRef: Partial<HTMLCanvasElement> = {
        clientHeight: 1000,
        clientWidth: 1500,
        offsetLeft: 0,
        offsetTop: 0,
        clientLeft: 0,
        clientTop: 0,
        getContext: () => {
            return stubCanvasContext() as any;
        },
        ...p
    };
    canvasRef['height'] = 100;
    canvasRef['width'] = 150;
    return canvasRef as HTMLCanvasElement;
}

function stubSkillBarController() {
    const controller: Partial<SkillBarController> = {
        selectSkill: () => {}
    }
    return controller as SkillBarController;
}

function stubRender() {
    const render : Partial<BattleRenderController> = {
        skillBarController: stubSkillBarController(),
        clickOnBoard: () => undefined,
        clickOnSkill: () => undefined
    }
    return render as BattleRenderController;
}

function stubBattleContext() {
    const context: Partial<IBattleContext> = {
        battle: {
            board: {entitiesPosition: [] as IEntityPosition[], size: {height: 1, width: 2}},
            id: '',
            entities: []
        },
        player: {
            id: '',
            damage: 1,
            defenseAbsorption: 0.01,
            equips: [],
            healthRadius: 1,
            maxMana: 1,
            skills: [],
            weapon: {damageOnX: DamageDirection.Neutral, damageOnY: DamageDirection.Neutral, name: ''}
        }
    }
    return context as IBattleContext;
}

function stubEntity() {
    const entity: Partial<IEntity> = {};
    return entity as IEntity;
}

function clickOn(p: Partial<MouseEvent>): MouseEvent {
    const ev: MouseEvent = {
        altKey: false,
        button: 0,
        buttons: 0,
        clientX: 0,
        clientY: 0,
        ctrlKey: false,
        metaKey: false,
        movementX: 0,
        movementY: 0,
        offsetX: 0,
        offsetY: 0,
        pageX: 0,
        pageY: 0,
        relatedTarget: null,
        screenX: 0,
        screenY: 0,
        shiftKey: false,
        x: 0,
        y: 0,
        getModifierState: function (keyArg: string): boolean {
            throw new Error("Function not implemented.");
        },
        initMouseEvent: function (typeArg: string, canBubbleArg: boolean, cancelableArg: boolean, viewArg: Window, detailArg: number, screenXArg: number, screenYArg: number, clientXArg: number, clientYArg: number, ctrlKeyArg: boolean, altKeyArg: boolean, shiftKeyArg: boolean, metaKeyArg: boolean, buttonArg: number, relatedTargetArg: EventTarget | null): void {
            throw new Error("Function not implemented.");
        },
        detail: 0,
        view: null,
        which: 0,
        initUIEvent: function (typeArg: string, bubblesArg?: boolean | undefined, cancelableArg?: boolean | undefined, viewArg?: Window | null | undefined, detailArg?: number | undefined): void {
            throw new Error("Function not implemented.");
        },
        bubbles: false,
        cancelBubble: false,
        cancelable: false,
        composed: false,
        currentTarget: null,
        defaultPrevented: false,
        eventPhase: 0,
        isTrusted: false,
        returnValue: false,
        srcElement: null,
        target: null,
        timeStamp: 0,
        type: "",
        composedPath: function (): EventTarget[] {
            throw new Error("Function not implemented.");
        },
        initEvent: function (type: string, bubbles?: boolean | undefined, cancelable?: boolean | undefined): void {
            throw new Error("Function not implemented.");
        },
        preventDefault: function (): void {
            throw new Error("Function not implemented.");
        },
        stopImmediatePropagation: function (): void {
            throw new Error("Function not implemented.");
        },
        stopPropagation: function (): void {
            throw new Error("Function not implemented.");
        },
        NONE: 0,
        CAPTURING_PHASE: 1,
        AT_TARGET: 2,
        BUBBLING_PHASE: 3,
        ...p
    };
    return ev;
}

it('should update the canvas reference size with the client size info', () => {
    const canvasRef = stubCanvasRef();  
    new CanvasController(canvasRef, stubBattleContext(), stubRender);
    expect(canvasRef.height).toBe(canvasRef.clientHeight);
    expect(canvasRef.width).toBe(canvasRef.clientWidth);
});

it('should use the 2d context', () => {
    const canvasRef = stubCanvasRef();
    canvasRef['getContext'] = (context: string) => {
        expect(context).toBe('2d');
        return stubCanvasContext() as any;
    }
    const spyGetContext = jest
        .spyOn<HTMLCanvasElement, 'getContext'>(canvasRef, 'getContext');

    new CanvasController(canvasRef, stubBattleContext(), stubRender);
    expect(spyGetContext).toBeCalledTimes(1);
});

it('should handle onclick events', () => {
    const canvasRef = stubCanvasRef();
    new CanvasController(canvasRef, stubBattleContext(), stubRender);
    const ev = clickOn({});
    expect(() => {
        if (canvasRef.onclick === null) {
            fail('the canvas reference dont have the onclick function seted');
            return;
        }
        canvasRef.onclick(ev)
    }).not.toThrowError();
});

it('sent click to the board', () => {
    const canvasRef = stubCanvasRef();
    const render = stubRender();
    const ev = clickOn({clientX: 987, clientY: 781});
    render.clickOnBoard = (click) => {
        expect(click.x).toBe(ev.clientX);
        expect(click.y).toBe(ev.clientY);
        return undefined;
    };
    const spyClickOnBoard = jest.spyOn(render, 'clickOnBoard');
    new CanvasController(canvasRef, stubBattleContext(), () => render);
    if (canvasRef.onclick === null) {
        fail('the canvas reference dont have the onclick function seted');
        return;
    }
    canvasRef.onclick(ev);
    expect(spyClickOnBoard).toBeCalledTimes(1);
});

it('sent click to the skill bar controller', () => {
    const canvasRef = stubCanvasRef();
    const render = stubRender();
    const ev = clickOn({clientX: 987, clientY: 781});
    render.clickOnSkill = (click) => {
        expect(click.x).toBe(ev.clientX);
        expect(click.y).toBe(ev.clientY);
        return undefined;
    };
    const spyClickOnSkill = jest.spyOn(render, 'clickOnSkill');
    new CanvasController(canvasRef, stubBattleContext(), () => render);
    if (canvasRef.onclick === null) {
        fail('the canvas reference dont have the onclick function seted');
        return;
    }
    canvasRef.onclick(ev);
    expect(spyClickOnSkill).toBeCalledTimes(1);
});

it('the click sent should use canvas offset to adjust', () => {
    const canvasRef = stubCanvasRef({
        offsetLeft: 200, clientLeft: 100, 
        offsetTop: 100, clientTop: 50
    });
    const render = stubRender();
    const ev = clickOn({clientX: 1000, clientY: 500});
    render.clickOnBoard = (click) => {
        expect(click.x).toBe(ev.clientX - (canvasRef.offsetLeft + canvasRef.clientLeft));
        expect(click.y).toBe(ev.clientY - (canvasRef.offsetTop + canvasRef.clientTop));
        return undefined;
    };
    const spyClickOnBoard = jest.spyOn(render, 'clickOnBoard');
    new CanvasController(canvasRef, stubBattleContext(), () => render);
    if (canvasRef.onclick === null) {
        fail('the canvas reference dont have the onclick function seted');
        return;
    }
    canvasRef.onclick(ev);
    expect(spyClickOnBoard).toBeCalledTimes(1);
});

it('should use corrrect key bindings', () => {
    const canvasRef = stubCanvasRef();
    const player = stubEntity();
    const skills: IEntity['skills'] = ['qSkill', 'wSkill', 'eSkill', 'rSkill', 'aSkill', 'sSkill', 'dSkill', 'fSkill']
    const expectedKeyBinding = ['q', 'w', 'e', 'r', 'a', 's', 'd', 'f']
    player['skills'] = skills;
    const spyCreateRender = jest.fn((props: IBattleRenderControllerProps) => {
        for (let index = 0; index < skills.length; index++) {
            const skill = skills[index];
            expect(props.skillKeyBindings[skill]).toBe(expectedKeyBinding[index]);
        }
        return stubRender();
    })
    const battleContext = stubBattleContext();
    battleContext.player = player;
    new CanvasController(canvasRef, battleContext, spyCreateRender);
    expect(spyCreateRender).toBeCalledTimes(1);    
});


it('select the right skill when key pressed correspond to it', () => {
    const canvasRef = stubCanvasRef();
    const player = stubEntity();
    const skills: IEntity['skills'] = ['qSkill']
    const keyBinding = ['q']
    player['skills'] = skills;
    const battleContext = stubBattleContext();
    battleContext.player = player;
    
    const controller = new CanvasController(canvasRef, battleContext, stubRender);
    
    expect(controller.skillSelected).toBeUndefined();
    controller.handleKey(keyBinding[0]);
    expect(controller.skillSelected).toBe(skills[0]);
});

it('when the skill is selected, handle it in the skillbarcontroller', () => {
    const canvasRef = stubCanvasRef();
    const player = stubEntity();
    const skills: IEntity['skills'] = ['qSkill']
    const keyBinding = ['q']
    player['skills'] = skills;
    const battleContext = stubBattleContext();
    battleContext.player = player;

    const skillBarController = stubSkillBarController();
    skillBarController.selectSkill = (skill) => {
        expect(skill).toBe(skills[0]);
    };
    const spySelectSkill = jest.spyOn(skillBarController, 'selectSkill');
    const render = stubRender();
    render.skillBarController = skillBarController;
    
    const controller = new CanvasController(canvasRef, battleContext, () => render);
    controller.handleKey(keyBinding[0]);

    expect(spySelectSkill).toBeCalledTimes(1);
});

