import { ICanvasWrapper } from "../../../CanvasWrapper";
import { IAsset, TCanvasCoordinates, TCanvasSize, TCoordinates } from "../../../interfaces";
import { mockCanvas, stubAsset } from "../../../jest/helpers";
import { DefaultEntityRender, EntityRender, IEntityAssets } from "./EntityRenders";


describe('default entity render', () => {
    it('render entity with correct parameters', () => {
        const entityColor = '#00FF00';
        const entitySize: TCanvasSize = {height: 10, width: 10};
        const start: TCanvasCoordinates = {x: 0, y: 0};
        const canvas = mockCanvas({height: 100, width: 100});
        canvas.drawCircle = (center, radius, color) => {
            expect(color).toBe(entityColor);
            expect(center).toEqual({x: 5, y: 5} as TCanvasCoordinates)
            expect(radius).toBe(5)
        }; 
        const spyDrawCircle = jest.spyOn(canvas, 'drawCircle');
        
        new DefaultEntityRender(canvas, start, entitySize).render();
        expect(spyDrawCircle).toBeCalledTimes(1);
    });
    it('when set position change the center', () => {
        const entitySize: TCanvasSize = {height: 10, width: 10};
        const start: TCanvasCoordinates = {x: 0, y: 0};
        const canvas = mockCanvas({height: 100, width: 100});

        const newPosition: TCanvasCoordinates = {x: 20, y: 40};
        canvas.drawCircle = (center) => {
            expect(center).toEqual({x: 25, y: 45} as TCanvasCoordinates);
        }; 
        const spyDrawCircle = jest.spyOn(canvas, 'drawCircle');
        
        const render = new DefaultEntityRender(canvas, start, entitySize);
        render.setPosition(newPosition);
        render.render();

        expect(spyDrawCircle).toBeCalledTimes(1);
    });
});

