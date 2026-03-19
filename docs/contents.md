# Table of Contents

## [1. Introduction](introduction.md)
- [1.1 Relevance of decentralized systems](introduction.md#11-relevance-of-decentralized-systems)
- [1.2 Ethical aspects and prevention of illegal use](#ethical-aspects-and-prevention-of-illegal-use)
- [1.3 Conclusion](#conclusion)

## 2. Theoretical Introduction
- [2.1 Blockchain](#blockchain)
- [2.2 Block](#block)
- [2.3 Transaction](#transaction)
- [2.4 Node](#node)
- [2.5 Cryptocurrency](#cryptocurrency)

## 3. Project Description

### 3.1 TCP Server Architecture
- [3.1.1 Main classes and their interactions](#main-classes-and-their-interactions-server)
    - [3.1.1.1 TcpServer](#tcpserver)
    - [3.1.1.2 TcpRequest](#tcprequest)
    - [3.1.1.3 Response](#response)
    - [3.1.1.4 Controller](#controller)
    - [3.1.1.5 RequestHandlerFactory](#requesthandlerfactory)
    - [3.1.1.6 ServerResponseService](#serverresponseservice)
    - [3.1.1.7 IController](#icontroller)
    - [3.1.1.8 IRequestHandler](#irequesthandler)
    - [3.1.1.9 Request Handlers](#request-handlers)
    - [3.1.1.10 Class Relationships](#class-relationships-server)
- [3.1.2 Conclusion](#conclusion-server)

### 3.2 TCP Client Architecture
- [3.2.1 Main classes and their interactions](#main-classes-and-their-interactions-client)
    - [3.2.1.1 ClientTcp](#clienttcp)
    - [3.2.1.2 IDbProcessor](#idbprocessor)
    - [3.2.1.3 DataSynchronizer](#datasynchronizer)
    - [3.2.1.4 RequestExecutor](#requestexecutor)
    - [3.2.1.5 RequestHandler](#requesthandler)
    - [3.2.1.6 ResponseHandler](#responsehandler)
    - [3.2.1.7 ConnectionManager](#connectionmanager)
    - [3.2.1.8 Request](#request)
    - [3.2.1.9 RequestFactory](#requestfactory)
    - [3.2.1.10 RequestPool](#requestpool)
    - [3.2.1.11 DbProcessor](#dbprocessor)
    - [3.2.1.12 AppDbContext](#appdbcontext)
    - [3.2.1.13 Class Relationships](#class-relationships-client)
- [3.2.2 Conclusion](#conclusion-client)

### 3.3 Blockchain Architecture
- [3.3.1 Main classes and their interactions](#main-classes-and-their-interactions-blockchain)
    - [3.3.1.1 Blockchain](#blockchain-class)
    - [3.3.1.2 Block](#block-class)
    - [3.3.1.3 BlockBuilder](#blockbuilder)
    - [3.3.1.4 BlockManager](#blockmanager)
    - [3.3.1.5 Transaction](#transaction-class)
    - [3.3.1.6 MerkleRoot](#merkleroot)
    - [3.3.1.7 TransactionMemoryPool](#transactionmemorypool)
    - [3.3.1.8 BlockchainService](#blockchainservice)
    - [3.3.1.9 TransactionService](#transactionservice)
    - [3.3.1.10 TransactionManager](#transactionmanager)
- [3.3.2 Conclusion](#conclusion-blockchain)

### 3.4 API Architecture
- [3.4.1 Main classes and their interactions](#main-classes-and-their-interactions-api)
    - [3.4.1.1 Service](#service)
    - [3.4.1.2 MainController](#maincontroller)
    - [3.4.1.3 IRepository](#irepository)
    - [3.4.1.4 Repository](#repository)
- [3.4.2 Conclusion](#conclusion-api)

### 3.5 Protocol Architecture
- [3.5.1 Main classes and their interactions](#main-classes-and-their-interactions-protocol)
    - [3.5.1.1 TerProtocol<T>](#terprotocol)
    - [3.5.1.2 TerPayload<T>](#terpayload)
    - [3.5.1.3 TerHeader](#terheader)
    - [3.5.1.4 IpHelper](#iphelper)
    - [3.5.1.5 RequestSerializer](#requestserializer)
- [3.5.2 Request and data types](#request-types)
    - [BlockRequest](#blockrequest)
    - [InfoSyncRequest](#infosyncrequest)
    - [KeyExchangeRequest](#keyexchangerequest)
    - [PeerInfoRequest](#peerinforequest)
    - [TransactionRequest](#transactionrequest)
- [3.5.3 Interaction description](#interaction)

### 3.6 Mobile Application
- [3.6.1 Main features](#mobile-features)
- [3.6.2 Wallet usage example](#wallet-example)

### 3.7 Desktop Application
- [3.7.1 Node usage example](#node-example)

## 4. Technologies Used
- [4.1 Programming language](#programming-language)
- [4.2 Network technologies](#network-technologies)
- [4.3 Security and cryptography](#security-and-cryptography)
- [4.4 Blockchain and mining](#blockchain-and-mining)
- [4.5 Databases](#databases)
- [4.6 Frameworks and libraries](#frameworks-and-libraries)
- [4.7 Mobile development](#mobile-development)
- [4.8 Version control](#version-control)
- [4.9 Other technologies](#other-technologies)

## 5. Conclusion