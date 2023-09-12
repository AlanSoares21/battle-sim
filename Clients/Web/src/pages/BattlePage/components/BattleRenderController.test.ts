import { ICanvasWrapper } from "../../../CanvasWrapper";
import { IAsset, IAssetsData, IEntity, IEquip, TBoard, TBoardCoordinates, TCanvasSize, TSize } from "../../../interfaces";
import { mockCanvas } from "../../../jest/helpers"
import BattleRenderController, { ICreateRenders } from "./BattleRenderController"
import { IPlayerRenderProps, PlayerRender } from "./BoardRenderComponents";
import { ILifeSphereRenderProps, IManaBarRenderProps, LifeSphereRender, ManaBarRender } from "./LifeSphereRenderComponents";

function mockPlayerRender() {
    const playerMock: Partial<PlayerRender> = {};
    return playerMock as PlayerRender;
}

function spyCreatePlayerRender(impl?: ICreateRenders['playerRender']) {
    const spy = jest.fn<PlayerRender, Array<IPlayerRenderProps>>();
    if (impl)
        spy.mockImplementation((...args) => impl(args[0]));
    return spy;
}

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

function getDefaultAssets() {
    return {
        'player': {size: {height: 1, width: 2}, start: {x: 0, y: 0}},
        'board-background': {},
        'life-pointer': { size: {height: 1, width: 2}},
        'life-sphere': { }
    } as IAssetsData;
}

function mockCreateRenders() {
    const value: ICreateRenders = {
        playerRender: mockPlayerRender,
        lifeSphere: mockLifeSphereRender,
        manaBar: mockManaBarRender
    }
    return value;
}

it('should create player render with correct properties', () => {
    
    const assets = getDefaultAssets();
    const canvasSize: TCanvasSize = {width: 100, height: 100};
    const canvas = mockCanvas(canvasSize);
    const boardSize: TBoard = {height: 4, width: 4}
    const player = mockEntity('myPlayerId');
    const skillKeysBindings = {};
    const createRenders = mockCreateRenders();

    const startPosition: TBoardCoordinates = {x: 0, y: 0};
    const playerRenderSpy = spyCreatePlayerRender(props => {
        expect(props.name).toBe(player.id)
        expect(props.current).toEqual(startPosition);
        expect(props.asset).toEqual(assets['player']);
        expect(props.board).toEqual(boardSize)
        const boardCanvasSize: TCanvasSize = {
            width: canvasSize.width / 2,
            height: canvasSize.height / 2
        }
        expect(props.cellSize).toEqual({
            width: boardCanvasSize.width / boardSize.width,
            height: boardCanvasSize.height / boardSize.height
        } as TSize);
        return mockPlayerRender();
    });
    createRenders['playerRender'] = playerRenderSpy;
    
    new BattleRenderController({
        canvas, 
        board: boardSize, 
        assetsData: assets, 
        player, 
        skillKeyBindings: skillKeysBindings, 
        createRenders
    }).setPlayer(player, startPosition, true);
    expect(playerRenderSpy).toBeCalledTimes(1);
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
    new BattleRenderController({
        canvas, 
        board: boardSize, 
        assetsData: assets, 
        player, 
        skillKeyBindings: skillKeysBindings, 
        createRenders
    }).setPlayer(player, startPosition, true);
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
    new BattleRenderController({
        canvas, 
        board: boardSize, 
        assetsData: assets, 
        player, 
        skillKeyBindings: skillKeysBindings, 
        createRenders
    }).setPlayer(player, startPosition, true);
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
    const updateManaBarMock = mockManaBarUpdate(value => {
        expect(value).toBe(newValue);
    });

    const manaBarMock = mockManaBarRender();
    manaBarMock['updateCurrentValue'] = updateManaBarMock;
    createRenders['manaBar'] = () => manaBarMock;

    const startPosition: TBoardCoordinates = {x: 0, y: 0};
    const controller = new BattleRenderController({
        canvas, 
        board: boardSize, 
        assetsData: assets, 
        player, 
        skillKeyBindings: skillKeysBindings, 
        createRenders
    });
    controller.setPlayer(player, startPosition, true);
    controller.updateMana(newValue);

    expect(updateManaBarMock).toBeCalledTimes(1);
});