describe('spin entity, then render the properly asset for', () => {
    let assets: IEntityAssets;
    let entitySize: TCanvasSize;
    let start: TCanvasCoordinates;
    let canvas: ICanvasWrapper;
    const above: TCanvasCoordinates = {x: 50, y: 0};
    const down: TCanvasCoordinates = {x: 50, y: 100};
    const right: TCanvasCoordinates = {x: 100, y: 50};
    const left: TCanvasCoordinates = {x: 0, y: 50};
    const diagonalRightUp: TCanvasCoordinates = {x: 100, y: 0};
    const diagonalRightDown: TCanvasCoordinates = {x: 100, y: 100};
    const diagonalLeftUp: TCanvasCoordinates = {x: 0, y: 0};
    const diagonalLeftDown: TCanvasCoordinates = {x: 0, y: 100};

    function spinArround(entity: EntityRender, exclude: TCanvasCoordinates ) {
        const directions: TCanvasCoordinates[] = [
            above, down, right, left, diagonalRightUp, diagonalRightDown,
            diagonalLeftUp, diagonalLeftDown
        ].filter(d => d.x !== exclude.x && d.y !== exclude.y);
        for (const direction of directions) {
            entity.turnTo(direction);
        }
    }

    function thisAssetIs(asset: IAsset) {
        if (asset === assets.normal)
            console.log('the asset is normal');
        else if (asset === assets.back)
            console.log('the asset is back');
        else if (asset === assets.side)
            console.log('the asset is side');
        else if (asset === assets.sideUpper)
            console.log('the asset is side upper');
    }

    beforeEach(() => {
        assets = {
            back: stubAsset(),
            normal: stubAsset(),
            side: stubAsset(),
            sideUpper: stubAsset()
        }
        entitySize = {height: 10, width: 10};
        start = {x: 50, y: 50};
        canvas = mockCanvas({height: 100, width: 100});
    })
    it('when turn down, render asset for normal', () => {
        canvas.drawAsset = (asset, dest) => {
            expect(asset).toBe(assets.normal);
            expect(dest.startAt).toEqual(start);
            expect(dest.width).toBe(entitySize.width);
            expect(dest.height).toBe(entitySize.height);
        }; 
        const spyDrawAsset = jest.spyOn(canvas, 'drawAsset');
        
        const entity = new EntityRender(canvas, start, entitySize, assets);
        spinArround(entity, down);
        entity.turnTo(down);
        entity.render();

        expect(spyDrawAsset).toBeCalledTimes(1);
    });
    it('when turn above render the asset for back', () => {
        canvas.drawAsset = (asset, dest) => {
            expect(dest.startAt).toEqual(start);
            expect(dest.width).toBe(entitySize.width);
            expect(dest.height).toBe(entitySize.height);
            expect(asset).toBe(assets.back);
        }; 
        const spyDrawAsset = jest.spyOn(canvas, 'drawAsset');
        
        const entity = new EntityRender(canvas, start, entitySize, assets);
        spinArround(entity, above);
        entity.turnTo(above);
        entity.render();

        expect(spyDrawAsset).toBeCalledTimes(1);
    });
    it('when turn to right render the asset for side', () => {
        canvas.drawAsset = (asset, dest) => {
            expect(asset).toBe(assets.side);
            expect(dest.startAt).toEqual(start);
            expect(dest.width).toBe(entitySize.width);
            expect(dest.height).toBe(entitySize.height);
        }; 
        const spyDrawAsset = jest.spyOn(canvas, 'drawAsset');
        
        const entity = new EntityRender(canvas, start, entitySize, assets);
        spinArround(entity, right);
        entity.turnTo(right);
        entity.render();

        expect(spyDrawAsset).toBeCalledTimes(1);
    });
    it('when turn to left render the asset for side mirrored', () => {
        canvas.drawAsset = (asset, dest, tranformations) => {
            if (tranformations === undefined)
                fail(`Transformations are undefined`);
            if (tranformations.length === 0)
                fail(`Transformations list are empty`);
            const tranformation = tranformations[0];
            expect(tranformation.type).toBe('Scale');
            expect(tranformation.x).toBe(-1);
            expect(tranformation.y).toBe(1);
            expect(asset).toBe(assets.side);
            // expectedX = -1 * start.x;
            const expectedX = -50;
            expect(dest.startAt.x).toBe(expectedX);
            expect(dest.startAt.y).toBe(start.y);
            expect(dest.width).toBe(entitySize.width);
            expect(dest.height).toBe(entitySize.height);
        }; 
        const spyDrawAsset = jest.spyOn(canvas, 'drawAsset');
        const entity = new EntityRender(canvas, start, entitySize, assets);
        spinArround(entity, left);
        entity.turnTo(left);
        entity.render();
        expect(spyDrawAsset).toBeCalledTimes(1);
    });
    describe('turn in diagonal directions', () => {
        it('turn upper and right', () => {
            canvas.drawAsset = (asset, dest) => {
                expect(asset).toBe(assets.sideUpper);
                expect(dest.startAt).toEqual(start);
                expect(dest.width).toBe(entitySize.width);
                expect(dest.height).toBe(entitySize.height);
            }; 
            const spyDrawAsset = jest.spyOn(canvas, 'drawAsset');
            const entity = new EntityRender(canvas, start, entitySize, assets);
            spinArround(entity, diagonalRightUp);
            entity.turnTo(diagonalRightUp);
            entity.render();
            expect(spyDrawAsset).toBeCalledTimes(1);
        });
        it('turn down and right', () => {
            canvas.drawAsset = (asset, dest, tranformations) => {
                if (tranformations === undefined)
                    fail(`Transformations are undefined`);
                if (tranformations.length === 0)
                    fail(`Transformations list are empty`);
                const tranformation = tranformations[0];
                expect(tranformation.type).toBe('Scale');
                expect(tranformation.x).toBe(1);
                expect(tranformation.y).toBe(-1);
                expect(dest.startAt.x).toBe(start.x);
                // expectedY = -1 * start.y;
                const expectedY = -50;
                expect(dest.startAt.y).toBe(expectedY);

                expect(asset).toBe(assets.sideUpper);
                expect(dest.width).toBe(entitySize.width);
                expect(dest.height).toBe(entitySize.height);
            }; 
            const spyDrawAsset = jest.spyOn(canvas, 'drawAsset');
            const entity = new EntityRender(canvas, start, entitySize, assets);
            spinArround(entity, diagonalRightDown);
            entity.turnTo(diagonalRightDown);
            entity.render();
            expect(spyDrawAsset).toBeCalledTimes(1);
        });
        it('turn upper and left', () => {
            canvas.drawAsset = (asset, dest, tranformations) => {
                if (tranformations === undefined)
                    fail(`Transformations are undefined`);
                if (tranformations.length === 0)
                    fail(`Transformations list are empty`);
                const tranformation = tranformations[0];
                expect(tranformation.type).toBe('Scale');
                expect(tranformation.x).toBe(-1);
                expect(tranformation.y).toBe(1);
                // expectedX = -1 * start.x;
                const expectedX = -50;
                expect(dest.startAt.x).toBe(expectedX);
                expect(dest.startAt.y).toBe(start.y);
    
                expect(asset).toBe(assets.sideUpper);
    
                expect(dest.width).toBe(entitySize.width);
                expect(dest.height).toBe(entitySize.height);
            }; 
            const spyDrawAsset = jest.spyOn(canvas, 'drawAsset');
            const entity = new EntityRender(canvas, start, entitySize, assets);
            spinArround(entity, diagonalLeftUp);
            entity.turnTo(diagonalLeftUp);
            entity.render();
            expect(spyDrawAsset).toBeCalledTimes(1);
        });
        it('turn down and left', () => {
            canvas.drawAsset = (asset, dest, tranformations) => {
                if (tranformations === undefined)
                    fail(`Transformations are undefined`);
                if (tranformations.length === 0)
                    fail(`Transformations list are empty`);
                const tranformation = tranformations[0];
                expect(tranformation.type).toBe('Scale');
                expect(tranformation.x).toBe(-1);
                expect(tranformation.y).toBe(-1);
                // expectedX = -1 * start.x;
                const expectedX = -50;
                expect(dest.startAt.x).toBe(expectedX);
                // expectedY = -1 * start.y;
                const expectedY = -50;
                expect(dest.startAt.y).toBe(expectedY);
    
                expect(asset).toBe(assets.sideUpper);
    
                expect(dest.width).toBe(entitySize.width);
                expect(dest.height).toBe(entitySize.height);
            }; 
            const spyDrawAsset = jest.spyOn(canvas, 'drawAsset');
            const entity = new EntityRender(canvas, start, entitySize, assets);
            spinArround(entity, diagonalLeftDown);
            entity.turnTo(diagonalLeftDown);
            entity.render();
            expect(spyDrawAsset).toBeCalledTimes(1);
        });
    });
    it('change the initial position', () => {
        const newPosition: TCanvasCoordinates = {x: 40, y:60};
        canvas.drawAsset = (asset, dest) => {
            expect(asset).toBe(assets.normal);
            expect(dest.startAt).toEqual(newPosition);
            expect(dest.width).toBe(entitySize.width);
            expect(dest.height).toBe(entitySize.height);
        }; 
        const spyDrawAsset = jest.spyOn(canvas, 'drawAsset');
        
        const entity = new EntityRender(canvas, start, entitySize, assets);
        spinArround(entity, down);
        entity.setPosition(newPosition);
        entity.turnTo({x: newPosition.x, y: 100});
        entity.render();

        expect(spyDrawAsset).toBeCalledTimes(1);
    });
});