
/**
 * recebe informações do board
 * 
 * posiciona entidades no board.
 * atualiza a posição de entidades no board.
 */

import { IAsset, IEntity, TBoard, TBoardCoordinates, TCanvasCoordinates, TCanvasSize, TGameAssets, TSize } from "../../../interfaces";
import { mockCanvas, stubEntity, stubIt } from "../../../jest/helpers";
import { BoardController, IBoardControllerProps, IBoardItemRender } from "./BoardController";
import { PointerRender } from "./PointerRender";

function stubImage(p?: Partial<ImageBitmap>) {
    const image = stubIt<ImageBitmap>(p);
    return image;
}

function stubAsset(p?: Partial<IAsset>) {
    const asset = stubIt<IAsset>({
        image: stubImage(),
        ...p
    });
    return asset;
}

function stubBoardCanvas(p?: Partial<IBoardControllerProps['canvas']>) {
    const canvas = stubIt<IBoardControllerProps['canvas']>({
        wrapper: mockCanvas({height: 10, width: 10}),
        boarCanvasSize: {height: 1, width: 1},
        startAt: {x: 1, y: 1},
        ...p
    });
    return canvas;
}

function stubPointerRender(p?: Partial<PointerRender>) {
    const pointer = stubIt<PointerRender>({
        setPosition: () => {},
        render: () => {},
        ...p
    });
    return pointer;
}

function stubEntityRender(p?: Partial<IBoardItemRender>) {
    const render = stubIt<IBoardItemRender>({
        render: () => {},
        ...p
    });
    return render;
}

function stubRenderFactory(p?: Partial<IBoardControllerProps['renderFactory']>) {
    const factories = stubIt<IBoardControllerProps['renderFactory']>({
        pointer: () => stubPointerRender(),
        ...p
    });
    return factories;
}

function getProperties(p?: Partial<IBoardControllerProps>): IBoardControllerProps {
    const props = stubIt<IBoardControllerProps>({
        assets: stubIt(),
        canvas: stubBoardCanvas(),
        board: {width: 1, height: 1},
        renderFactory: stubRenderFactory(),
        ...p
    });
    return props;
}

it('draw background', () => {
    const backgroundColor = '#D9D9D9';
    const canvas = mockCanvas({height: 1000, width: 1000});    
    const boardSize: TCanvasSize =  {height: 100, width: 100};
    const startAt: TCanvasCoordinates =  {x: 500, y: 500};

    canvas.drawRect = (color, start, size) => {
        expect(color).toBe(backgroundColor);
        expect(start).toEqual(startAt);
        expect(size).toEqual(boardSize);
    }
    const spyDrawRect = jest.spyOn(canvas, 'drawRect');

    new BoardController(getProperties({
        canvas: stubBoardCanvas({
            wrapper: canvas, 
            boarCanvasSize: boardSize, 
            startAt
        })
    })).render();

    expect(spyDrawRect).toBeCalledTimes(1);
});

it('draw background with asset', () => {
    const backgroundAsset = stubAsset();
    const assets: TGameAssets = {
        'board-background': backgroundAsset
    }
    const canvas = mockCanvas({height: 1000, width: 1000});    
    const boardSize: TCanvasSize =  {height: 100, width: 100};
    const startAt: TCanvasCoordinates =  {x: 500, y: 500};

    const backgroundPattern = stubIt<CanvasPattern>();
    canvas.createPattern = (image) => {
        expect(image).toBe(backgroundAsset.image);
        return backgroundPattern;
    }

    canvas.drawRect = (pattern, start, size) => {
        expect(pattern).toBe(backgroundPattern);
        expect(start).toBe(startAt);
        expect(size).toBe(boardSize);
    }
    const spyDrawRect = jest.spyOn(canvas, 'drawRect');

    new BoardController(getProperties({
        canvas: stubBoardCanvas({
            wrapper: canvas, 
            boarCanvasSize: boardSize, 
            startAt
        }),
        assets
    })).render();

    expect(spyDrawRect).toBeCalledTimes(1);
});

