# Termite_2.0

Termite is a decentralized blockchain system for performing digital financial transactions without the need for a central intermediary.

The project implements its own blockchain, P2P network, and cryptographic security to demonstrate the principles of modern decentralized systems.

---

## Main Features

### Blockchain
- Custom blockchain implementation  
- SHA-256 hashing  
- Merkle Tree for transaction validation  
- Consensus: Proof of Work (PoW)  
- Block mining  
- Block rewards and transaction fees  

### P2P Network
- Communication between nodes via TCP/IP  
- Custom protocol (TerProtocol)  
- Automatic data synchronization  
- Peer (node) management  
- Encrypted communication  

### Transactions
- Digital signatures (RSA)  
- Transaction validation in the network  
- Memory pool for pending transactions  
- Support for transaction fees  

### Mobile Application (Wallet)
- Wallet creation and management  
- Sending and receiving cryptocurrency  
- QR codes for addresses  
- Transaction history  

### Desktop Application (Node)
- Running a network node  
- Block mining  
- Blockchain monitoring  
- Connection management  

---

## Architecture

The project is divided into several modules:

### TCP Server
- Handles incoming requests  
- Controller + Request Handlers  
- Request queue  

### TCP Client
- Connection to nodes  
- Data synchronization  
- Request/Response system  

### Blockchain Module
- Block and transaction management  
- Mining (PoW)  
- Merkle Tree  

### API
- REST interface for external applications  

**Operations:**
- create address  
- send transaction  
- check balance  

### Protocol (TerProtocol)
- Structured communication (Header + Payload)  
- JSON serialization  
- Support for different message types  

---

## Technologies

### Backend
- C#  
- .NET  

### Networking
- TCP/IP  
- Web API  

### Security
- RSA (digital signatures)  
- SHA-256 (hashing)  

### Databases
- SQLite  
- Redis  

### Frameworks
- Entity Framework Core  

### Tools
- Git  
- GitHub  

---

## Project Status

The project is under active development and serves as a demonstration of a decentralized system architecture.

### Current limitations:
- network communication is not fully stable  
- synchronization between nodes may be inconsistent  
- some parts of the system are not optimized  

Despite this, the project includes working implementations of:
- blockchain  
- transaction model  
- cryptographic protection  
- basic P2P communication  

---

## Documentation

Full documentation is available in the repository.

👉 You can find detailed documentation here: [[link to documentation folder](https://github.com/KyryloShestakov/Termite_2.0/blob/main/docs/contents.md)]

This folder contains:
- architecture details  
- protocol description  
- API documentation  
- setup and configuration guides  
- technical explanations of all modules  

---

## Running the Project

### Node
1. Open the project in your IDE (Visual Studio / Rider)  
2. Run the application  
3. Start the node (Start Node)  
4. The node will connect to the network and begin synchronization  

### Mobile Application
1. Open the project in Android Studio  
2. Run the application on a device or emulator  
3. Create or import a wallet  
4. Connect to the node API  

---

## Security

- Encrypted communication between nodes  
- Digital transaction signatures  
- Block hashing  
- Decentralized validation  

---

## Future Development

Possible improvements:
- more stable P2P network  
- alternative consensus (e.g. PoS)  
- smart contracts  
- system scaling  
- web interface  

---

## Author

Kyrylo Shestakov  

---

## Disclaimer

This project is not intended for production use.  
It is an experimental implementation of a decentralized system.

---

## To run the system

1. Start the node (desktop application)  
2. Run the API  
3. Connect the mobile application  

Network and node configuration is done manually.
