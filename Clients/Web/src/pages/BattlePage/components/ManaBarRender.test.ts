import { TCanvasSize, TCoordinates } from "../../../interfaces";
import { mockCanvas, mockCanvasDrawEmptyRect, mockCanvasDrawRect, mockCanvasWrite } from "../../../jest/helpers";
import { ManaBarRender } from "./LifeSphereRenderComponents";

function getCanvas(size: TCanvasSize) {
    const canvas = mockCanvas(size);
    canvas['writeText'] = mockCanvasWrite();
    canvas['drawEmptyRect'] = mockCanvasDrawEmptyRect();
    canvas['drawRect'] = mockCanvasDrawRect();
    return canvas;
}

it('should render border', () => {
    const borderColor = '#000000';
    const canvasSize: TCanvasSize = {height: 100, width: 100};
    const canvas = getCanvas(canvasSize);
    const drawBorders = mockCanvasDrawEmptyRect((color, start, size) => {
        expect(color).toBe(borderColor)
        expect(size)
        .toEqual({width: canvasSize.width, height: 25} as TCanvasSize);
        expect(start).toEqual({x: 0, y: 0} as TCoordinates)
    });
    canvas['drawEmptyRect'] = drawBorders;
    new ManaBarRender(canvas).render();
    expect(drawBorders).toBeCalledTimes(1);
});

it('should fill background', () => {
    const backgroundColor = '#589099'
    const canvasSize: TCanvasSize = {height: 100, width: 100};
    const canvas = getCanvas(canvasSize);
    const drawBackground = mockCanvasDrawRect((color, start, size) => {
        expect(color).toBe(backgroundColor)
        expect(size)
        .toEqual({width: canvasSize.width, height: 25} as TCanvasSize);
        expect(start).toEqual({x: 0, y: 0} as TCoordinates)
    });
    canvas['drawRect'] = drawBackground;
    new ManaBarRender(canvas).render();
    expect(drawBackground).toBeCalledTimes(1);
});

it('should write the quantity of mana', () => {
    const textColor = '#FFFFFF'
    const canvasSize: TCanvasSize = {height: 100, width: 100};
    const canvas = getCanvas(canvasSize);
    const write = mockCanvasWrite((coordinates, value, color) => {
        expect(color).toBe(textColor);
        expect(value).toBe('0');
        const manaBarHeight = 25;
        expect(coordinates)
        .toEqual({x: (canvasSize.width / 2) - 2, y: manaBarHeight - 10} as TCoordinates);
    });
    canvas['writeText'] = write;
    new ManaBarRender(canvas).render();
    expect(write).toBeCalledTimes(1);
});

it('should render the background, the border and then  render the text', () => {
    const canvas = mockCanvas({height: 100, width: 100});
    
    const drawBackground = mockCanvasDrawRect();
    canvas['drawRect'] = drawBackground;

    const drawBorders = mockCanvasDrawEmptyRect(() => {
        expect(drawBackground).toBeCalledTimes(1);
    });
    canvas['drawEmptyRect'] = drawBorders;
    
    const write = mockCanvasWrite(() => {
        expect(drawBorders).toBeCalledTimes(1);    
    });
    canvas['writeText'] = write;
    
    new ManaBarRender(canvas).render();

    expect(write).toBeCalledTimes(1);
});

it('When change the current value, change the text', () => {
    const canvas = getCanvas({height: 100, width: 100});
    const newValue = 123;
    const write = mockCanvasWrite((_, text) => {
        expect(text).toBe(`${newValue}`);    
    });
    canvas['writeText'] = write;
    
    const manaBar = new ManaBarRender(canvas);
    manaBar.updateCurrentValue(newValue);

    expect(write).toBeCalledTimes(1);
});