it('set pointer initial position', () => {
    const canvas = mockCanvas({height: 1000, width: 1000});
    const startAt: TCanvasCoordinates =  {x: 500, y: 500};
    const board: TBoard = {width: 10, height: 10};
    const boardSize: TCanvasSize =  {height: 200, width: 200};
    
    const pointerPositionInCanvas: TBoardCoordinates = {x: 600, y: 600};

    const pointerRender = stubIt<PointerRender>();
    pointerRender.setPosition = coord => {
        expect(coord).toEqual(pointerPositionInCanvas);
    };
    const spySetPosition = jest.spyOn(pointerRender, 'setPosition');

    const renderFactory = stubRenderFactory({
        pointer(props) {
            expect(props.canvas).toBe(canvas);
            expect(props.cellSize).toEqual({width: 20, height: 20});
            return pointerRender;
        },
    });
    const spyCreatePointer = jest.spyOn(renderFactory, 'pointer');

    new BoardController(getProperties({
        canvas: stubBoardCanvas({
            wrapper: canvas, 
            boarCanvasSize: boardSize, 
            startAt
        }),
        board,
        renderFactory
    }));
    
    expect(spyCreatePointer).toBeCalledTimes(1);
    expect(spySetPosition).toBeCalledTimes(1);
});

it('render pointer', () => {
    const pointerRender = stubPointerRender();
    const spyPointerRender = jest.spyOn(pointerRender, 'render');

    new BoardController(getProperties({
        renderFactory: stubRenderFactory({
            pointer() {
                return pointerRender;
            },
        })
    })).render();
    
    expect(spyPointerRender).toBeCalledTimes(1);
});

describe('handling board click', () => {
    it('return undefined when canvas click is out of the board area', () => {
        const canvas = mockCanvas({height: 1000, width: 1000});    
        const boardSize: TCanvasSize =  {height: 100, width: 100};
        const startAt: TCanvasCoordinates =  {x: 500, y: 500};
        const board: TBoard = {width: 10, height: 10};

        const controller = new BoardController(getProperties({
            canvas: stubBoardCanvas({
                wrapper: canvas, 
                boarCanvasSize: boardSize, 
                startAt
            }),
            board
        }));
        const clicks: TCanvasCoordinates[] = [
            {x: 499, y: 499}, {x: 600, y: 600}, {x: 499, y: 600}, {x: 600, y: 499}
        ];
        for (let index = 0; index < clicks.length; index++) {
            const click = controller.clickOnBoard(clicks[index]);
            expect(click).toBeUndefined();
        }
    });
    it('convert canvas click into board click', () => {
        const canvas = mockCanvas({height: 1000, width: 1000});    
        const boardSize: TCanvasSize =  {height: 100, width: 100};
        const startAt: TCanvasCoordinates =  {x: 500, y: 500};
        const board: TBoard = {width: 10, height: 10};

        const controller = new BoardController(getProperties({
            canvas: stubBoardCanvas({
                wrapper: canvas, 
                boarCanvasSize: boardSize, 
                startAt
            }),
            board
        }));
        const clicks: TCanvasCoordinates[] = [
            {x: 500, y: 500}, {x: 599, y: 599}, {x: 500, y: 599}, {x: 599, y: 500}
        ];
        const boardCell: TBoardCoordinates[] = [
            {x: 0, y: 0}, {x: 9, y: 9}, {x: 0, y: 9}, {x: 9, y: 0}
        ];
        for (let index = 0; index < clicks.length; index++) {
            const click = controller.clickOnBoard(clicks[index]);
            expect(click).toEqual(boardCell[index]);
        }
    });
    it('set pointer in the click', () => {
        const canvas = mockCanvas({height: 1000, width: 1000});    
        const boardSize: TCanvasSize =  {height: 100, width: 100};
        const startAt: TCanvasCoordinates =  {x: 500, y: 500};
        const board: TBoard = {width: 10, height: 10};

        const pointerPositionInCanvas: TCanvasCoordinates = {x: 590, y: 590};

        const pointerRender = stubIt<PointerRender>();
        let firstCheck = true;
        pointerRender.setPosition = coord => {
            if (firstCheck) {
                firstCheck = false;
                return;
            }
            expect(coord).toEqual(pointerPositionInCanvas);
        };
        const spySetPosition = jest.spyOn(pointerRender, 'setPosition');

        const controller = new BoardController(getProperties({
            canvas: stubBoardCanvas({
                wrapper: canvas, 
                boarCanvasSize: boardSize, 
                startAt
            }),
            board,
            renderFactory: stubRenderFactory({
                pointer: () => pointerRender
            })
        }));

        const click = controller.clickOnBoard({x: 599, y: 599})
        
        expect(click).not.toBeUndefined();
        expect(spySetPosition).toBeCalledTimes(2);
    });
    it('turn player entity to the click', () => {
        const canvas = mockCanvas({height: 1000, width: 1000});    
        const boardSize: TCanvasSize =  {height: 100, width: 100};
        const startAt: TCanvasCoordinates =  {x: 500, y: 500};
        const board: TBoard = {width: 10, height: 10};
        const canvasClick = {x: 599, y: 599};
        const canvasClickAfterBeConverted = {x: 590, y: 590};

        const player = stubEntity({id: 'playerId'});
        const playerRender = stubEntityRender({
            turnTo(point) {
                expect(point).toEqual(canvasClickAfterBeConverted);
            },
        });
        const spyTurnTo = jest.spyOn(playerRender, 'turnTo');


        const controller = new BoardController(getProperties({
            playerId: player.id,
            canvas: stubBoardCanvas({
                wrapper: canvas, 
                boarCanvasSize: boardSize, 
                startAt
            }),
            board,
            renderFactory: stubRenderFactory({
                entity: () => playerRender
            })
        }));
        controller.addEntity(player, {x: 0, y: 0});
        const click = controller.clickOnBoard(canvasClick)
        expect(click).not.toBeUndefined();
        expect(spyTurnTo).toBeCalledTimes(1);
    });
});

