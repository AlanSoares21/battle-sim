import { TCanvasCoordinates, TCanvasSize } from "../../../interfaces";
import { mockCanvas } from "../../../jest/helpers";
import { PointerRender } from "./PointerRender";

/**
 * Obs: this test is simple because this component is simple
 */
it('pointer render works properly', () => {
    const canvas = mockCanvas({height: 100, width: 200});
    const cellSize: TCanvasSize = {height: 10, width: 20};
    const pointerPosition: TCanvasCoordinates = {x: 30, y: 30};

    canvas.drawEmptyElipse = (center, radius, rotation, borderColor) => {
        expect(rotation).toBe(0);
        expect(borderColor).toBe('#FF0000');
        expect(radius).toEqual({x: 10, y: 5});
        expect(center).toEqual({x: 40, y: 35});
    }
    const spyDrawElipse = jest.spyOn(canvas, 'drawEmptyElipse');
    
    const pointer = new PointerRender(canvas, cellSize); 
    pointer.setPosition(pointerPosition);
    pointer.render();

    expect(spyDrawElipse).toBeCalledTimes(1);
});