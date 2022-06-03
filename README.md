# About

This repository contains an experiment where I try to use the contract testing tool [Pact Net](https://github.com/pact-foundation/pact-net) together with the typesafe REST client [refit](https://github.com/reactiveui/refit) to generate automated contract testing for API-dependencies (by way of using some reflectiond dark magic 🧙‍♂️).

The full example is in the [ PactGenerator file ](./Consumer.Tests/PactGenerator.cs) 

# TODO
 
 - [ ] Use Bogus (or antoher faker implementation) to specify default values for DTO-objects
 - [ ] Use a tool to test the pactfile against the apis swagger.json