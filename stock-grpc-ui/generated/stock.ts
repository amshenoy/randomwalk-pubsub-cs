// @generated by protobuf-ts 2.9.4
// @generated from protobuf file "stock.proto" (package "stock", syntax proto3)
// tslint:disable
import { ServiceType } from "@protobuf-ts/runtime-rpc";
import type { BinaryWriteOptions } from "@protobuf-ts/runtime";
import type { IBinaryWriter } from "@protobuf-ts/runtime";
import { WireType } from "@protobuf-ts/runtime";
import type { BinaryReadOptions } from "@protobuf-ts/runtime";
import type { IBinaryReader } from "@protobuf-ts/runtime";
import { UnknownFieldHandler } from "@protobuf-ts/runtime";
import type { PartialMessage } from "@protobuf-ts/runtime";
import { reflectionMergePartial } from "@protobuf-ts/runtime";
import { MessageType } from "@protobuf-ts/runtime";
/**
 * @generated from protobuf message stock.StockPriceRequest
 */
export interface StockPriceRequest {
    /**
     * @generated from protobuf field: string symbol = 1;
     */
    symbol: string;
}
/**
 * @generated from protobuf message stock.StockStreamRequest
 */
export interface StockStreamRequest {
    /**
     * @generated from protobuf field: repeated string symbols = 1;
     */
    symbols: string[];
    /**
     * @generated from protobuf field: stock.RequestType type = 2;
     */
    type: RequestType;
}
/**
 * @generated from protobuf message stock.StockPriceResponse
 */
export interface StockPriceResponse {
    /**
     * @generated from protobuf field: string symbol = 1;
     */
    symbol: string;
    /**
     * @generated from protobuf field: double price = 2;
     */
    price: number;
}
/**
 * @generated from protobuf enum stock.RequestType
 */
export enum RequestType {
    /**
     * @generated from protobuf enum value: SUBSCRIBE = 0;
     */
    SUBSCRIBE = 0,
    /**
     * @generated from protobuf enum value: UNSUBSCRIBE = 1;
     */
    UNSUBSCRIBE = 1
}
// @generated message type with reflection information, may provide speed optimized methods
class StockPriceRequest$Type extends MessageType<StockPriceRequest> {
    constructor() {
        super("stock.StockPriceRequest", [
            { no: 1, name: "symbol", kind: "scalar", T: 9 /*ScalarType.STRING*/ }
        ]);
    }
    create(value?: PartialMessage<StockPriceRequest>): StockPriceRequest {
        const message = globalThis.Object.create((this.messagePrototype!));
        message.symbol = "";
        if (value !== undefined)
            reflectionMergePartial<StockPriceRequest>(this, message, value);
        return message;
    }
    internalBinaryRead(reader: IBinaryReader, length: number, options: BinaryReadOptions, target?: StockPriceRequest): StockPriceRequest {
        let message = target ?? this.create(), end = reader.pos + length;
        while (reader.pos < end) {
            let [fieldNo, wireType] = reader.tag();
            switch (fieldNo) {
                case /* string symbol */ 1:
                    message.symbol = reader.string();
                    break;
                default:
                    let u = options.readUnknownField;
                    if (u === "throw")
                        throw new globalThis.Error(`Unknown field ${fieldNo} (wire type ${wireType}) for ${this.typeName}`);
                    let d = reader.skip(wireType);
                    if (u !== false)
                        (u === true ? UnknownFieldHandler.onRead : u)(this.typeName, message, fieldNo, wireType, d);
            }
        }
        return message;
    }
    internalBinaryWrite(message: StockPriceRequest, writer: IBinaryWriter, options: BinaryWriteOptions): IBinaryWriter {
        /* string symbol = 1; */
        if (message.symbol !== "")
            writer.tag(1, WireType.LengthDelimited).string(message.symbol);
        let u = options.writeUnknownFields;
        if (u !== false)
            (u == true ? UnknownFieldHandler.onWrite : u)(this.typeName, message, writer);
        return writer;
    }
}
/**
 * @generated MessageType for protobuf message stock.StockPriceRequest
 */
