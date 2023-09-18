import { ICanvasWrapper } from "../../../CanvasWrapper";
import { IAsset, TGameAssets, IEntity, IEquip, TBoard, TBoardCoordinates, TCanvasSize, TSize } from "../../../interfaces";
import { mockCanvas, stubAsset, stubBoardController, stubEntity, stubIt } from "../../../jest/helpers"
import BattleRenderController, { IBattleRenderControllerProps, IControllerFactory, ICreateRenders } from "./BattleRenderController"
import { BoardController } from "./BoardController";
import { createEntityFactory } from "./EntityRenders";
import { ILifeSphereRenderProps, IManaBarRenderProps, LifeSphereRender, ManaBarRender } from "./LifeSphereRenderComponents";
import { createPointerRender } from "./PointerRender";

function mockLifeSphereRender() {
    const mock: Partial<LifeSphereRender> = {};
    return mock as LifeSphereRender;
}

function spyCreateLifeSphereRender(impl?: ICreateRenders['lifeSphere']) {
    const spy = jest.fn<LifeSphereRender, Array<ILifeSphereRenderProps>>();
    if (impl)
        spy.mockImplementation((...args) => impl(args[0]));
    return spy;
}

function mockManaBarRender() {
    const mock: Partial<ManaBarRender> = {};
    return mock as ManaBarRender;
}

function spyCreateManaBarRender(impl?: ICreateRenders['manaBar']) {
    const spy = jest.fn<ManaBarRender, Array<IManaBarRenderProps>>();
    if (impl)
        spy.mockImplementation((...args) => impl(args[0]));
    return spy;
}

function mockManaBarUpdate(impl: ManaBarRender['updateCurrentValue']) {
    const mockObj = jest.fn(impl);
    return mockObj;
}

function mockEntity(id?: IEntity['id']) {
    const value: Partial<IEntity> = {
        id, 
        skills: [] as string[], 
        equips: [] as IEquip[]
    };
    return value as IEntity;
}

function getDefaultAssets(): TGameAssets {
    return {
        'player': stubAsset({size: {height: 1, width: 2}, start: {x: 0, y: 0}}),
        'board-background': stubAsset(),
        'life-pointer': stubAsset({size: {height: 1, width: 2}}),
        'life-sphere': stubAsset()
    };
}

function mockCreateRenders() {
    const value: ICreateRenders = {
        lifeSphere: mockLifeSphereRender,
        manaBar: mockManaBarRender
    }
    return value;
}

function stubControllersFactory(p?: Partial<IControllerFactory>) {
    const factory = stubIt<IControllerFactory>({
        boardController: () => stubBoardController(),
        ...p
    });
    return factory;
}

function getProperties(p?: Partial<IBattleRenderControllerProps>) {
    const props = stubIt<IBattleRenderControllerProps>({
        canvas: mockCanvas({height: 10, width: 10}),
        player: mockEntity(),
        assetsData: getDefaultAssets(),
        createRenders: mockCreateRenders(),
        createController: stubControllersFactory(),
        ...p
    });
    return props;
}

it('should create board controller with correct properties', () => {
    
    const assets = getDefaultAssets();
    const canvasSize: TCanvasSize = {width: 1000, height: 500};
    const canvas = mockCanvas(canvasSize);
    const boardSize: TBoard = {height: 4, width: 4}
    const player = mockEntity('myPlayerId');

    const createController = stubIt<IControllerFactory>();
    createController.boardController = props => {
        expect(props.assets).toBe(assets);
        expect(props.canvas.wrapper).toBe(canvas);
        expect(props.canvas.startAt).toEqual({x: 200, y: 50});
        expect(props.canvas.boarCanvasSize)
            .toEqual({width: 500, height: 250});
        expect(props.board).toEqual(boardSize);
        expect(props.playerId).toBe(player.id);
        expect(props.renderFactory.entity).toBe(createEntityFactory);
        expect(props.renderFactory.pointer).toBe(createPointerRender);
        return stubIt<BoardController>();
    }

    const spyCreateBoardController = jest.spyOn(createController, 'boardController');
    
    new BattleRenderController(getProperties({
        canvas, 
        board: boardSize, 
        assetsData: assets, 
        player,
        createController
    }));

    expect(spyCreateBoardController).toBeCalledTimes(1);
});

it('when render, should call board controller render method', () => {
    const controller = stubBoardController();

    const spyRender = jest.spyOn(controller, 'render');
    
    new BattleRenderController(getProperties({
        createController: stubControllersFactory({
            boardController: () => controller
        })
    })).render();

    expect(spyRender).toBeCalledTimes(1);
});

