import CanvasWrapper, { ICanvasWrapper } from "../CanvasWrapper";
import { IAsset, TCanvasCoordinates, TCanvasSize, TCoordinates, TSize } from "../interfaces";

export function stubIt<T>(props?: Partial<T>) {
    const v: Partial<T> =  {...props};
    return v as T;
}

export function mockCanvas(size: TCanvasSize): ICanvasWrapper {
    const canvasMock: Partial<ICanvasWrapper> = {
        'getSize': () => size        
    }
    canvasMock['writeText'] = mockCanvasWrite();
    canvasMock['drawEmptyRect'] = mockCanvasDrawEmptyRect();
    canvasMock['drawRect'] = mockCanvasDrawRect();
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
        }
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
                }
            );
        });
    return value;
}