export const StockPriceRequest = new StockPriceRequest$Type();
// @generated message type with reflection information, may provide speed optimized methods
class StockStreamRequest$Type extends MessageType<StockStreamRequest> {
    constructor() {
        super("stock.StockStreamRequest", [
            { no: 1, name: "symbols", kind: "scalar", repeat: 2 /*RepeatType.UNPACKED*/, T: 9 /*ScalarType.STRING*/ },
            { no: 2, name: "type", kind: "enum", T: () => ["stock.RequestType", RequestType] }
        ]);
    }
    create(value?: PartialMessage<StockStreamRequest>): StockStreamRequest {
        const message = globalThis.Object.create((this.messagePrototype!));
        message.symbols = [];
        message.type = 0;
        if (value !== undefined)
            reflectionMergePartial<StockStreamRequest>(this, message, value);
        return message;
    }
    internalBinaryRead(reader: IBinaryReader, length: number, options: BinaryReadOptions, target?: StockStreamRequest): StockStreamRequest {
        let message = target ?? this.create(), end = reader.pos + length;
        while (reader.pos < end) {
            let [fieldNo, wireType] = reader.tag();
            switch (fieldNo) {
                case /* repeated string symbols */ 1:
                    message.symbols.push(reader.string());
                    break;
                case /* stock.RequestType type */ 2:
                    message.type = reader.int32();
                    break;
                default:
                    let u = options.readUnknownField;
                    if (u === "throw")
                        throw new globalThis.Error(`Unknown field ${fieldNo} (wire type ${wireType}) for ${this.typeName}`);
                    let d = reader.skip(wireType);
                    if (u !== false)
                        (u === true ? UnknownFieldHandler.onRead : u)(this.typeName, message, fieldNo, wireType, d);
            }
        }
        return message;
    }
    internalBinaryWrite(message: StockStreamRequest, writer: IBinaryWriter, options: BinaryWriteOptions): IBinaryWriter {
        /* repeated string symbols = 1; */
        for (let i = 0; i < message.symbols.length; i++)
            writer.tag(1, WireType.LengthDelimited).string(message.symbols[i]);
        /* stock.RequestType type = 2; */
        if (message.type !== 0)
            writer.tag(2, WireType.Varint).int32(message.type);
        let u = options.writeUnknownFields;
        if (u !== false)
            (u == true ? UnknownFieldHandler.onWrite : u)(this.typeName, message, writer);
        return writer;
    }
}
/**
 * @generated MessageType for protobuf message stock.StockStreamRequest
 */
export const StockStreamRequest = new StockStreamRequest$Type();
// @generated message type with reflection information, may provide speed optimized methods
class StockPriceResponse$Type extends MessageType<StockPriceResponse> {
    constructor() {
        super("stock.StockPriceResponse", [
            { no: 1, name: "symbol", kind: "scalar", T: 9 /*ScalarType.STRING*/ },
            { no: 2, name: "price", kind: "scalar", T: 1 /*ScalarType.DOUBLE*/ }
        ]);
    }
    create(value?: PartialMessage<StockPriceResponse>): StockPriceResponse {
        const message = globalThis.Object.create((this.messagePrototype!));
        message.symbol = "";
        message.price = 0;
        if (value !== undefined)
            reflectionMergePartial<StockPriceResponse>(this, message, value);
        return message;
    }
    internalBinaryRead(reader: IBinaryReader, length: number, options: BinaryReadOptions, target?: StockPriceResponse): StockPriceResponse {
        let message = target ?? this.create(), end = reader.pos + length;
        while (reader.pos < end) {
            let [fieldNo, wireType] = reader.tag();
            switch (fieldNo) {
                case /* string symbol */ 1:
                    message.symbol = reader.string();
                    break;
                case /* double price */ 2:
                    message.price = reader.double();
                    break;
                default:
                    let u = options.readUnknownField;
                    if (u === "throw")
                        throw new globalThis.Error(`Unknown field ${fieldNo} (wire type ${wireType}) for ${this.typeName}`);
                    let d = reader.skip(wireType);
                    if (u !== false)
                        (u === true ? UnknownFieldHandler.onRead : u)(this.typeName, message, fieldNo, wireType, d);
            }
        }
        return message;
    }
    internalBinaryWrite(message: StockPriceResponse, writer: IBinaryWriter, options: BinaryWriteOptions): IBinaryWriter {
        /* string symbol = 1; */
        if (message.symbol !== "")
            writer.tag(1, WireType.LengthDelimited).string(message.symbol);
        /* double price = 2; */
        if (message.price !== 0)
            writer.tag(2, WireType.Bit64).double(message.price);
        let u = options.writeUnknownFields;
        if (u !== false)
            (u == true ? UnknownFieldHandler.onWrite : u)(this.typeName, message, writer);
        return writer;
    }
}
/**
 * @generated MessageType for protobuf message stock.StockPriceResponse
 */
export const StockPriceResponse = new StockPriceResponse$Type();
/**
 * @generated ServiceType for protobuf service stock.StockService
 */
export const StockService = new ServiceType("stock.StockService", [
    { name: "GetPrice", options: {}, I: StockPriceRequest, O: StockPriceResponse },
    { name: "PriceStream", serverStreaming: true, clientStreaming: true, options: {}, I: StockStreamRequest, O: StockPriceResponse }
]);
