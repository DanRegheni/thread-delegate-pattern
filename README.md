# Thread-Delegate-Pattern in Event-Driven Architectures

## Overview
Explore the implementation of the Thread-Delegate Pattern in event-driven architectures for ordered, context-specific message processing using C#. This repository provides a practical demonstration, 
inspired by Mark Richards (https://www.youtube.com/watch?v=ZM0IFSToceU), showcasing how to ensure that messages within a specific context are processed sequentially in a multi-threaded environment.

## Description
This GitHub repository is a deep dive into utilizing the Thread-Delegate Pattern within event-driven architectures, focusing on maintaining the order of message processing based on their context. It's particularly useful for systems where the sequence and context of message processing are critical for data integrity and consistency.

## Key Components
- **Dispatcher**: Central to the application, it routes incoming messages to the appropriate `TradeProcessor` based on the message context and load balancing.
- **TradeProcessor**: Handles the processing of messages, ensuring that each message is processed in the order it was received within its context.
- **ConcurrentDictionaries**: Utilized for thread-safe operations, managing mappings between contexts and threads (`allocationMap`) and tracking the workload of each thread (`processingCountMap`).

## Inspired by Mark Richards
This implementation is based on the example provided by Mark Richards, offering a concrete application of theoretical concepts in a real-world scenario.

## Usage
The repository serves as an educational tool for developers and architects aiming to implement or understand the Thread-Delegate Pattern in their event-driven systems, ensuring that messages are processed efficiently, in order, and contextually.

