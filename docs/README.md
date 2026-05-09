# Deveel Messaging

Welcome to the documentation home for `deveel.messaging`.

This site is the practical guide to the framework: start fast, understand the core model, wire connectors, then tune and migrate when you need to.

If you are reading this in GitBook, this page is meant to be the homepage.

## What this framework gives you

- A shared message model for SMS, email, push, and chat
- Strongly-typed endpoints and schema-driven validation
- Connector abstractions that keep provider code isolated
- DI-friendly registration and a channel registry for runtime wiring
- Ready-made connectors for Twilio, SendGrid, Firebase, Facebook Messenger, and Telegram

## What to read first

If you are new here, follow this order:

- [Framework overview](framework-overview.md)
- [Quick start](quick-start.md)
- [Installation and setup](installation-setup.md)

## Core concepts

- [Channel schema usage](channelschema-usage.md)
- [Endpoint types](endpointtype-usage.md)
- [Channel registry guide](channelregistry-guide.md)
- [Connector implementation](channelconnector-usage.md)

## Validation and auth

- [Message validation examples](validatemessage-usage-examples.md)
- [Validation test example](validatemessage-test-example.md)
- [Validation extensions](channelschema-validation-extension-usage.md)
- [Authentication mechanism](authentication-mechanism.md)
- [Enhanced authentication configuration](enhanced-authentication-configuration.md)

## Advanced topics

- [Advanced configuration](advanced-configuration.md)
- [Schema derivation guide](channelschema-derivation-guide.md)

## Connector guides

- [Connector index](connectors/README.md)
- [Twilio SMS connector](connectors/twilio-sms-connector.md)
- [Twilio WhatsApp connector](connectors/twilio-whatsapp-connector.md)
- [SendGrid email connector](connectors/sendgrid-email-connector.md)
- [Firebase push connector](connectors/firebase-push-connector.md)
- [Facebook Messenger connector](connectors/facebook-messenger-connector.md)
- [Telegram bot connector](connectors/telegram-bot-connector.md)

## Suggested reading paths

### First project

1. [Framework overview](framework-overview.md)
2. [Quick start](quick-start.md)
3. [Connector index](connectors/README.md)

### Custom connector authoring

1. [Channel schema usage](channelschema-usage.md)
2. [Connector implementation](channelconnector-usage.md)
3. [Channel registry guide](channelregistry-guide.md)

### Working with existing code

1. [Endpoint types](endpointtype-usage.md)
2. [Message validation examples](validatemessage-usage-examples.md)

## Short version

If you only want the path of least resistance:

1. Read [Framework overview](framework-overview.md)
2. Try [Quick start](quick-start.md)
3. Open one channel guide from [Connector index](connectors/README.md)
