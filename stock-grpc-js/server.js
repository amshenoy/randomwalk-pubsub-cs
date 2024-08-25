const grpc = require('@grpc/grpc-js');
const protoLoader = require('@grpc/proto-loader');
const path = require('path');

// Load the protobuf
const PROTO_PATH = path.join(__dirname, '..', 'protos', 'greet.proto');
const packageDefinition = protoLoader.loadSync(PROTO_PATH, {
  keepCase: true,
  longs: String,
  enums: String,
  defaults: true,
  oneofs: true,
});
const greeterProto = grpc.loadPackageDefinition(packageDefinition).greet;

// Implement the SayHello RPC method
function sayHello(call, callback) {
  console.log("Received HelloRequest", call);
  callback(null, { message: `Hello, ${call.request.name}!` });
}

// Create and start the server
const PORT = 5243;
const server = new grpc.Server();
server.addService(greeterProto.Greeter.service, { SayHello: sayHello });
server.bindAsync(`0.0.0.0:${PORT}`, grpc.ServerCredentials.createInsecure(), () => {
  console.log(`Server running at http://0.0.0.0:${PORT}`);
  // server.start(); // No longer needed as of version 1.10.x
});
