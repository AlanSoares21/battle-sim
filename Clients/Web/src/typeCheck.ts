import { ILoginResponse } from "./interfaces";

export function isLoginResponse(value: any): value is ILoginResponse {
    return typeof value["accessToken"] === "string" && typeof value["refreshToken"] === "string";
}