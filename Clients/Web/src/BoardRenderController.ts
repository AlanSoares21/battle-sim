import BoardCanvas from "./BoardCanvas";
import { IPlayerRenderData, RenderElementsHandlers, TBoard, TBoardCoordinates, TRenderElement } from "./interfaces";
import { isPlayerRenderData } from "./typeCheck";

function getMiddleCoordinates(board: TBoard): TBoardCoordinates {
    return {
        x: board.width / 2,
        y: board.height / 2
    }
}

export default class BoardRenderController {
    boardCanvas: BoardCanvas;
    /**
     * lista de elementos para rendenizar
     */
    private elements: TRenderElement[] = [];

    constructor(board: TBoard, boardCanvas: BoardCanvas) {
        this.boardCanvas = boardCanvas;
        this.placePointer(getMiddleCoordinates(board));
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