it('should call board controller addEntity when add a new player', () => {
    const player = mockEntity('myPlayerId');
    const playerPostition: TBoardCoordinates = {x: 1, y: 2};

    const boardController = stubBoardController({
        addEntity(entity, position) {
            expect(entity).toEqual(player);
            expect(position).toEqual(playerPostition);
        }
    });
    const spyAddEntity = jest.spyOn(boardController, 'addEntity');
    
    new BattleRenderController(getProperties({
        createController: stubControllersFactory({
            boardController: () => boardController
        })
    })).setPlayer(player, playerPostition, false);

    expect(spyAddEntity).toBeCalledTimes(1);
});

it('should create player life sphere', () => {
    const assets = getDefaultAssets();
    const canvasSize: TCanvasSize = {width: 100, height: 100};
    const canvas = mockCanvas(canvasSize);
    const boardSize: TBoard = {height: 4, width: 4}
    const player = mockEntity('myPlayerId');
    player['healthRadius'] = 100;
    const skillKeysBindings = {};
    const createRenders = mockCreateRenders();

    const createLifeSphereSpy = spyCreateLifeSphereRender((props) => {
        const lifeSphereSide = canvasSize.width / 5;
        expect(props.canvas.getSize())
        .toEqual({width: lifeSphereSide, height: lifeSphereSide} as TCanvasSize);
        expect(props.healthRadiusInScale).toBe(player.healthRadius * 2 / lifeSphereSide);
        expect(props.asset).toEqual(assets['life-sphere']);
        return mockLifeSphereRender();
    });
    createRenders['lifeSphere'] = createLifeSphereSpy;
    
    const startPosition: TBoardCoordinates = {x: 0, y: 0};
    new BattleRenderController(getProperties({
        canvas, 
        board: boardSize, 
        assetsData: assets, 
        player, 
        skillKeyBindings: skillKeysBindings, 
        createRenders
    })).setPlayer(player, startPosition, true);
    expect(createLifeSphereSpy).toBeCalledTimes(1);
});

it('should create mana bar', () => {
    const assets = getDefaultAssets();
    const canvasSize: TCanvasSize = {width: 100, height: 100};
    const canvas = mockCanvas(canvasSize);
    const boardSize: TBoard = {height: 4, width: 4}
    const player = mockEntity('myPlayerId');
    const skillKeysBindings = {};
    const createRenders = mockCreateRenders();

    const createManaBarSpy = spyCreateManaBarRender(props => {
        expect(props.canvas.getSize())
        .toEqual({width: canvasSize.width / 5, height: 25} as TCanvasSize);
        return mockManaBarRender();
    });
    createRenders['manaBar'] = createManaBarSpy;

    const startPosition: TBoardCoordinates = {x: 0, y: 0};
    new BattleRenderController(getProperties({
        canvas, 
        board: boardSize, 
        assetsData: assets, 
        player, 
        skillKeyBindings: skillKeysBindings, 
        createRenders
    })).setPlayer(player, startPosition, true);
    expect(createManaBarSpy).toBeCalledTimes(1);
});

it('should update mana bar current value', () => {
    const assets = getDefaultAssets();
    const canvasSize: TCanvasSize = {width: 100, height: 100};
    const canvas = mockCanvas(canvasSize);
    const boardSize: TBoard = {height: 4, width: 4}
    const player = mockEntity('myPlayerId');
    const skillKeysBindings = {};
    const createRenders = mockCreateRenders();

    const newValue = 123;
    const newMax = 1234;
    const updateManaBarMock = mockManaBarUpdate((currentMana, currentMaxMana) => {
        expect(currentMana).toBe(newValue);
        expect(currentMaxMana).toBe(newMax);
    });

    const manaBarMock = mockManaBarRender();
    manaBarMock['updateCurrentValue'] = updateManaBarMock;
    createRenders['manaBar'] = () => manaBarMock;

    const startPosition: TBoardCoordinates = {x: 0, y: 0};
    const controller = new BattleRenderController(getProperties({
        canvas, 
        board: boardSize, 
        assetsData: assets, 
        player, 
        skillKeyBindings: skillKeysBindings, 
        createRenders
    }));
    controller.setPlayer(player, startPosition, true);
    controller.updateMana(newValue, newMax);

    expect(updateManaBarMock).toBeCalledTimes(1);
});