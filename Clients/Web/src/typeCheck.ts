import { ICheckNameResponse } from "./interfaces";

export function isCheckNameResponse(value: any): value is ICheckNameResponse {
    return typeof value["accessToken"] === "string" && typeof value["refreshToken"] === "string";
}