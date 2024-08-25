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
const client = new stockProto.StockService(endpoint, grpc.credentials.createInsecure());

// client.GetPrice({ symbol: 'GOOGL' }, (error, response) => {
//   if (error) {
//     console.error('Error:', error);
//   } else {
//     console.log('Price response:', response.message);
//   }
// });

// Create a new stream for the bi-directional PriceStream RPC
const call = client.PriceStream((error, response) => {
  if (error) {
    console.error('Error:', error);
  } else {
    console.log('Received price response:', response.symbol, '->', response.price);
  }
});

// Send a subscription request for a stock symbol
call.write({ symbol: 'GOOGL', type: 'SUBSCRIBE' });

// Optional: Send another request (e.g., to unsubscribe)
setTimeout(() => {
  call.write({ symbol: 'GOOGL', type: 'UNSUBSCRIBE' });
  call.write({ symbol: 'MSFT', type: 'SUBSCRIBE' });
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
