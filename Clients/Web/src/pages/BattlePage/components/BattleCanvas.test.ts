import { DamageDirection, IBattleData, IBoardData, IEntity, IEntityPosition, TCoordinates } from "../../../interfaces";
import { stubIt } from "../../../jest/helpers";
import { ServerConnection } from "../../../server";
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
        selectSkill: () => {},
        unSelectSkill: () => {}
    }
    return controller as SkillBarController;
}

function stubRender(p?: Partial<BattleRenderController>) {
    const render : Partial<BattleRenderController> = {
        skillBarController: stubSkillBarController(),
        clickOnBoard: () => undefined,
        clickOnSkill: () => undefined,
        ...p
    }
    return render as BattleRenderController;
}

function stubServer() {
    const server: Partial<ServerConnection> = {
        Skill: () => {}
    }
    return server as ServerConnection;
}

function stubBattleContext(p?: Partial<IBattleContext>) {
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
        },
        server: stubServer(),
        ...p
    }
    return context as IBattleContext;
}

function stubEntity(p?: Partial<IEntity>) {
    const entity: Partial<IEntity> = {...p};
    return entity as IEntity;
}

function stubBoard(p?: Partial<IBoardData>) {
    const stub = stubIt<IBoardData>({...p});
    return stub as IBoardData;
}

function stubBattleData(p?: Partial<IBattleData>) {
    const stub = stubIt<IBattleData>({...p});
    return stub as IBattleData;
}

function clickOn(p?: Partial<MouseEvent>): MouseEvent {
    const ev: Partial<MouseEvent> = {
        clientX: 0,
        clientY: 0,
        movementX: 0,
        movementY: 0,
        offsetX: 0,
        offsetY: 0,
        pageX: 0,
        pageY: 0,
        screenX: 0,
        screenY: 0,
        x: 0,
        y: 0,
        ...p
    };
    return ev as MouseEvent;
}

it('should update the canvas reference size with the client size info', () => {
    const canvasRef = stubCanvasRef();  
    new CanvasController(canvasRef, stubBattleContext(), () => stubRender());
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

    new CanvasController(canvasRef, stubBattleContext(), () => stubRender());
    expect(spyGetContext).toBeCalledTimes(1);
});

it('should handle onclick events', () => {
    const canvasRef = stubCanvasRef();
    new CanvasController(canvasRef, stubBattleContext(), () => stubRender());
    const ev = clickOn({});
    expect(() => {
        if (canvasRef.onclick === null) {
            fail('the canvas reference dont have the onclick function seted');
            return;
        }
        canvasRef.onclick(ev)
    }).not.toThrowError();
});