describe('drawning entities', () => {
    it('set player', () => {
        const assets: TGameAssets = {
            'player': stubAsset(),
            'player-back': stubAsset(),
            'player-side': stubAsset(),
            'player-side-upper': stubAsset(),
        }
        const canvas = mockCanvas({height: 1000, width: 1000});
        const startAt: TCanvasCoordinates =  {x: 500, y: 500};
        const board: TBoard = {width: 10, height: 10};
        const boardSize: TCanvasSize =  {height: 200, width: 200};
        const expectedCellSize: TCanvasSize = {height: 20, width: 20};
        const player = stubEntity();
        const playerPostition: TBoardCoordinates = {x: 2, y: 2};
        const startDrawningEntityIn: TCanvasCoordinates = {
            x: 40 + startAt.x, 
            y: 40 + startAt.y
        };

        const renderFactory = stubRenderFactory({
            entity(props) {
                expect(props.assets).toBe(assets);
                expect(props.canvas).toBe(canvas);
                expect(props.isThePlayer).toBeTruthy();
                expect(props.start).toEqual(startDrawningEntityIn);
                expect(props.cellSize).toEqual(expectedCellSize);
                return stubEntityRender();
            }
        });
        const spyEntityFactory = jest.spyOn(renderFactory, 'entity');

        const controller = new BoardController(getProperties({
            playerId: player.id,
            canvas: stubBoardCanvas({
                wrapper: canvas, 
                boarCanvasSize: boardSize, 
                startAt
            }),
            renderFactory,
            board,
            assets
        }));
        controller.addEntity(player, playerPostition);
        expect(spyEntityFactory).toBeCalledTimes(1);
    });
    it('update entity position', () => {
        const canvas = mockCanvas({height: 1000, width: 1000});
        const startAt: TCanvasCoordinates =  {x: 500, y: 600};
        const board: TBoard = {width: 10, height: 10};
        const boardSize: TCanvasSize =  {height: 200, width: 200};
        
        const entity = stubEntity({id: 'entityId'});
        const entityInitialPosition: TBoardCoordinates = {x:0, y:0};
        const entityIFinalPosition: TBoardCoordinates = {x:1, y:2};
        const finalPositionInCanvas: TCanvasCoordinates = {x: 520, y: 640};

        const entityRender = stubEntityRender();
        entityRender.turnTo = coord => {
            expect(coord).toEqual(finalPositionInCanvas);
        }
        const spyTurnTo = jest.spyOn(entityRender, 'turnTo');
        entityRender.setPosition = coord => {
            expect(coord).toEqual(finalPositionInCanvas);
            expect(spyTurnTo).toBeCalledTimes(1);
        }
        const spySetPosition = jest.spyOn(entityRender, 'setPosition');

        const controller = new BoardController(getProperties({
            canvas: stubBoardCanvas({
                wrapper: canvas, 
                boarCanvasSize: boardSize, 
                startAt
            }),
            renderFactory: stubRenderFactory({
                entity: () => entityRender
            }),
            board
        }));
        controller.addEntity(entity, entityInitialPosition);
        controller.updateEntityPosition(entity.id, entityIFinalPosition)
        expect(spySetPosition).toBeCalledTimes(1);
    });
    it('render entity', () => {
        const entity = stubEntity();

        const entityRender = stubEntityRender();
        const spyEntityRender = jest.spyOn(entityRender, 'render');

        const controller = new BoardController(getProperties({
            renderFactory: stubRenderFactory({
                entity: () => entityRender
            })
        }));
        controller.addEntity(entity, {x: 0, y: 0});
        controller.render();
        expect(spyEntityRender).toBeCalledTimes(1);
    });
});