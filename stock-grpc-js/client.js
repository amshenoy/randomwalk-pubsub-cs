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


var call = client.GetPrice({ symbol: 'GOOGL' });

call.on('data', (response) => {
  console.log('Received price:', response.symbol, '->', response.price);
});

call.on('error', (error) => {
  console.error('Error:', error);
});

call.on('end', () => {
  console.log('Stream ended.');
});