it('send click to the board', () => {
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

it('send click to the skill bar controller', () => {
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

it('when click in a skill, select it', () => {
    const skill = 'qSkill';
    const player = stubEntity({skills: [skill]});
    const canvasRef = stubCanvasRef();
    const skillBarController = stubSkillBarController();
    skillBarController.selectSkill = name => {
        expect(name).toBe(skill);
    }
    const spySelectSkill = jest.spyOn(skillBarController, 'selectSkill');
    
    const controller = new CanvasController(
        canvasRef, 
        stubBattleContext({player}), 
        () => stubRender({clickOnSkill: () => skill, skillBarController})
    );

    if (canvasRef.onclick === null) {
        fail('the canvas reference dont have the onclick function seted');
        return;
    }
    expect(controller.skillSelected).toBeUndefined();
    canvasRef.onclick(clickOn());
    expect(controller.skillSelected).toBe(skill);
    expect(spySelectSkill).toBeCalledTimes(1);
});

it('when click in the selected skill, unselect it', () => {
    const skill = 'qSkill';
    const player = stubEntity({skills: [skill]});
    const canvasRef = stubCanvasRef();
    const skillBarController = stubSkillBarController();
    skillBarController.selectSkill = name => {
        expect(name).toBe(skill);
    }
    skillBarController.unSelectSkill = () => {}
    const spySelectSkill = jest.spyOn(skillBarController, 'selectSkill');
    const spyUnSelectSkill = jest.spyOn(skillBarController, 'unSelectSkill');
    
    const controller = new CanvasController(
        canvasRef, 
        stubBattleContext({player}), 
        () => stubRender({clickOnSkill: () => skill, skillBarController})
    );

    if (canvasRef.onclick === null) {
        fail('the canvas reference dont have the onclick function seted');
        return;
    }

    canvasRef.onclick(clickOn());
    expect(controller.skillSelected).toBe(skill);
    
    canvasRef.onclick(clickOn());
    expect(controller.skillSelected).toBeUndefined();
    
    expect(spySelectSkill).toBeCalledTimes(1);
    expect(spyUnSelectSkill).toBeCalledTimes(1);
});

it('the click should use canvas offset to adjust', () => {
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

it('when click on board, should send movement call to server', () => {
    const canvasRef = stubCanvasRef();
    const render = stubRender();
    const ev = clickOn({});
    const moveTo: TCoordinates = {x: 0, y: 0};
    render.clickOnBoard = () => moveTo;
    const server = stubServer();
    server.Move = (x, y) => {
        expect(x).toBe(moveTo.x);
        expect(y).toBe(moveTo.y);
    }
    const spyMove = jest.spyOn(server, 'Move');
    new CanvasController(canvasRef, stubBattleContext({server}), () => render);
    if (canvasRef.onclick === null) {
        fail('the canvas reference dont have the onclick function seted');
        return;
    }
    canvasRef.onclick(ev);
    expect(spyMove).toBeCalledTimes(1);
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
    
    const controller = new CanvasController(canvasRef, battleContext, () => stubRender());
    
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

it('when click on a player and have a skill selected, should send a skill call to the server', () => {
    const skillTarget = 'skillTargetId';
    const canvasRef = stubCanvasRef();
    const render = stubRender();
    const boardClick: TCoordinates = {x: 0, y: 0};
    const battle = stubBattleData({
        board: stubBoard({
            entitiesPosition: [{entityIdentifier: skillTarget, ...boardClick}]
        })
    });
    const player = stubEntity();
    player.skills = ['qSkill'];
    render.clickOnBoard = () => boardClick;
    const server = stubServer();
    server.Skill = (skill, target) => {
        expect(skill).toBe(player.skills[0]);
        expect(target).toBe(skillTarget);
    }
    const spySkill = jest.spyOn(server, 'Skill');
    const controller = new CanvasController(
        canvasRef, 
        stubBattleContext({server, player, battle}), 
        () => render
    );
    controller.handleKey('q');
    if (canvasRef.onclick === null) {
        fail('the canvas reference dont have the onclick function seted');
        return;
    }
    canvasRef.onclick(clickOn());
    expect(spySkill).toBeCalledTimes(1);
});

it('after use a skill, unselect the skill', () => {
    const boardClick: TCoordinates = {x: 0, y: 0};
    const canvasRef = stubCanvasRef();
    const player = stubEntity({skills: ['qSkill']});
    
    const battle = stubBattleData({
        board: stubBoard({
            entitiesPosition: [{entityIdentifier: 'target', ...boardClick}]
        })
    });

    const skillBarController = stubSkillBarController();
    skillBarController.unSelectSkill = () => {};
    const spyUnSelectSkill = jest.spyOn(skillBarController, 'unSelectSkill');
    
    const controller = new CanvasController(
        canvasRef, 
        stubBattleContext({player, battle}), 
        () => stubRender({
            clickOnBoard: () => boardClick,
            skillBarController
        })
    );
    controller.handleKey('q');
    if (canvasRef.onclick === null) {
        fail('the canvas reference dont have the onclick function seted');
        return;
    }
    canvasRef.onclick(clickOn());
    expect(controller.skillSelected).toBeUndefined();
    expect(spyUnSelectSkill).toBeCalledTimes(1);
});