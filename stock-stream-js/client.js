const grpc = require('@grpc/grpc-js');
const protoLoader = require('@grpc/proto-loader');
const path = require('path');

// Load the protobuf
const PROTO_PATH = path.join(__dirname, '..', 'protos', 'stock.proto');
const packageDefinition = protoLoader.loadSync(PROTO_PATH, {
  keepCase: true,
  longs: String,
  enums: String,
  defaults: true,
  oneofs: true,
});
const stockProto = grpc.loadPackageDefinition(packageDefinition).stock;

const PORT = 5243;
const endpoint = `localhost:${PORT}`;

// This will not work when SSL is required
const client = new stockProto.StockService(endpoint, grpc.credentials.createInsecure());
// const client = new stockProto.StockService(endpoint, grpc.credentials.createSsl());

// client.GetPrice({ symbol: 'GOOGL' }, (error, response) => error ? null : response.message);

// Create a new stream for the bi-directional PriceStream RPC
const call = client.PriceStream((error, response) => {
  if (error) {
    console.error('Error:', error);
  } else {
    console.log('Received price response:', response.symbol, '->', response.price);
  }
});

// Send a subscription request for a stock symbol
call.write({ symbols: ['GOOGL'], type: 'SUBSCRIBE' });
call.write({ symbols: ['AMZN'], type: 'SUBSCRIBE' });

// Optional: Send another request (e.g., to unsubscribe)
setTimeout(() => {
  call.write({ symbols: ['GOOGL'], type: 'UNSUBSCRIBE' });
  call.write({ symbols: ['MSFT'], type: 'SUBSCRIBE' });
  call.write({ symbols: ['AAPL'], type: 'SUBSCRIBE' });
}, 10000); // Adjust the delay as needed


call.on('data', (response) => {
  console.log('Received price:', response.symbol, '->', response.price);
});

call.on('error', (error) => {
  console.error('Error:', error);
});

call.on('end', () => {
  console.log('Stream ended.');
});

// setTimeout(() => { call.end(); }, 15000);
