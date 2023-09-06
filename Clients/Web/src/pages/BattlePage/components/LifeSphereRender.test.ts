import CanvasWrapper, { ICanvasWrapper } from "../../../CanvasWrapper";
import { IAsset, TCanvasCoordinates, TCanvasSize, TCoordinates, TSize } from "../../../interfaces";
import { LifeSphereRender } from "./LifeSphereRenderComponents"

function mockCanvas(size: TCanvasSize): ICanvasWrapper {
    const canvasMock: Partial<ICanvasWrapper> = {
        'getSize': () => size        
    }
    return canvasMock as ICanvasWrapper;
}

function mockCanvasDrawRect() {
    return jest.fn<
    void, 
    Array<
        CanvasFillStrokeStyles['fillStyle'] | 
        TCanvasCoordinates | 
        TSize | 
        (number | undefined)
    >, 
    CanvasWrapper>();
}

function mockCanvasDrawEmptyRect() {
    return jest.fn<
    void, 
    Array<
        CanvasFillStrokeStyles['fillStyle'] | 
        TCanvasCoordinates | 
        TSize
    >, 
    CanvasWrapper>();
}

function mockCanvasDrawCircle(impl?: ICanvasWrapper['drawCircle']) {
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

function mockCanvasDrawAsset(impl?: ICanvasWrapper['drawAsset']) {
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

function sizesMatch(currentSize: TSize, expected: TSize) {
    expect(currentSize.height).toBe(expected.height);
    expect(currentSize.width).toBe(expected.width);
}

/**
 * TODO:: receive scale and the health raidus as separated values
 * and handles the logic inside to draw a circle that fits the canvas
 */

it('should draw circle to fill the canvas', () => {
    const fakeAsset = {} as IAsset;
    const canvas = mockCanvas({width: 100, height: 100});
    const healthRadiusInScale  = 50;
    const expectedCenter: TCanvasCoordinates = {
        x: healthRadiusInScale, y: healthRadiusInScale
    };
    const drawCircle = mockCanvasDrawCircle((center, radius, color) => {
        expect(center.x).toBe(expectedCenter.x);
        expect(center.y).toBe(expectedCenter.y);
        expect(radius).toBe(healthRadiusInScale);
    });
    canvas['drawCircle'] = drawCircle;
    const sphere = new LifeSphereRender(canvas, healthRadiusInScale, fakeAsset);
    sphere.render();

    expect(drawCircle).toBeCalledTimes(1);
});

it('should not draw circle when the assets have an image', () => {
    const fakeAsset = { image: {} } as IAsset;
    const canvas = mockCanvas({width: 100, height: 100});
    const healthRadiusInScale  = 50;
    const drawCircle = mockCanvasDrawCircle();
    canvas['drawCircle'] = drawCircle;
    canvas['drawAsset'] = mockCanvasDrawAsset();
    const sphere = new LifeSphereRender(canvas, healthRadiusInScale, fakeAsset);
    sphere.render();

    expect(drawCircle).not.toBeCalled();
});

it('should draw asset to fill the canvas', () => {
    const fakeAsset = { 
        image: {}, 
        size: {width: 50, height: 50},
        start: {x: 0, y: 0}
    } as IAsset;
    const canvasSize: TCanvasSize = {width: 100, height: 100};
    const canvas = mockCanvas(canvasSize);
    const healthRadiusInScale  = 50;
    const drawAsset = mockCanvasDrawAsset((asset, destination) => {
        expect(asset.size).toEqual(fakeAsset.size);
        expect(asset.start).toEqual(fakeAsset.start);
        expect(asset.image).toBe(fakeAsset.image);
        expect(destination.startAt).toEqual({x: 0, y: 0} as TCoordinates);
        expect(destination.height).toBe(canvasSize.height);
        expect(destination.width).toBe(canvasSize.width);
    });
    canvas['drawCircle'] = mockCanvasDrawCircle();
    canvas['drawAsset'] = drawAsset;
    const sphere = new LifeSphereRender(canvas, healthRadiusInScale, fakeAsset);
    sphere.render();

    expect(drawAsset).toBeCalledTimes(1);
})