import BoardCanvas from "./BoardCanvas";
import { IPlayerRenderData, RenderElementsHandlers, TBoardCoordinates, TRenderElement } from "./interfaces";
import { isPlayerRenderData } from "./typeCheck";

export default class BoardRenderController {
    boardCanvas: BoardCanvas;
    /**
     * lista de elementos para rendenizar
     */
    private elements: TRenderElement[] = [];

    constructor(boardCanvas: BoardCanvas) {
        this.boardCanvas = boardCanvas;
        this.render();
    }

    placePointerAndRender(cordinates: TBoardCoordinates) {
        this.placePointer(cordinates);
        this.render();
    }

    placePlayer(cordinates: TBoardCoordinates, data: IPlayerRenderData) {
        const player: TRenderElement = {
            cell: cordinates,
            type: 'player',
            data
        };
        this.placeElement(player, e => {
            if (e.type !== 'player' || !isPlayerRenderData(e.data))
                return false;
            return e.data.name === data.name;
        });
    }

    placePointer(cordinates: TBoardCoordinates) {
        const pointer: TRenderElement = {
            cell: cordinates,
            type: 'pointer'
        };
        this.placeElement(pointer, e => e.type === 'pointer');
    }

    private placeElement(element: TRenderElement, indentifyElement: (e: TRenderElement) => boolean) {
        const elementIndex = this.elements.findIndex(indentifyElement);
        if (elementIndex === -1)
            this.elements.push(element);
        else
            this.elements.splice(elementIndex, 1, element);
    }

    renderFunctions: RenderElementsHandlers =  {
        'pointer': pointer => this.boardCanvas.drawPointerCircle(pointer.cell),
        'player': player => this.boardCanvas.drawPlayer(player.cell, player.data)
    }

    render() {
        this.boardCanvas.fillBackground();
        this.boardCanvas.drawGrid();
        for (const element of this.elements)
            this.renderFunctions[element.type](element);
    }
}