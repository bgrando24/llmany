## Get started tutorial

https://learn.microsoft.com/en-us/aspnet/core/tutorials/first-web-api?view=aspnetcore-9.0&tabs=visual-studio-code

## General Architecture

Controller → Service → Connector

### Controller

Handles the HTTP endpoints, in terms of exposing an endpoint requests can be sent to, and responding to those requests appropriately.

### Service

The 'business logic' layer, i.e. what the application is actually meant to do.

### Connector

Handles integrating/consuming external services (in this case, the LLM API services).
It acts as a layer of abstraction between the application's models/logic, and the specific logic related to the external service.
