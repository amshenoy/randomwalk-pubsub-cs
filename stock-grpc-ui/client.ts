import {
    GrpcWebFetchTransport,
} from "@protobuf-ts/grpcweb-transport";
import {
    StockServiceClient
} from "./generated/stock.client";
import {
    StockStreamRequest,
    StockPriceResponse,
    RequestType,
} from "./generated/stock";

// Initialize the gRPC-Web transport
const transport = new GrpcWebFetchTransport({
    baseUrl: "https://localhost:5243", // Replace with your server URL
});

// Create a client instance for the StockService
const client = new StockServiceClient(transport);

// Function to start streaming stock prices
async function streamStockPrices() {
    // Create a duplex streaming call
    const call = client.priceStream();

    // Set up the listener for responses from the server
    call.responses.onMessage((response: StockPriceResponse) => {
        console.log(`Received price update: ${response.symbol} - $${response.price}`);
    });

    // Send a subscription request for multiple stock symbols
    const request: StockStreamRequest = {
        symbols: ["AAPL", "GOOGL", "MSFT"],
        type: RequestType.SUBSCRIBE,
    };

    call.requests.send(request);

    // Simulate user input to unsubscribe after 10 seconds
    setTimeout(() => {
        const unsubscribeRequest: StockStreamRequest = {
            symbols: ["AAPL", "GOOGL", "MSFT"],
            type: RequestType.UNSUBSCRIBE,
        };
        call.requests.send(unsubscribeRequest);
        console.log("Unsubscribed from stock prices.");
    }, 10000);

    // End the request stream after 12 seconds (assuming we no longer want to receive updates)
    setTimeout(() => {
        call.requests.complete();
        console.log("Stream ended.");
    }, 12000);

    // Wait for the call to finish (it may throw if the server or network fails)
    await call.status;
}

// Start streaming stock prices
streamStockPrices().catch(console.error);
