import { DamageDirection, IBattleData, IBoardData, IEntity, IEntityPosition, TBoardCoordinates, TCoordinates } from "../../../interfaces";
import { stubEntity, stubIt } from "../../../jest/helpers";
import { IServerEvents, ServerConnection } from "../../../server";
import { IBattleContext } from "../BattleContext";
import { CanvasController } from "./BattleCanvas"
import BattleRenderController, { IBattleRenderControllerProps } from "./BattleRenderController";
import { PointerRender } from "./BoardRenderComponents";
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

function stubPointer(p?: Partial<PointerRender>) {
    const pointer = stubIt<PointerRender>({
        setPosition: () => {},
        ...p
    })
    return pointer;
}

function stubRender(p?: Partial<BattleRenderController>) {
    const render : Partial<BattleRenderController> = {
        skillBarController: stubSkillBarController(),
        pointer: stubPointer(),
        setPlayer: () => {},
        clickOnBoard: () => undefined,
        clickOnSkill: () => undefined,
        updateMana: () => {},
        updateEntityCurrentHealth: () => {},
        ...p
    }
    return render as BattleRenderController;
}

function stubServer() {
    const server: Partial<ServerConnection> = {
        Move: () => {},
        Skill: () => {},
        onEntitiesMove: () => server as ServerConnection,
        onSkill: () => server as ServerConnection,
        onManaRecovered: () => server as ServerConnection
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

function stubBoard(p?: Partial<IBoardData>) {
    const stub = stubIt<IBoardData>({
        entitiesPosition: [],
        ...p
    });
    return stub as IBoardData;
}

function stubBattleData(p?: Partial<IBattleData>) {
    const stub = stubIt<IBattleData>({
        board: stubBoard(),
        entities: [],
        ...p
    });
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

it('when click on board, should set pointer', () => {
    const canvasRef = stubCanvasRef();
    const ev = clickOn({});
    const moveTo: TCoordinates = {x: 0, y: 0};
    const pointer = stubPointer({
        setPosition(cell) {
            expect(cell).toEqual(moveTo);
        }
    })
    const spySetPosition = jest.spyOn(pointer, 'setPosition');
    new CanvasController(
        canvasRef, 
        stubBattleContext(), 
        () => stubRender({
            pointer,
            clickOnBoard: () => moveTo
        })
    );
    if (canvasRef.onclick === null) {
        fail('the canvas reference dont have the onclick function seted');
        return;
    }
    canvasRef.onclick(ev);
    expect(spySetPosition).toBeCalledTimes(1);
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

it('when click in a player and have a skill selected, should send a skill call to the server', () => {
    const skill = 'skillName';
    const canvasRef = stubCanvasRef();
    const boardClick: TCoordinates = {x: 1, y: 1};
    const target = stubEntity({id: 'targetId'});
    const targetPosition: IEntityPosition = {
        entityIdentifier: target.id, 
        ...boardClick
    }
    const player = stubEntity({id: 'playerId'});
    const playerPosition: IEntityPosition = {
        entityIdentifier: player.id, 
        x: 0, y: 1
    }
    const render = stubRender({clickOnBoard: () => boardClick});
    
    const server = stubServer();
    server.Skill = (skill, targetId) => {
        expect(skill).toBe(skill);
        expect(targetId).toBe(target.id);
    }
    const spySkill = jest.spyOn(server, 'Skill');
    
    const controller = new CanvasController(
        canvasRef, 
        stubBattleContext({
            server, 
            player
        }), 
        () => render
    );
    controller.skillSelected = skill;
    controller.state.positions[0] = targetPosition;
    controller.state.positions[1] = playerPosition;
    controller.state.currentPlayer.mana = 5;

    if (canvasRef.onclick === null) {
        fail('the canvas reference dont have the onclick function seted');
        return;
    }
    
    canvasRef.onclick(clickOn());
    expect(spySkill).toBeCalledTimes(1);
});

it('after use a skill, unselect the skill', () => {
    const skill = 'skillName';
    const canvasRef = stubCanvasRef();
    const boardClick: TCoordinates = {x: 1, y: 1};
    const target = stubEntity({id: 'targetId'});
    const targetPosition: IEntityPosition = {
        entityIdentifier: target.id, 
        ...boardClick
    }
    const player = stubEntity({id: 'playerId'});
    const playerPosition: IEntityPosition = {
        entityIdentifier: player.id, 
        x: 0, y: 1
    }

    const server = stubServer();
    const spySkill = jest.spyOn(server, 'Skill');

    const skillBarController = stubSkillBarController();
    skillBarController.unSelectSkill = () => {};
    const spyUnSelectSkill = jest.spyOn(skillBarController, 'unSelectSkill');
    
    const controller = new CanvasController(
        canvasRef, 
        stubBattleContext({player, server}), 
        () => stubRender({
            clickOnBoard: () => boardClick,
            skillBarController
        })
    );
    controller.skillSelected = skill;
    controller.state.currentPlayer.mana = 5;
    controller.state.positions[0] = targetPosition;
    controller.state.positions[1] = playerPosition;

    if (canvasRef.onclick === null) {
        fail('the canvas reference dont have the onclick function seted');
        return;
    }

    canvasRef.onclick(clickOn());
    expect(spySkill).toBeCalledTimes(1);
    expect(controller.skillSelected).toBeUndefined();
    expect(spyUnSelectSkill).toBeCalledTimes(1);
});

it('dont use skill when dont have mana enough', () => {
    const canvasRef = stubCanvasRef();
    const skillNameUsedByThePlayer = "basicNegativeDamageOnX"
    const player = stubEntity({
        id: "playerId", 
        maxMana: 10,
        skills: [skillNameUsedByThePlayer]
    });
    const playerPosition: TBoardCoordinates = {x: 0, y:0};
    const board = stubBoard({
        entitiesPosition: [{entityIdentifier: player.id, ...playerPosition}]
    })

    const server = stubServer();
    const spySkill = jest.spyOn(server, 'Skill');

    const skillBarController = stubSkillBarController();
    skillBarController.unSelectSkill = () => {};
    const spyUnSelectSkill = jest.spyOn(skillBarController, 'unSelectSkill');

    const controller = new CanvasController(
        canvasRef, 
        stubBattleContext({server, player, battle: stubBattleData({board})}), 
        () => stubRender({
            clickOnBoard: () => playerPosition,
            skillBarController
        })
    );

    if (canvasRef['onclick'] === null) {
        fail(`The canvas ref onclick function is null`);
        return;
    }

    controller.skillSelected = skillNameUsedByThePlayer;
    controller.state.currentPlayer.mana = 4;
    canvasRef['onclick'](clickOn());

    expect(spySkill).toBeCalledTimes(0);
    expect(controller.skillSelected).toBeUndefined();
    expect(spyUnSelectSkill).toBeCalledTimes(1);
});

it('dont use skill when the target is out of the range', () => {
    const canvasRef = stubCanvasRef();
    const skillNameUsedByThePlayer = "basicNegativeDamageOnX"
    const player = stubEntity({
        id: "playerId", 
        maxMana: 1,
        skills: [skillNameUsedByThePlayer]
    });
    const playerPosition: IEntityPosition = {
        entityIdentifier: player.id,
        x: 0, y:0
    }
    const targetCell: TBoardCoordinates = {x: 999, y: 999};
    const targetPosition: IEntityPosition = {
        entityIdentifier: 'target',
        ...targetCell
    }
    const board = stubBoard({
        entitiesPosition: [playerPosition, targetPosition]
    })

    const server = stubServer();
    const spySkill = jest.spyOn(server, 'Skill');

    const skillBarController = stubSkillBarController();
    skillBarController.unSelectSkill = () => {};
    const spyUnSelectSkill = jest.spyOn(skillBarController, 'unSelectSkill');

    const controller = new CanvasController(
        canvasRef, 
        stubBattleContext({server, player, battle: stubBattleData({board})}), 
        () => stubRender({
            clickOnBoard: () => targetCell,
            skillBarController
        })
    );

    if (canvasRef['onclick'] === null) {
        fail(`The canvas ref onclick function is null`);
        return;
    }

    controller.skillSelected = skillNameUsedByThePlayer;
    controller.state.currentPlayer.mana = 5;
    canvasRef['onclick'](clickOn());
    
    expect(spySkill).toBeCalledTimes(0);
    expect(controller.skillSelected).toBeUndefined();
    expect(spyUnSelectSkill).toBeCalledTimes(1);
});

it('render entities when the battle starts', () => {
    const player = stubEntity({id: 'playerId'});
    const playerPosition: TBoardCoordinates = {x: 0, y: 0};
    const enemyPosition: TBoardCoordinates = {x: 1, y: 1};
    const enemy = stubEntity({id: 'enemyId'});
    const battle = stubBattleData({
        board: stubBoard({
            entitiesPosition: [
                {entityIdentifier: player.id, ...playerPosition},
                {entityIdentifier: enemy.id, ...enemyPosition}
            ]
        }),
        entities: [enemy, player]
    });
    let userChecked = false;
    let enemyChecked = false;
    const render = stubRender({
        setPlayer(data, position, isTheUser) {
            if (isTheUser) {
                expect(data.id).toBe(player.id);
                expect(position.x).toBe(playerPosition.x);
                expect(position.y).toBe(playerPosition.y);
                userChecked = true;
            } else {
                expect(data.id).toBe(enemy.id);
                expect(position.x).toBe(enemyPosition.x);
                expect(position.y).toBe(enemyPosition.y);
                enemyChecked = true;
            }
        },
    });
    const spySetPlayer = jest.spyOn(render, 'setPlayer');

    new CanvasController(
        stubCanvasRef(), 
        stubBattleContext({battle, player}), 
        () => render
    );

    expect(spySetPlayer).toBeCalledTimes(2);
    expect(userChecked).toBeTruthy();
    expect(enemyChecked).toBeTruthy();
})

it('updates the board when a movement happens', () => {
    const entityid = 'entitieMoved';
    const player = stubEntity({id: entityid});
    const playerPosition: IEntityPosition = 
        {entityIdentifier: player.id, x: 0, y: 0};
    const server = stubServer();
    let entitiesMoveListener: IServerEvents['EntitiesMove'] | undefined;
    server.onEntitiesMove = listener => {
        entitiesMoveListener = listener;
        return server;
    }

    const entitiesMoves = {[entityid]: {x: 1, y:1}};
    /**
     * this function is called two times,
     * the first one is in the initial setup and will have the
     * player initial position,
     * the second will be the event from the server
     */
    let isTheFirstRender = true;
    const render = stubRender({
        setPlayer(data, position, isTheUser) {
            if (isTheFirstRender) {
                isTheFirstRender = false;
                return;
            }
            expect(data.id).toBe(entityid);
            expect(isTheUser).toBeTruthy();
            expect(position).toEqual(entitiesMoves[entityid]);
        },
    });
    const spySetPlayer = jest.spyOn(render, 'setPlayer');

    const controller = new CanvasController(
        stubCanvasRef(), 
        stubBattleContext({
            server, 
            battle: stubBattleData({
                entities: [player], 
                board: stubBoard({entitiesPosition: [playerPosition]})
            }), 
            player
        }), 
        () => render
    );

    if (entitiesMoveListener === undefined) {
        fail(`The listener to the entities move event is undefined`);
        return;
    }
    entitiesMoveListener(entitiesMoves);
    expect(spySetPlayer).toBeCalledTimes(2);
})

it('updates the entities position when a movement happens', () => {
    const player = stubEntity({id: 'playerid'});
    const initialPlayerPosition: IEntityPosition = {
        entityIdentifier: player.id, 
        x: 0, y: 0
    };
    const battle = stubBattleData({
        board: stubBoard({
            entitiesPosition: [initialPlayerPosition]
        }),
        entities: [player]
    });
    const server = stubServer();
    let entitiesMoveListener: IServerEvents['EntitiesMove'] | undefined;
    server.onEntitiesMove = listener => {
        entitiesMoveListener = listener;
        return server;
    }

    const entitiesMoves = {[player.id]: {x: 1, y:1}};

    const controller = new CanvasController(
        stubCanvasRef(), 
        stubBattleContext({server, battle, player}), 
        () => stubRender()
    );

    if (entitiesMoveListener === undefined) {
        fail(`The listener to the entities move event is undefined`);
        return;
    }
    expect(controller.state.positions[0]).toEqual(initialPlayerPosition);
    entitiesMoveListener(entitiesMoves);
    const expectedPlayerPosition: IEntityPosition = {
        entityIdentifier: player.id,
        ...entitiesMoves[player.id]
    }
    expect(controller.state.positions[0]).toEqual(expectedPlayerPosition);
})

it('updates the user life when a skill event happens and he is the target', () => {
    const playerId = 'playerId';
    const currentHealth: TCoordinates = {x: 123, y: 321};
    const player = stubEntity({id: playerId});
    const server = stubServer();
    let skillEventListener: IServerEvents['Skill'] | undefined;
    server.onSkill = listener => {
        skillEventListener = listener;
        return server;
    }

    const render = stubRender({
        updateEntityCurrentHealth(isTheUser, health) {
            expect(isTheUser).toBeTruthy();
            expect(health).toEqual(currentHealth);   
        }
    });
    const spyUpdateCurrentHealth = jest.spyOn(render, 'updateEntityCurrentHealth');

    const controller = new CanvasController(
        stubCanvasRef(), 
        stubBattleContext({server, player}), 
        () => render
    );

    if (skillEventListener === undefined) {
        fail(`The listener to the skill event is undefined`);
        return;
    }
    skillEventListener('someSkillName', 'someSource', playerId, currentHealth);
    expect(spyUpdateCurrentHealth).toBeCalledTimes(1);
});

it('updates the target life when a skill event happens and he is the target', () => {
    const targetId = 'targetId';
    const currentHealth: TCoordinates = {x: 123, y: 321};
    const player = stubEntity({id: "playerId"});
    const server = stubServer();
    let skillEventListener: IServerEvents['Skill'] | undefined;
    server.onSkill = listener => {
        skillEventListener = listener;
        return server;
    }

    const render = stubRender({
        updateEntityCurrentHealth(isTheUser, health) {
            expect(isTheUser).toBeFalsy();
            expect(health).toEqual(currentHealth);   
        }
    });
    const spyUpdateCurrentHealth = jest.spyOn(render, 'updateEntityCurrentHealth');

    const controller = new CanvasController(
        stubCanvasRef(), 
        stubBattleContext({server, player}), 
        () => render
    );

    if (skillEventListener === undefined) {
        fail(`The listener to the skill event is undefined`);
        return;
    }
    skillEventListener('someSkillName', 'someSource', targetId, currentHealth);
    expect(spyUpdateCurrentHealth).toBeCalledTimes(1);
});

it('add 5 units in the mana bar when a mana recover event happens', () => {
    const player = stubEntity({
        id: "playerId", 
        maxMana: 10
    });
    const server = stubServer();
    let manaRecoveredEventListener
        : IServerEvents['ManaRecovered'] | undefined;
    server.onManaRecovered = listener => {
        manaRecoveredEventListener = listener;
        return server;
    }
    let mana = 0;
    const render = stubRender({
        updateMana(value, maxMana) {
            expect(value).toBe(mana + 5);
            expect(maxMana).toBe(player.maxMana);
            mana = value;
        },
    });
    const spyUpdateMana = jest.spyOn(render, 'updateMana');

    const controller = new CanvasController(
        stubCanvasRef(), 
        stubBattleContext({server, player}), 
        () => render
    );

    if (manaRecoveredEventListener === undefined) {
        fail(`The listener to the mana recover event is undefined`);
        return;
    }
    manaRecoveredEventListener();
    manaRecoveredEventListener();
    expect(spyUpdateMana).toBeCalledTimes(2);
});

it('dont add mana when a mana recover event happens and the user mana is at limit ', () => {
    const player = stubEntity({
        id: "playerId", 
        maxMana: 10
    });
    const server = stubServer();
    let manaRecoveredEventListener
        : IServerEvents['ManaRecovered'] | undefined;
    server.onManaRecovered = listener => {
        manaRecoveredEventListener = listener;
        return server;
    }

    const render = stubRender();
    const spyUpdateMana = jest.spyOn(render, 'updateMana');

    const controller = new CanvasController(
        stubCanvasRef(), 
        stubBattleContext({server, player}), 
        () => render
    );

    if (manaRecoveredEventListener === undefined) {
        fail(`The listener to the mana recover event is undefined`);
        return;
    }
    manaRecoveredEventListener();
    manaRecoveredEventListener();
    manaRecoveredEventListener();
    expect(spyUpdateMana).toBeCalledTimes(2);
});

it('decrease mana when a skill is used by the player', () => {
    const skillName = "basicNegativeDamageOnX"
    const player = stubEntity({
        id: "playerId", 
        maxMana: 5,
        skills: [skillName]
    });

    const server = stubServer();
    let manaRecoveredEventListener
        : IServerEvents['ManaRecovered'] | undefined;
    server.onManaRecovered = listener => {
        manaRecoveredEventListener = listener;
        return server;
    }
    let skillEventListener
        : IServerEvents['Skill'] | undefined;
    server.onSkill = listener => {
        skillEventListener = listener;
        return server;
    }

    let lastManaValue = -1;
    const render = stubRender({
        updateMana(value) {
            lastManaValue = value;
        }
    });
    const spyUpdateMana = jest.spyOn(render, 'updateMana');

    const controller = new CanvasController(
        stubCanvasRef(), 
        stubBattleContext({server, player}), 
        () => render
    );

    if (skillEventListener === undefined) {
        fail(`The listener to the skill event is undefined`);
        return;
    }
    if (manaRecoveredEventListener === undefined) {
        fail(`The listener to the mana recover event is undefined`);
        return;
    }

    manaRecoveredEventListener();
    skillEventListener(skillName, player.id, 'someOne', {x: 0, y: 0});
    expect(spyUpdateMana).toBeCalledTimes(2);
    expect(lastManaValue).toBe(0);
});