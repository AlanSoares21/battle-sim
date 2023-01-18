import { ICheckNameResponse, IPlayerRenderData } from "./interfaces";

export function isCheckNameResponse(value: any): value is ICheckNameResponse {
    return typeof value["token"] === "string";
}

export function isPlayerRenderData(value: any): value is IPlayerRenderData {
    return typeof value["name"] === "string";
}