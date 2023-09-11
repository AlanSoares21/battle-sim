import { IAssetsData, IEntity, IEquip, TBoard, TBoardCoordinates, TCanvasSize, TSize } from "../../../interfaces";
import { mockCanvas } from "../../../jest/helpers"
import BattleRenderController, { ICreateRenders } from "./BattleRenderController"
import { IPlayerRenderProps, PlayerRender } from "./BoardRenderComponents";

function mockPlayerRender() {
    const playerMock: Partial<PlayerRender> = {};
    return playerMock as PlayerRender;
}

function spyPlayerRender(impl?: ICreateRenders['playerRender']) {
    const spy = jest.fn<PlayerRender, Array<IPlayerRenderProps>>();
    if (impl)
        spy.mockImplementation((...args) => impl(args[0]));
    return spy;
}

function mockEntity(id?: IEntity['id']) {
    const value: Partial<IEntity> = {
        id, 
        skills: [] as string[], 
        equips: [] as IEquip[]
    };
    return value as IEntity;
}

it('should create player render with correct properties', () => {
    
    const assets = {
        'player': {size: {height: 1, width: 2}, start: {x: 0, y: 0}},
        'board-background': {},
        'life-pointer': { size: {height: 1, width: 2}}
    } as IAssetsData;
    const canvasSize: TCanvasSize = {width: 100, height: 100};
    const canvas = mockCanvas(canvasSize);
    const boardSize: TBoard = {height: 4, width: 4}
    const player = mockEntity('myPlayerId');
    const skillKeysBindings = {};
    const startPosition: TBoardCoordinates = {x: 0, y: 0};
    const playerRenderSpy = spyPlayerRender(props => {
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
    const controller = new BattleRenderController(
        canvas, 
        boardSize, 
        assets, 
        player, 
        skillKeysBindings, 
        {
            playerRender: playerRenderSpy,
        }
    );
    controller.setPlayer(player, startPosition, true);
    expect(playerRenderSpy).toBeCalledTimes(1);
})