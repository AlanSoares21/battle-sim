import CanvasWrapper, { ICanvasWrapper, TCanvasTransformations } from "../CanvasWrapper";
import { IAsset, IEntity, TCanvasCoordinates, TCanvasSize, TCoordinates, TSize } from "../interfaces";
import { BoardController } from "../pages/BattlePage/components/BoardController";

export function stubIt<T>(props?: Partial<T>) {
    const v: Partial<T> =  {...props};
    return v as T;
}

export function stubAsset(p?: Partial<IAsset>) {
    const asset = stubIt<IAsset>({
        image: stubIt(),
        ...p
    });
    return asset;
}

export function mockCanvas(size: TCanvasSize): ICanvasWrapper {
    const canvasMock: Partial<ICanvasWrapper> = {
        'getSize': () => size        
    }
    canvasMock['writeText'] = mockCanvasWrite();
    canvasMock['drawEmptyRect'] = mockCanvasDrawEmptyRect();
    canvasMock['drawRect'] = mockCanvasDrawRect();
    canvasMock['createPattern'] = () => null;
    canvasMock['drawAsset'] = () => {};
    return canvasMock as ICanvasWrapper;
}

export function mockCanvasWrite(impl?: ICanvasWrapper['writeText']) {
    const value = jest.fn<
    void, 
    Array<
        TCoordinates | 
        string | 
        CanvasFillStrokeStyles['fillStyle'] | 
        (number | undefined)
    >, 
    CanvasWrapper>();
    if (impl)
        value.mockImplementation((...args) => {
            impl(
                args[0] as TCoordinates,
                args[1] as string,
                args[2] as CanvasFillStrokeStyles['fillStyle']
            )
        });
    return value;
}

export function mockCanvasDrawRect(impl?: ICanvasWrapper['drawRect']) {
    const value = jest.fn<
    void, 
    Array<
        CanvasFillStrokeStyles['fillStyle'] | 
        TCanvasCoordinates | 
        TSize | 
        (number | undefined)
    >, 
    CanvasWrapper>();
    if (impl)
        value.mockImplementation((...args) => {
            impl(
                args[0] as CanvasFillStrokeStyles['fillStyle'],
                args[1] as TCoordinates,
                args[2] as TSize
            )
        });
    return value;
}

export function mockCanvasDrawEmptyRect(impl?: ICanvasWrapper['drawEmptyRect']) {
    const value = jest.fn<
    void, 
    Array<
        CanvasFillStrokeStyles['fillStyle'] | 
        TCanvasCoordinates | 
        TSize
    >, 
    CanvasWrapper>();
    if (impl)
        value.mockImplementation((...args) => {
            impl(
                args[0] as CanvasFillStrokeStyles['fillStyle'],
                args[1] as TCoordinates,
                args[2] as TSize
            )    
        });
    return value;
}

export function mockCanvasDrawCircle(impl?: ICanvasWrapper['drawCircle']) {
    const value = jest.fn<
    void, 
    Array<
        TCanvasCoordinates |
        number |
        CanvasFillStrokeStyles['fillStyle']
    >,
    CanvasWrapper>();
    if (impl)
        value.mockImplementation((...args) => {
            impl(
                args[0] as TCanvasCoordinates, 
                args[1] as number, 
                args[2] as CanvasFillStrokeStyles['fillStyle']
            );
        });
    return value;
}

export function mockCanvasDrawAsset(impl?: ICanvasWrapper['drawAsset']) {
    const value = jest.fn<
    void, 
    Array<
        IAsset |
        {
            startAt: TCoordinates;
            height: number;
            width: number;
        } |
        TCanvasTransformations |
        undefined
    >,
    CanvasWrapper>();
    if (impl)
        value.mockImplementation((...args) => {
            impl(
                args[0] as IAsset, 
                args[1] as {
                    startAt: TCoordinates;
                    height: number;
                    width: number;
                },
                args[2] as TCanvasTransformations | undefined
            );
        });
    return value;
}

export function stubEntity(p?: Partial<IEntity>) {
    const entity: Partial<IEntity> = {
        skills: [],
        ...p
    };
    return entity as IEntity;
}

export function stubBoardController(p?: Partial<BoardController>) {
    const controller = stubIt<BoardController>({
        addEntity: () => {},
        render: () => {},
        clickOnBoard: () => undefined,
        ...p
    });
    return controller;
}