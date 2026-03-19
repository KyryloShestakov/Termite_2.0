# 4 Used Technologies

During the development of the decentralized network node and user applications, the following technologies were used:

---

## 4.1 Programming Language

- **C#** — the main programming language used for developing the server and client parts of the node

---

## 4.2 Network Technologies

- **TCP/IP** — used for communication between network nodes
- **Web API** — enables communication with other nodes via HTTP requests

---

## 4.3 Security and Cryptography

- **RSA** — used for data encryption and secure connections
- **SHA-256, SHA-3** — hashing algorithms used to ensure data integrity and blockchain security
- **RSA for transaction signing** — used together with SHA-256 for signing transactions in the cryptocurrency wallet

---

## 4.4 Blockchain and Mining

### Blockchain

**Blockchain** is a decentralized and distributed ledger used for storing transactions. Each block:
- contains transaction data
- is linked to the previous block using a hash function

This structure ensures:
- transparency
- data immutability
- security through cryptography and consensus mechanisms

---

### Proof-of-Work (PoW)

**Proof-of-Work** is a consensus algorithm used to validate transactions and create new blocks.

In this system:
- nodes (miners) compete to solve a mathematical problem
- the problem involves finding a correct **nonce**
- the solution must meet a required difficulty level

---

### Rewards and Incentives

Miners who successfully solve the problem and add a new block receive rewards consisting of:
- **block reward** — newly created coins
- **transaction fees** — fees from transactions included in the block

---

### Mining Process

Mining requires:
- high computational power
- significant energy consumption

It ensures:
- creation of new blocks
- protection of the network against fraud and manipulation

---

## 4.5 Databases

- **SQLite** — lightweight relational database used for storing blockchain data, nodes, and transactions
- **Redis** — used for caching data and storing temporary session keys

---

## 4.6 Frameworks and Libraries

- **Entity Framework Core** — ORM for database operations
- **Avalonia UI** — cross-platform framework for building the user interface
- **Newtonsoft.Json** — library for JSON serialization and deserialization

---

## 4.7 Version Control

- **Git** — version control system
- **GitHub** — platform for repository storage and team collaboration

---

## 4.8 Additional Technologies

- **QR Codes** — used for easy transaction processing (generation and scanning)
- **Logging** — system for tracking node activity and analyzing events

---