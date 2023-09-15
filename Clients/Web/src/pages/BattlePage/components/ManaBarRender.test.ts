import { ICanvasWrapper } from "../../../CanvasWrapper";
import { TCanvasSize, TCoordinates } from "../../../interfaces";
import { mockCanvas, mockCanvasDrawEmptyRect, mockCanvasDrawRect, mockCanvasWrite, stubAsset, stubIt } from "../../../jest/helpers";
import { IManaBarRenderProps, ManaBarRender } from "./LifeSphereRenderComponents";

const getProperties = stubIt<IManaBarRenderProps>;

it('should render border', () => {
    const borderColor = '#000000';
    const canvasSize: TCanvasSize = {height: 100, width: 100};
    const canvas = mockCanvas(canvasSize);
    const drawBorders = mockCanvasDrawEmptyRect((color, start, size) => {
        expect(color).toBe(borderColor)
        expect(size)
        .toEqual({width: canvasSize.width, height: 25} as TCanvasSize);
        expect(start).toEqual({x: 0, y: 0} as TCoordinates)
    });
    canvas['drawEmptyRect'] = drawBorders;
    new ManaBarRender(getProperties({canvas})).render();
    expect(drawBorders).toBeCalledTimes(1);
});

it('should fill background', () => {
    const backgroundColor = '#589099'
    const canvasSize: TCanvasSize = {height: 100, width: 100};
    const canvas = mockCanvas(canvasSize);
    const drawBackground = mockCanvasDrawRect((color, start, size) => {
        expect(color).toBe(backgroundColor)
        expect(size)
        .toEqual({width: canvasSize.width, height: 25} as TCanvasSize);
        expect(start).toEqual({x: 0, y: 0} as TCoordinates)
    });
    canvas['drawRect'] = drawBackground;
    new ManaBarRender(getProperties({canvas})).render();
    expect(drawBackground).toBeCalledTimes(1);
});

it('should write the quantity of mana', () => {
    const textColor = '#FFFFFF'
    const canvasSize: TCanvasSize = {height: 100, width: 100};
    const canvas = mockCanvas(canvasSize);
    const write = mockCanvasWrite((coordinates, value, color) => {
        expect(color).toBe(textColor);
        expect(value).toBe('0');
        const manaBarHeight = 25;
        expect(coordinates)
        .toEqual({x: (canvasSize.width / 2) - 2, y: manaBarHeight - 10} as TCoordinates);
    });
    canvas['writeText'] = write;
    new ManaBarRender(getProperties({canvas})).render();
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
    
    new ManaBarRender(getProperties({canvas})).render();

    expect(write).toBeCalledTimes(1);
});

it('When change the current value, change the text', () => {
    const canvas = mockCanvas({height: 100, width: 100});
    const newValue = 123;
    const write = mockCanvasWrite((_, text) => {
        expect(text).toBe(`${newValue}`);    
    });
    canvas['writeText'] = write;
    
    const manaBar = new ManaBarRender(getProperties({canvas}));
    manaBar.updateCurrentValue(newValue, 100);

    expect(write).toBeCalledTimes(1);
});

describe('rendering assets', () => {
    let canvas: ICanvasWrapper;
    let assets: {
        background: IManaBarRenderProps['background'],
        border: IManaBarRenderProps['border'],
        fill: IManaBarRenderProps['fill']
    };

    beforeEach(() => {
        assets = {
            background: stubAsset(),
            border: stubAsset(),
            fill: stubAsset()
        }
        canvas = mockCanvas({height: 100, width: 100});
    });

    it('should not draw rectangles when have assets', () => {
        const spyDrawBackground = jest.spyOn(canvas, 'drawRect');
        const spyDrawBorder = jest.spyOn(canvas, 'drawEmptyRect');
        
        new ManaBarRender(getProperties({
            canvas,
            ...assets
        })).render();

        expect(spyDrawBackground).toBeCalledTimes(0);
        expect(spyDrawBorder).toBeCalledTimes(0);
    });
    it('should draw the assets in order', () => {
        let assetsChecked = 0;
        canvas.drawAsset = (asset, _) => {
            if (assetsChecked === 0)
                expect(asset).toBe(assets.background);
            else if (assetsChecked === 1)
                expect(asset).toBe(assets.fill);
            else if (assetsChecked === 3)
                expect(asset).toBe(assets.border);
            assetsChecked++;
        };
        const spyDrawAsset = jest.spyOn(canvas, 'drawAsset');
        
        new ManaBarRender(getProperties({
            canvas,
            ...assets
        })).render();
        expect(spyDrawAsset).toBeCalledTimes(3);
    });
    it('should draw background and border assets with the mana bar size', () => {
        let assertNumber = 0;
        const canvasSize = canvas.getSize();
        canvas.drawAsset = (asset, destinaton) => {
            if (assertNumber === 0) {
                expect(asset).toBe(assets.background);
                expect(destinaton.height).toBe(canvasSize.width / 4);
                expect(destinaton.width)
                    .toBe(canvasSize.width);
            }
            else if (assertNumber === 2) {
                expect(asset).toBe(assets.border);
                expect(destinaton.height).toBe(canvasSize.width / 4);
                expect(destinaton.width)
                    .toBe(canvasSize.width);
            }
            assertNumber++;
        }
        const spyDrawAsset = jest.spyOn(canvas, 'drawAsset');
        new ManaBarRender(getProperties({
            canvas,
            ...assets
        })).render();
        expect(spyDrawAsset).toBeCalledTimes(3);
    })
    it('should draw the assets before write the text', () => {
        const spyDrawAsset = jest.spyOn(canvas, 'drawAsset');
        canvas.writeText = () => {
            expect(spyDrawAsset).toBeCalledTimes(3);
        };
        const spyWriteText = jest.spyOn(canvas, 'writeText');
        new ManaBarRender(getProperties({
            canvas,
            ...assets
        })).render();
        expect(spyWriteText).toBeCalledTimes(1);
    });
    it('should fill mana bar accordingly to the ammount of mana', () => {
        const fillHappensInAssert = [1, 4, 7];
        const fillProportion = [5/100, 50/100, 100/100];
        const canvasSize = canvas.getSize();
        let assertNumber = 0;
        canvas.drawAsset = (asset, destinaton) => {
            const proportionIndex = fillHappensInAssert.findIndex(a => a === assertNumber);
            if (proportionIndex !== -1) {
                expect(asset).toBe(assets.fill);
                expect(destinaton.height).toBe(canvasSize.width / 4);
                expect(destinaton.width)
                    .toBe(canvasSize.width * fillProportion[proportionIndex]);
            }
            assertNumber++;
        }
        const spyDrawAsset = jest.spyOn(canvas, 'drawAsset');
        const manaBar = new ManaBarRender(getProperties({
            canvas,
            ...assets
        }));
        const maxMana = 100;
        manaBar.updateCurrentValue(5, maxMana);
        expect(spyDrawAsset).toBeCalledTimes(3);
        manaBar.updateCurrentValue(50, maxMana);
        expect(spyDrawAsset).toBeCalledTimes(6);
        manaBar.updateCurrentValue(100, maxMana);
        expect(spyDrawAsset).toBeCalledTimes(9);
    });
});